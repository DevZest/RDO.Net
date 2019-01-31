using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
                    _sqlVersion = Connection.GetSqlVersion();
                return _sqlVersion;
            }
            set { _sqlVersion = value; }
        }

        internal ConditionalWeakTable<DbQueryStatement, SqlGenerator> _sqlGeneratorCache = new ConditionalWeakTable<DbQueryStatement, SqlGenerator>();

        protected override string GetSqlString(DbQueryStatement query)
        {
            return SqlGenerator.Select(this, query).CreateCommand(null).ToTraceString();
        }

        protected sealed override TransactionInvoker CreateTransactionInvoker(IsolationLevel? isolationLevel)
        {
            return new SqlTransactionInterceptorInvoker(this, Connection, isolationLevel);
        }

        protected sealed override SqlCommand GetQueryCommand(DbQueryStatement queryStatement)
        {
            var queryGenerator = SqlGenerator.Select(this, queryStatement);
            return queryGenerator.CreateCommand(Connection);
        }

        protected sealed override ReaderInvoker CreateReaderInvoker(Model model, SqlCommand command)
        {
            return new SqlReaderInvoker(this, model, command);
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

        protected sealed override SqlCommand GetCreateTableCommand(Model model, bool isTempTable)
        {
            var sqlBuilder = new IndentedStringBuilder();
            model.GenerateCreateTableSql(sqlBuilder, SqlVersion, isTempTable);
            return sqlBuilder.ToString().CreateSqlCommand(Connection);
        }

        public DbSet<SqlXmlNode> OpenXml(string dbSetName, SqlXml xml, string xPath)
        {
            dbSetName = "@" + dbSetName;
            var model = new SqlXmlNode();
            model.Initialize(dbSetName, xml, xPath);
            return model.CreateDbTable(this, dbSetName);
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

        private SqlXml GetSqlXml<T>(DataSet<T> dataSet, IList<Column> columns)
            where T : class, IModelReference, new()
        {
            var stream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
            }))
            {
                xmlWriter.WriteStartElement(XML_ROOT_TAG_NAME);

                foreach (var row in dataSet)
                {
                    xmlWriter.WriteStartElement(XML_ROW_TAG_NAME);

                    for (int i = 0; i < columns.Count; i++)
                    {
                        xmlWriter.WriteStartElement(string.Format(CultureInfo.InvariantCulture, XML_COL_TAG_NAME, i));
                        xmlWriter.WriteString(columns[i].GetSqlType().GetXmlValueByOrdinal(dataSet.IndexOf(row), SqlVersion));
                        xmlWriter.WriteEndElement();
                    }

                    // Write an extra column as the sequential id (row.Ordinal + 1) of current row
                    xmlWriter.WriteStartElement(string.Format(CultureInfo.InvariantCulture, XML_COL_TAG_NAME, columns.Count));
                    xmlWriter.WriteString((dataSet.IndexOf(row) + 1).ToString(NumberFormatInfo.InvariantInfo));
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();

                stream.Position = 0;
                return new SqlXml(stream);
            }
        }

        internal DbQuery<KeyOutput> BuildImportKeyQuery<T>(DataSet<T> dataSet)
            where T : class, IModelReference, new()
        {
            var keyOutput = new KeyOutput(dataSet.Model);
            return BuildQuery(dataSet, keyOutput, (m, s, t) => KeyOutput.BuildKeyMappings(m, s.Model, t));
        }

        internal DbQuery<T> BuildImportQuery<T>(DataSet<T> dataSet)
            where T : class, IModelReference, new()
        {
            return BuildQuery(dataSet, dataSet._, null);
        }

        internal DbQuery<TTarget> BuildQuery<TSource, TTarget>(DataSet<TSource> dataSet, TTarget targetModel, Action<ColumnMapper, TSource, TTarget> columnMappingsBuilder)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()
        {
            if (SqlVersion >= SqlVersion.Sql13)
                return BuildJsonQuery(dataSet, targetModel, columnMappingsBuilder);
            else
                return BuildXmlQuery(dataSet, targetModel, columnMappingsBuilder);
        }


        private DbQuery<TTarget> BuildXmlQuery<TSource, TTarget>(DataSet<TSource> dataSet, TTarget targetModel, Action<ColumnMapper, TSource, TTarget> columnMappingsBuilder)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()
        {
            var result = CreateQuery<TTarget>(targetModel, (builder, _) =>
            {
                var dataSetOrdinalColumn = new _Int32();
                _.Model.AddSystemColumn(dataSetOrdinalColumn, "sys_dataset_ordinal");

                var columnMappings = ColumnMapping.Map(dataSet._, _, columnMappingsBuilder, true);

                var xml = GetSqlXml(dataSet, columnMappings.Select(x => ((DbColumnExpression)x.SourceExpression).Column).ToList());

                var source = OpenXml(dataSet.Model.GetDbAlias(), xml, XML_ROW_XPATH);
                builder.From(source, out var xmlNode);
                for (int i = 0; i < columnMappings.Count; i++)
                {
                    var targetColumn = columnMappings[i].Target;
                    builder.SelectColumn(xmlNode[GetColumnTagXPath(i), targetColumn], targetColumn);
                }
                builder.OrderBy(xmlNode[GetColumnTagXPath(columnMappings.Count), dataSetOrdinalColumn].Asc());
            });
            result.UpdateOriginalDataSource(dataSet, true);
            return result;
        }

        protected sealed override Task<int> ImportAsync<T>(DataSet<T> source, DbTable<T> target, CancellationToken cancellationToken)
        {
            var command = BuildImportCommand(source, target);
            return ExecuteNonQueryAsync(command, cancellationToken);
        }

        internal SqlCommand BuildImportCommand<T>(DataSet<T> source, DbTable<T> target)
            where T : class, IModelReference, new()
        {
            var statement = target.BuildInsertStatement(BuildImportQuery(source), ColumnMapper.AutoSelectInsertable);
            return GetInsertCommand(statement);
        }

        protected sealed override Task<int> ImportKeyAsync<T>(DataSet<T> source, DbTable<KeyOutput> target, CancellationToken cancellationToken)
        {
            var command = BuildImportKeyCommand(source, target);
            return ExecuteNonQueryAsync(command, cancellationToken);
        }

        internal SqlCommand BuildImportKeyCommand<T>(DataSet<T> source, DbTable<KeyOutput> target)
            where T : class, IModelReference, new()
        {
            var statement = target.BuildInsertStatement(BuildImportKeyQuery(source), ColumnMapper.AutoSelectInsertable);
            return GetInsertCommand(statement);
        }

        protected sealed override SqlCommand GetInsertCommand(DbSelectStatement statement)
        {
            return GetInsertCommand(statement, null);
        }

        internal async Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo, IDbTable identityOutput, CancellationToken ct)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()
        {
            var command = BuildInsertCommand(source, target, columnMapper, joinTo, identityOutput);
            return await ExecuteNonQueryAsync(command, ct);
        }

        internal SqlCommand BuildInsertCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo, IDbTable identityOutput)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()

        {
            var import = BuildImportQuery(source);
            IReadOnlyList<ColumnMapping> join = joinTo == null ? null : import.Model.PrimaryKey.UnsafeJoin(joinTo);
            var statement = target.BuildInsertStatement(import, columnMapper, join);
            return GetInsertCommand(statement, identityOutput);
        }

        internal async Task<int> InsertAsync<TSource, TTarget>(DbTable<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo, IDbTable identityMappings, CancellationToken ct)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()
        {
            IReadOnlyList<ColumnMapping> join = joinTo == null ? null : source.Model.PrimaryKey.UnsafeJoin(joinTo);
            var statement = target.BuildInsertStatement(source, columnMapper, join);
            if (identityMappings == null)
                return await ExecuteNonQueryAsync(GetInsertCommand(statement), ct);

            var identityOutput = await CreateIdentityOutputTable(identityMappings, ct);
            var result = await ExecuteNonQueryAsync(GetInsertCommand(statement, identityOutput), ct);
            await ExecuteNonQueryAsync(GetInsertIntoIdentityMappingsCommand(source, identityMappings, joinTo != null ? target : null), ct);
            await ExecuteNonQueryAsync(GetUpdateIdentityMappingsCommand(identityMappings, identityOutput), ct);
            return result;
        }

        private async Task<IDbTable> CreateIdentityOutputTable(IDbTable identityMappings, CancellationToken ct)
        {
            if (identityMappings is DbTable<Int32IdentityMapping>)
                return await CreateTempTableAsync<Int32IdentityOutput>(ct);
            else if (identityMappings is DbTable<Int64IdentityMapping>)
                return await CreateTempTableAsync<Int64IdentityOutput>(ct);
            else
            {
                Debug.Assert(identityMappings is DbTable<Int16IdentityMapping>);
                return await CreateTempTableAsync<Int16IdentityOutput>(ct);
            }
        }

        protected sealed override Task<int> InsertWithUpdateIdentityAsync<TSource, TTarget>(DbTable<TSource> sourceData, DbTable<TTarget> targetTable, Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo, CancellationToken ct)
        {
            return DbTableInsert<TTarget>.ExecuteWithUpdateIdentityAsync(targetTable, sourceData, columnMapper, joinTo, ct);
        }

        protected sealed override Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable, Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo, bool updateIdentity, CancellationToken ct)
        {
            return DbTableInsert<TTarget>.ExecuteAsync(targetTable, sourceData, columnMapper, joinTo, updateIdentity, ct);
        }

        internal SqlCommand GetInsertCommand(DbSelectStatement statement, IDbTable identityOutput = null)
        {
            return SqlGenerator.Insert(this, statement, identityOutput).CreateCommand(Connection);
        }

        internal SqlCommand GetInsertIntoIdentityMappingsCommand<T, TSource>(DbTable<TSource> sourceData, IDbTable identityMappings, DbTable<T> targetTable)
            where T : class, IModelReference, new()
            where TSource : class, IModelReference, new()
        {
            var statement = BuildInsertIntoIdentityMappingsStatement(sourceData, identityMappings, targetTable);
            return GetInsertCommand(statement);
        }

        private DbSelectStatement BuildInsertIntoIdentityMappingsStatement<T, TSource>(DbTable<TSource> sourceData, IDbTable identityMappings, DbTable<T> targetTable)
            where T : class, IModelReference, new()
            where TSource : class, IModelReference, new()
        {
            var identityMappingModel = identityMappings.Model;
            var identityMapping = (IIdentityMapping)identityMappingModel;
            var source = sourceData.Model;
            var sourceSysRowId = source.GetIdentity(true).Int32Column;

            var select = new ColumnMapping[]
            {
                source.GetIdentity(false).Column.UnsafeMap(identityMapping.OldValue),
                ColumnMapping.Map(sourceSysRowId, identityMapping.OriginalSysRowId)
            };

            var from = GetAutoJoinFromClause(sourceData, targetTable);
            DbExpression where = targetTable == null ? null : targetTable.Model.PrimaryKey[0].Column.IsNull().DbExpression;

            var orderBy = new DbExpressionSort[] { new DbExpressionSort(sourceSysRowId.DbExpression, SortDirection.Ascending) };

            return new DbSelectStatement(identityMappingModel, select, from, where, orderBy, -1, -1);
        }

        private static DbFromClause GetAutoJoinFromClause<TSource, TTarget>(DbTable<TSource> source, DbTable<TTarget> target)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()
        {
            if (target == null)
                return source.GetFromClause();

            var mappings = new ColumnMapping[] { source.Model.GetIdentity(false).Column.UnsafeMap(target.Model.GetIdentity(false).Column) };
            return new DbJoinClause(DbJoinKind.LeftJoin, source.GetFromClause(), target.GetFromClause(), new ReadOnlyCollection<ColumnMapping>(mappings));
        }

        internal SqlCommand GetUpdateIdentityMappingsCommand(IDbTable identityMappings, IDbTable identityOutput)
        {
            var statement = BuildUpdateIdentityMappingsStatement(identityMappings, identityOutput);
            return GetUpdateCommand(statement);
        }

        private static DbSelectStatement BuildUpdateIdentityMappingsStatement(IDbTable identityMappings, IDbTable identityOutputs)
        {
            var identityMappingModel = identityMappings.Model;
            var identityMapping = (IIdentityMapping)identityMappingModel;
            var identityOutputModel = identityOutputs.Model;
            var identityOutput = (IIdentityOutput)identityOutputModel;
            var select = new ColumnMapping[]
            {
                identityOutput.NewValue.UnsafeMap(identityMapping.NewValue)
            };

            var mappings = new ColumnMapping[] { identityMappingModel.GetIdentity(true).Column.UnsafeMap(identityOutputModel.GetIdentity(true).Column) };
            var from = new DbJoinClause(DbJoinKind.InnerJoin, identityMappings.GetFromClause(), identityOutputs.GetFromClause(),
                new ReadOnlyCollection<ColumnMapping>(mappings));
            return new DbSelectStatement(identityMappingModel, select, from, null, null, -1, -1);
        }

        protected sealed override SqlCommand GetInsertScalarCommand(DbSelectStatement statement, bool outputIdentity)
        {
            return SqlGenerator.InsertScalar(this, statement, outputIdentity).CreateCommand(Connection);
        }

