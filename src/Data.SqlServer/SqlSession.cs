using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace DevZest.Data.SqlServer
{
    public abstract partial class SqlSession : DbSession<SqlConnection, SqlTransaction, SqlCommand, SqlReader>
    {
        protected SqlSession(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private SqlVersion _sqlVersion;
        public SqlVersion SqlVersion
        {
            get
            {
                if (_sqlVersion == 0)
                    _sqlVersion = GetConnection().GetSqlVersion();
                return _sqlVersion;
            }
            set { _sqlVersion = value; }
        }

        internal ConditionalWeakTable<DbQueryStatement, SqlGenerator> _sqlGeneratorCache = new ConditionalWeakTable<DbQueryStatement, SqlGenerator>();

        protected override string GetSqlString(DbQueryStatement query)
        {
            return SqlGenerator.Select(this, query).CreateCommand(null).ToTraceString();
        }

        protected sealed override DbTransactionInvoker<SqlConnection, SqlTransaction> CreateTransactionInvoker(IsolationLevel isolationLevel)
        {
            return new SqlTransactionInvoker(this, GetConnection(), isolationLevel);
        }

        protected sealed override SqlCommand GetQueryCommand(DbQueryStatement queryStatement)
        {
            var queryGenerator = SqlGenerator.Select(this, queryStatement);
            return queryGenerator.CreateCommand(GetConnection());
        }

        protected sealed override DbReaderInvoker<SqlCommand, SqlReader> CreateReaderInvoker(SqlCommand command, Model model)
        {
            return new SqlReaderInvoker(this, command, model);
        }

        ConditionalWeakTable<Model, string> _tempTableNamesByModel = new ConditionalWeakTable<Model, string>();
        Dictionary<string, int> _tempTableNameSuffixes = new Dictionary<string, int>();

        private string GetUniqueTempTableName(Model model)
        {
            Debug.Assert(model != null);

            return "#" + _tempTableNameSuffixes.GetUniqueName(model.GetDbAlias());
        }

        protected override string AssignTempTableName(Model model)
        {
            return _tempTableNamesByModel.GetValue(model, GetUniqueTempTableName);
        }

        protected sealed override SqlCommand GetCreateTableCommand(Model model, string tableName, bool isTempTable)
        {
            var sqlBuilder = new IndentedStringBuilder();
            model.GenerateCreateTableSql(sqlBuilder, SqlVersion, tableName, isTempTable);
            return sqlBuilder.ToString().CreateSqlCommand(this.GetConnection());
        }

        public DbTable<SqlXmlModel> Nodes(SqlXml xml, string xPath)
        {
            var param = _SqlXml.Param(xml);
            var model = new SqlXmlModel();
            model.Initialize(param, xPath);
            return model.CreateDbTable(this, "sys_xml_nodes");
        }

        private const string XML_ROOT_TAG_NAME = "root";
        private const string XML_ROW_TAG_NAME = "row";
        private const string XML_ROW_XPATH = "/" + XML_ROOT_TAG_NAME + "/" + XML_ROW_TAG_NAME;
        private const string XML_COL_TAG_NAME = "col_{0}";
        private const string XML_COL_TAG_XPATH = XML_COL_TAG_NAME + "[1]/text()[1]";

        private static string GetColumnTagXPath(int index)
        {
            return string.Format(NumberFormatInfo.InvariantInfo, XML_COL_TAG_XPATH, index);
        }

        private SqlXml GetSqlXml<T>(DataSet<T> dataSet)
            where T : Model, new()
        {
            using (var stream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "  ",
                    Encoding = Encoding.UTF8,
                }))
                {
                    xmlWriter.WriteStartElement(XML_ROOT_TAG_NAME);

                    var columns = dataSet._.GetColumns();
                    foreach (var row in dataSet)
                    {
                        xmlWriter.WriteStartElement(XML_ROW_TAG_NAME);

                        foreach (var column in columns)
                        {
                            xmlWriter.WriteStartElement(string.Format(CultureInfo.InvariantCulture, XML_COL_TAG_NAME, column.Ordinal));
                            xmlWriter.WriteString(column.GetMapper().GetXmlValue(row.Ordinal, SqlVersion));
                            xmlWriter.WriteEndElement();
                        }

                        // Write an extra column as the sequential id (row.Ordinal + 1) of current row
                        xmlWriter.WriteStartElement(string.Format(CultureInfo.InvariantCulture, XML_COL_TAG_NAME, columns.Count));
                        xmlWriter.WriteString((row.Ordinal + 1).ToString(NumberFormatInfo.InvariantInfo));
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.Flush();
                }

                stream.Position = 0;
                using (var xmlReader = XmlReader.Create(stream))
                {
                    return new SqlXml(xmlReader);
                }
            }
        }

        protected sealed override DbSet<T> CreateDbSet<T>(DataSet<T> dataSet)
        {
            return CreateDbSet<T>(GetSqlXml(dataSet));
        }

        protected sealed override Task<DbSet<T>> CreateDbSetAsync<T>(DataSet<T> dataSet, CancellationToken cancellationToken)
        {
            return Task.FromResult(CreateDbSet(dataSet));
        }

        private DbSet<T> CreateDbSet<T>(SqlXml xml)
            where T : Model, new()
        {
            return this.CreateQuery<T>((builder, model) =>
            {
                model.GetDataSetOrdinalColumn(createIfNotExist: true);

                var sourceTable = Nodes(xml, XML_ROW_XPATH);
                SqlXmlModel xmlModel;
                builder.From(sourceTable, out xmlModel);
                var columns = model.GetColumns();
                for (int i = 0; i < columns.Count; i++)
                {
                    var targetColumn = columns[i];
                    builder.SelectColumn(xmlModel.Xml.ValueColumn(GetColumnTagXPath(i), targetColumn), targetColumn);
                }
                builder.OrderBy(builder.SelectList[columns.Count - 1].Source.Asc());
            });
        }

        protected sealed override SqlCommand GetInsertCommand(DbSelectStatement statement)
        {
            return GetInsertCommand(statement, null);
        }

        protected sealed override int Insert<T, TSource>(DbTable<T> targetTable, DbSet<TSource> sourceData, DbSelectStatement statement, DbTable<IdentityMapping> identityMappings)
        {
            if (identityMappings == null)
                return ExecuteNonQuery(GetInsertCommand(statement));

            var tempTable = this.CreateTempTable<T>(GetTempTableInitializer(sourceData), true);
            var identityOutput = this.CreateTempTable<IdentityOutput>(null, true);

            var commands = GetInsertCommands(statement, sourceData, tempTable, identityOutput, identityMappings);
            int result = 0;
            foreach (var command in commands)
            {
                result = ExecuteNonQuery(command);
                if (result == 0)
                    break;
            }
            return result;
        }

        internal static Action<Model> GetTempTableInitializer<TSource>(DbSet<TSource> sourceData)
            where TSource : Model, new()
        {
            var hasDataSetOrdinal = !ReferenceEquals(sourceData.Model.GetDataSetOrdinalColumn(), null);
            if (hasDataSetOrdinal)
                return (Model x) => x.GetDataSetOrdinalColumn(createIfNotExist: true);
            else
                return null;
        }

        protected sealed override async Task<int> InsertAsync<T, TSource>(DbTable<T> targetTable, DbSet<TSource> sourceData, DbSelectStatement statement, DbTable<IdentityMapping> identityMappings, CancellationToken cancellationToken)
        {
            if (identityMappings == null)
                return await ExecuteNonQueryAsync(GetInsertCommand(statement), cancellationToken);

            var tempTable = await this.CreateTempTableAsync<T>(GetTempTableInitializer(sourceData), true, cancellationToken);
            var identityOutput = await this.CreateTempTableAsync<IdentityOutput>(null, true, cancellationToken);

            var commands = GetInsertCommands(statement, sourceData, tempTable, identityOutput, identityMappings);
            int result = 0;
            foreach (var command in commands)
            {
                result = await ExecuteNonQueryAsync(command, cancellationToken);
                if (result == 0)
                    break;
            }
            return result;
        }

        internal SqlCommand GetInsertCommand(DbSelectStatement statement, DbTable<IdentityOutput> identityOutput = null)
        {
            return SqlGenerator.Insert(this, statement, identityOutput).CreateCommand(GetConnection());
        }

        internal IList<SqlCommand> GetInsertCommands<T, TSource>(DbSelectStatement statement, DbSet<TSource> sourceData, DbTable<T> tempTable, DbTable<IdentityOutput> identityOutput, DbTable<IdentityMapping> identityMappings)
            where T : Model, new()
            where TSource : Model, new()
        {
            var result = new SqlCommand[3];

            result[0] = GetInsertCommand(BuildSourceDataToTempTableStatement(statement, sourceData, tempTable));
            result[1] = GetInsertCommand(BuildInsertFromTempTableStatement(statement, tempTable), identityOutput);
            result[2] = GetInsertCommand(BuildInsertIntoIdentityMappingsStatement(tempTable, identityOutput, identityMappings));
            return result;
        }

        private static DbSelectStatement BuildSourceDataToTempTableStatement<T, TSource>(DbSelectStatement statement, DbSet<TSource> sourceData, DbTable<T> tempTable)
            where T : Model, new()
            where TSource : Model, new()
        {
            var tempTableModel = tempTable.Model;
            var tempTableColumns = tempTableModel.GetColumns();

            var select = new List<ColumnMapping>();
            foreach (var mapping in statement.Select)
                select.Add(tempTableColumns[mapping.Target.Ordinal].From(mapping.Source));

            var sourceModel = sourceData.Model;

            var identityColumn = tempTableModel.GetIdentity(false).Column;
            Debug.Assert(!ReferenceEquals(identityColumn, null));
            select.Add(identityColumn.From(sourceData.GetSourceColumn(sourceModel.GetIdentity(false).Column)));

            var dataSetOrdinalColumn = tempTableModel.GetDataSetOrdinalColumn();
            if (!ReferenceEquals(dataSetOrdinalColumn, null))
                select.Add(dataSetOrdinalColumn.From(sourceData.GetSourceColumn(sourceModel.GetDataSetOrdinalColumn())));

            return new DbSelectStatement(tempTableModel, select, statement.From, statement.Where, statement.OrderBy, statement.Offset, statement.Fetch);
        }

        private static DbSelectStatement BuildInsertFromTempTableStatement<T>(DbSelectStatement statement, DbTable<T> tempTable)
            where T : Model, new()
        {
            var tempTableModel = tempTable.Model;
            var tempTableColumns = tempTableModel.GetColumns();

            var select = new List<ColumnMapping>();
            foreach (var mapping in statement.Select)
            {
                var target = mapping.Target;
                select.Add(target.From(tempTableColumns[target.Ordinal]));
            }

            var from = tempTable.FromClause;

            var orderBy = new DbExpressionSort[] {
                new DbExpressionSort(tempTableModel.GetIdentity(true).Column.DbExpression, SortDirection.Ascending)
            };

            return new DbSelectStatement(statement.Model, select, from, null, orderBy, -1, -1);
        }

        private static DbSelectStatement BuildInsertIntoIdentityMappingsStatement<T>(DbTable<T> tempTable, DbTable<IdentityOutput> identityOutput, DbTable<IdentityMapping> identityMappings)
            where T : Model, new()
        {
            var tempTableModel = tempTable.Model;
            var tempTableRowId = tempTableModel.GetIdentity(true).Column;
            var oldValue = tempTableModel.GetIdentity(false).Column;
            var dataSetOrdinal = tempTableModel.GetDataSetOrdinalColumn();

            var outputModel = identityOutput._;
            var outputRowId = outputModel.GetIdentity(true).Column;
            var newValue = outputModel.NewValue;

            var resultModel = identityMappings._;
            var select = new List<ColumnMapping>();
            select.Add(resultModel.OldValue.From(oldValue));
            select.Add(resultModel.NewValue.From(newValue));
            if (!ReferenceEquals(dataSetOrdinal, null))
                select.Add(resultModel.DataSetOrdinal.From(dataSetOrdinal));

            var from = new DbJoinClause(DbJoinKind.InnerJoin, tempTable.FromClause, identityOutput.FromClause,
                new ReadOnlyCollection<ColumnMapping>(new ColumnMapping[] { tempTableRowId.From(outputRowId) }));

            return new DbSelectStatement(resultModel, select, from, null, null, -1, -1);
        }

        protected sealed override SqlCommand GetInsertScalarCommand(DbSelectStatement statement, bool outputIdentity)
        {
            return SqlGenerator.InsertScalar(this, statement, outputIdentity).CreateCommand(GetConnection());
        }

        protected sealed override SqlCommand GetUpdateCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Update(this, statement).CreateCommand(GetConnection());
        }

        protected sealed override SqlCommand GetDeleteCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Delete(this, statement).CreateCommand(GetConnection());
        }

        protected sealed override object CreateMockDb()
        {
            var sqlCommandText =
@"declare @id int

set @id = (select max(cast(right(name, len(name) - 4) AS int)) from tempdb.sys.schemas where name like 'Mock%');
set @id = isnull(@id, 0) + 1;

declare @mockschema nvarchar(24) = N'Mock' + cast(@id as nvarchar(20));
declare @sql nvarchar(50) = N'create schema ' + @mockschema;
exec tempdb..sp_executesql @sql;
select @mockschema;
";
            var sqlCommand = new SqlCommand(sqlCommandText, GetConnection());
            return sqlCommand.ExecuteScalar();
        }

        private void CreateMockSchema(string mockSchema)
        {
            var sqlCommandText = string.Format(@"exec tempdb..sp_executesql N'create schema {0}';", mockSchema);
            var sqlCommand = new SqlCommand(sqlCommandText, GetConnection());
            ExecuteNonQuery(sqlCommand);
        }

        protected sealed override string GetMockTableName(string tableName, object tag)
        {
            var quotedTableName = string.Join(".", tableName.ParseIdentifier()).ToQuotedIdentifier();
            return string.Format(CultureInfo.InvariantCulture, "[tempdb].[{0}].{1}", tag, quotedTableName);
        }
    }
}
