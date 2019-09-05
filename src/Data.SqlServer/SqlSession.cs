using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.SqlServer
{
    /// <summary>
    /// SQL Server implementation of database session.
    /// </summary>
    public abstract partial class SqlSession : DbSession<SqlConnection, SqlCommand, SqlReader>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqlSession"/>.
        /// </summary>
        /// <param name="sqlConnection">The SQL Server connection.</param>
        protected SqlSession(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private SqlVersion _sqlVersion;
        /// <summary>
        /// Gets or sets the SQL Server version.
        /// </summary>
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

        /// <inheritdoc/>
        protected override string GetSqlString(DbQueryStatement query)
        {
            return SqlGenerator.Select(this, query).CreateCommand(null).ToTraceString();
        }

        /// <inheritdoc/>
        protected sealed override SqlCommand GetQueryCommand(DbQueryStatement queryStatement)
        {
            var queryGenerator = SqlGenerator.Select(this, queryStatement);
            return queryGenerator.CreateCommand(Connection);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override string AssignTempTableName(Model model)
        {
            return _tempTableNamesByModel.GetValue(model, GetUniqueTempTableName);
        }

        /// <inheritdoc/>
        protected sealed override SqlCommand GetCreateTableCommand(Model model, bool isTempTable)
        {
            var sqlBuilder = new IndentedStringBuilder();
            model.GenerateCreateTableSql(sqlBuilder, SqlVersion, isTempTable);
            return sqlBuilder.ToString().CreateSqlCommand(Connection);
        }

        internal DbQuery<KeyOutput> BuildImportKeyQuery<T>(DataSet<T> dataSet)
            where T : class, IEntity, new()
        {
            var keyOutput = new KeyOutput(dataSet.Model);
            return BuildQuery(dataSet, keyOutput, (m, s, t) => KeyOutput.BuildKeyMappings(m, s.Model, t));
        }

        internal DbQuery<T> BuildImportQuery<T>(DataSet<T> dataSet)
            where T : class, IEntity, new()
        {
            return BuildQuery(dataSet, dataSet._, null);
        }

        internal DbQuery<TTarget> BuildQuery<TSource, TTarget>(DataSet<TSource> dataSet, TTarget targetModel, Action<ColumnMapper, TSource, TTarget> columnMappingsBuilder)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            return BuildJsonQuery(dataSet, targetModel, columnMappingsBuilder);
        }

        /// <inheritdoc/>
        protected sealed override SqlCommand GetInsertCommand(DbSelectStatement statement)
        {
            return GetInsertCommand(statement, null);
        }

        internal async Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, IDbTable identityOutputs, CancellationToken ct)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            var command = BuildInsertCommand(source, target, columnMapper, identityOutputs);
            return await ExecuteNonQueryAsync(command, ct);
        }

        internal SqlCommand BuildInsertCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, IDbTable identityOutput)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()

        {
            var import = BuildImportQuery(source);
            var statement = target.BuildInsertStatement(import, columnMapper);
            return GetInsertCommand(statement, identityOutput);
        }

        /// <inheritdoc/>
        protected sealed override Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable, Action<ColumnMapper, TSource, TTarget> columnMapper, bool updateIdentity, CancellationToken ct)
        {
            return DbTableInsert<TTarget>.ExecuteAsync(targetTable, sourceData, columnMapper, updateIdentity, ct);
        }

        internal SqlCommand GetInsertCommand(DbSelectStatement statement, IDbTable identityOutput = null)
        {
            return SqlGenerator.Insert(this, statement, identityOutput).CreateCommand(Connection);
        }

        private static DbFromClause GetAutoJoinFromClause<TSource, TTarget>(DbTable<TSource> source, DbTable<TTarget> target)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            IDbSet sourceDbSet = source;
            if (target == null)
                return sourceDbSet.FromClause;

            var mappings = new ColumnMapping[] { source.Model.GetIdentity(false).Column.UnsafeMap(target.Model.GetIdentity(false).Column) };
            IDbSet targetDbSet = target;
            return new DbJoinClause(DbJoinKind.LeftJoin, sourceDbSet.FromClause, targetDbSet.FromClause, new ReadOnlyCollection<ColumnMapping>(mappings));
        }

        private sealed class ScalarIdentityOutput : Model
        {
            // This will NOT work. Static field is initialized before static constructor!
            //public static readonly ScalarIdentityOutput Singleton = new ScalarIdentityOutput();

            private static ScalarIdentityOutput s_singleton;
            public static ScalarIdentityOutput Singleton
            {
                get
                {
                    if (s_singleton == null)
                        s_singleton = new ScalarIdentityOutput();
                    return s_singleton;
                }
            }

            static ScalarIdentityOutput()
            {
                RegisterColumn((ScalarIdentityOutput x) => x.IdentityValue);
            }

            private ScalarIdentityOutput()
            {
            }

            public _Int64 IdentityValue { get; private set; }
        }

        /// <inheritdoc/>
        protected sealed override async Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken ct)
        {
            var command = GetInsertScalarCommand(statement, outputIdentity, out var scopeIdentityParam);
            var rowCount = await ExecuteNonQueryAsync(command, ct);
            if (scopeIdentityParam == null)
                return new InsertScalarResult(rowCount > 0, null);
            else
            {
                var returnValue = scopeIdentityParam.Value;
                return returnValue == DBNull.Value ? new InsertScalarResult(false, null) : new InsertScalarResult(true, (long)scopeIdentityParam.Value);
            }
        }

        internal SqlCommand GetInsertScalarCommand(DbSelectStatement statement, bool outputIdentity, out SqlParameter scopeIdentityParam)
        {
            return SqlGenerator.InsertScalar(this, statement, outputIdentity, out scopeIdentityParam).CreateCommand(Connection, scopeIdentityParam);
        }

        /// <inheritdoc/>
        protected sealed override SqlCommand GetUpdateCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Update(this, statement).CreateCommand(Connection);
        }

        /// <inheritdoc/>
        protected sealed override Task<int> UpdateAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey targetKey, CancellationToken ct)
        {
            var command = BuildUpdateCommand(source, target, columnMapper, targetKey);
            return ExecuteNonQueryAsync(command, ct);
        }

        internal SqlCommand BuildUpdateCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey targetKey)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            var import = BuildImportQuery(source);
            var join = import.Model.PrimaryKey.UnsafeJoin(targetKey);
            var statement = target.BuildUpdateStatement(import, columnMapper, join);
            return GetUpdateCommand(statement);
        }

        /// <inheritdoc/>
        protected sealed override SqlCommand GetDeleteCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Delete(this, statement).CreateCommand(Connection);
        }

        /// <inheritdoc/>
        protected sealed override Task<int> DeleteAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey targetKey, CancellationToken cancellationToken)
        {
            var command = BuildDeleteCommand(source, target, targetKey);
            return ExecuteNonQueryAsync(command, cancellationToken);
        }

        internal SqlCommand BuildDeleteCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey targetKey)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            var keys = BuildImportKeyQuery(source);
            var columnMappings = keys._.PrimaryKey.UnsafeJoin(targetKey);
            var statement = target.BuildDeleteStatement(keys, columnMappings);
            return GetDeleteCommand(statement);
        }

        /// <inheritdoc/>
        protected sealed override async Task<object> CreateMockDbAsync(CancellationToken ct)
        {
            if (Connection.State != ConnectionState.Open)
                await OpenConnectionAsync(ct);

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
            return await sqlCommand.ExecuteScalarAsync(ct);
        }

        /// <inheritdoc/>
        protected sealed override string GetMockTableName(string tableName, object tag)
        {
            var quotedTableName = string.Join(".", tableName.ParseIdentifier()).ToQuotedIdentifier();
            return string.Format(CultureInfo.InvariantCulture, "[tempdb].[{0}].{1}", tag, quotedTableName);
        }

        private const string SYS_DATASET_ORDINAL = "sys_dataset_ordinal";

        private DbQuery<TTarget> BuildJsonQuery<TSource, TTarget>(DataSet<TSource> dataSet, TTarget targetModel, Action<ColumnMapper, TSource, TTarget> columnMappingsBuilder)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
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
                    builder.UnsafeSelect(sourceColumn, targetColumn);
                }
                builder.OrderBy(dataSetOrdinalColumn.Asc());
            });
            result.UpdateOriginalDataSource(dataSet, true);
            return result;
        }

        /// <summary>
        /// Opens JSON string as DbSet.
        /// </summary>
        /// <typeparam name="T">Type of entity.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <param name="ordinalColumnName">The name of extra ordinal column.</param>
        /// <returns>The result DbSet.</returns>
        public DbSet<T> OpenJson<T>(string json, string ordinalColumnName = null)
            where T : class, IEntity, new()
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            return this.CreateJsonRowSet<T>(json, ordinalColumnName);
        }

        private DbSet<SqlXmlNode> OpenXml(string dbSetName, SqlXml xml, string xPath)
        {
            dbSetName = "@" + dbSetName;
            var model = new SqlXmlNode();
            model.Initialize(dbSetName, xml, xPath);
            return model.CreateDbTable(this, dbSetName);
        }
    }
}