#if DEBUG
        // for unit test
        internal SqlCommand InternalGetInsertScalarCommand(DbSelectStatement statement, bool outputIdentity)
        {
            return GetInsertScalarCommand(statement, outputIdentity);
        }
#endif

        protected sealed override SqlCommand GetUpdateCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Update(this, statement).CreateCommand(Connection);
        }

        protected sealed override Task<int> UpdateAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo, CancellationToken ct)
        {
            var command = BuildUpdateCommand(source, target, columnMapper, joinTo);
            return ExecuteNonQueryAsync(command, ct);
        }

        internal SqlCommand BuildUpdateCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()
        {
            var import = BuildImportQuery(source);
            var join = import.Model.PrimaryKey.UnsafeJoin(joinTo);
            var statement = target.BuildUpdateStatement(import, columnMapper, join);
            return GetUpdateCommand(statement);
        }

        protected sealed override SqlCommand GetDeleteCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Delete(this, statement).CreateCommand(Connection);
        }

        protected sealed override Task<int> DeleteAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey joinTo, CancellationToken cancellationToken)
        {
            var command = BuildDeleteCommand(source, target, joinTo);
            return ExecuteNonQueryAsync(command, cancellationToken);
        }

        internal SqlCommand BuildDeleteCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey joinTo)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()
        {
            var keys = BuildImportKeyQuery(source);
            var columnMappings = keys._.PrimaryKey.UnsafeJoin(joinTo);
            var statement = target.BuildDeleteStatement(keys, columnMappings);
            return GetDeleteCommand(statement);
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
            var sqlCommand = new SqlCommand(sqlCommandText, Connection);
            return sqlCommand.ExecuteScalar();
        }

        protected sealed override string GetMockTableName(string tableName, object tag)
        {
            var quotedTableName = string.Join(".", tableName.ParseIdentifier()).ToQuotedIdentifier();
            return string.Format(CultureInfo.InvariantCulture, "[tempdb].[{0}].{1}", tag, quotedTableName);
        }

        private const string SYS_DATASET_ORDINAL = "sys_dataset_ordinal";

        private DbQuery<TTarget> BuildJsonQuery<TSource, TTarget>(DataSet<TSource> dataSet, TTarget targetModel, Action<ColumnMapper, TSource, TTarget> columnMappingsBuilder)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()
        {
            var result = CreateQuery<TTarget>(targetModel, (builder, _) =>
            {

                var columnMappings = ColumnMapping.Map(dataSet._, _, columnMappingsBuilder, true);

                var json = dataSet.ForJson(SYS_DATASET_ORDINAL, isPretty:false);

                var source = OpenJson<TSource>(json, SYS_DATASET_ORDINAL);
                var sourceColumns = source.Model.GetColumns();
                var dataSetOrdinalColumn = sourceColumns[SYS_DATASET_ORDINAL];
                builder.From(source, out var _);
                for (int i = 0; i < columnMappings.Count; i++)
                {
                    var sourceColumn = sourceColumns[columnMappings[i].Source.Ordinal];
                    var targetColumn = columnMappings[i].Target;
                    builder.SelectColumn(sourceColumn, targetColumn);
                }
                builder.OrderBy(dataSetOrdinalColumn.Asc());
            });
            result.UpdateOriginalDataSource(dataSet, true);
            return result;
        }

        public DbSet<T> OpenJson<T>(string json, string ordinalColumnName = null)
            where T : class, IModelReference, new()
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            return this.CreateJsonRowSet<T>(json, ordinalColumnName);
        }
    }
}
