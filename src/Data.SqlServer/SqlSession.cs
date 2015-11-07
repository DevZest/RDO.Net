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

        internal DbQuery<T> GetDbQuery<T>(DataSet<T> dataSet)
            where T : Model, new()
        {
            var xml = GetSqlXml(dataSet);
            return CreateQuery<T>((builder, model) =>
            {
                var dataSetOrdinalColumn = new _Int32();
                model.AddSystemColumn(dataSetOrdinalColumn, "sys_dataset_ordinal");

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

        protected override int Insert<T, TSource>(DbTable<T> targetTable, DataSet<TSource> sourceData, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder, bool autoJoin)
        {
            var statement = targetTable.BuildInsertStatement(GetDbQuery(sourceData), columnMappingsBuilder, autoJoin);
            return ExecuteNonQuery(GetInsertCommand(statement));
        }

        protected override Task<int> InsertAsync<T, TSource>(DbTable<T> targetTable, DataSet<TSource> sourceData, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder, bool autoJoin, CancellationToken cancellationToken)
        {
            var statement = targetTable.BuildInsertStatement(GetDbQuery(sourceData), columnMappingsBuilder, autoJoin);
            return ExecuteNonQueryAsync(GetInsertCommand(statement), cancellationToken);
        }

        protected sealed override int Insert<T, TSource>(DbTable<T> targetTable, DbTable<TSource> sourceData, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder, bool autoJoin, DbTable<IdentityMapping> identityMappings)
        {
            var statement = targetTable.BuildInsertStatement(sourceData, columnMappingsBuilder, autoJoin);
            if (identityMappings == null)
                return ExecuteNonQuery(GetInsertCommand(statement));

            var identityOutput = this.CreateTempTable<IdentityOutput>(null, true);
            var result = ExecuteNonQuery(GetInsertCommand(statement, identityOutput));
            ExecuteNonQuery(GetInsertIntoIdentityMappingsCommand(sourceData, identityMappings, autoJoin ? targetTable : null));
            ExecuteNonQuery(GetUpdateIdentityMappingsCommand(identityMappings, identityOutput));
            return result;
        }

        protected sealed override async Task<int> InsertAsync<T, TSource>(DbTable<T> targetTable, DbTable<TSource> sourceData, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder, bool autoJoin, DbTable<IdentityMapping> identityMappings, CancellationToken cancellationToken)
        {
            var statement = targetTable.BuildInsertStatement(sourceData, columnMappingsBuilder, autoJoin);
            if (identityMappings == null)
                return await ExecuteNonQueryAsync(GetInsertCommand(statement), cancellationToken);

            var identityOutput = await this.CreateTempTableAsync<IdentityOutput>(null, true, cancellationToken);
            var result = await ExecuteNonQueryAsync(GetInsertCommand(statement, identityOutput), cancellationToken);
            await ExecuteNonQueryAsync(GetInsertIntoIdentityMappingsCommand(sourceData, identityMappings, autoJoin ? targetTable : null), cancellationToken);
            await ExecuteNonQueryAsync(GetUpdateIdentityMappingsCommand(identityMappings, identityOutput), cancellationToken);
            return result;
        }

        internal SqlCommand GetInsertCommand(DbSelectStatement statement, DbTable<IdentityOutput> identityOutput = null)
        {
            return SqlGenerator.Insert(this, statement, identityOutput).CreateCommand(GetConnection());
        }

        internal SqlCommand GetInsertIntoIdentityMappingsCommand<T, TSource>(DbTable<TSource> sourceData, DbTable<IdentityMapping> identityMappings, DbTable<T> targetTable)
            where T : Model, new()
            where TSource : Model, new()
        {
            var statement = BuildInsertIntoIdentityMappingsStatement(sourceData, identityMappings, targetTable);
            return GetInsertCommand(statement);
        }

        private static DbSelectStatement BuildInsertIntoIdentityMappingsStatement<T, TSource>(DbTable<TSource> sourceData, DbTable<IdentityMapping> identityMappings, DbTable<T> targetTable)
            where T : Model, new()
            where TSource : Model, new()
        {
            throw new NotImplementedException();
        }

        internal SqlCommand GetUpdateIdentityMappingsCommand(DbTable<IdentityMapping> identityMappings, DbTable<IdentityOutput> identityOutput)
        {
            var statement = BuildUpdateIdentityMappingsStatement(identityMappings, identityOutput);
            return GetUpdateCommand(statement);
        }

        private static DbSelectStatement BuildUpdateIdentityMappingsStatement(DbTable<IdentityMapping> identityMappings, DbTable<IdentityOutput> identityOutput)
        {
            throw new NotImplementedException();
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
