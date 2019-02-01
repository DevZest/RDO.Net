using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

namespace DevZest.Data.MySql
{
    public abstract partial class MySqlSession : DbSession<MySqlConnection, MySqlTransaction, MySqlCommand, MySqlReader>
    {
        protected MySqlSession(MySqlConnection mySqlConnection)
            : base(mySqlConnection)
        {
        }

        private MySqlVersion _mySqlVersion;
        public MySqlVersion MySqlVersion
        {
            get
            {
                if (_mySqlVersion.Major == 0)
                    MySqlVersion = GetMySqlVersion(Connection);
                return _mySqlVersion;
            }
            set
            {
                if (!value.IsAtLeast(8, 0, 4))
                    throw new NotSupportedException(DiagnosticMessages.VersionNotSupported(value, MySqlVersion.LowestSupported));
                _mySqlVersion = value;
            }
        }

        private static MySqlVersion GetMySqlVersion(MySqlConnection mySqlConnection)
        {
            if (mySqlConnection.State == ConnectionState.Closed)
                mySqlConnection.Open();

            return MySqlVersion.Parse(mySqlConnection.ServerVersion);
        }

        internal ConditionalWeakTable<DbQueryStatement, SqlGenerator> _sqlGeneratorCache = new ConditionalWeakTable<DbQueryStatement, SqlGenerator>();

        protected override string GetSqlString(DbQueryStatement query)
        {
            return SqlGenerator.Select(this, query).CreateCommand(null).ToTraceString();
        }

#if DEBUG
        // for unit test
        internal string InternalGetSqlString(DbQueryStatement query)
        {
            return GetSqlString(query);
        }
#endif

        protected sealed override TransactionInvoker CreateTransactionInvoker(IsolationLevel? isolationLevel)
        {
            return new MySqlTransactionInterceptorInvoker(this, Connection, isolationLevel);
        }

        protected sealed override MySqlCommand GetQueryCommand(DbQueryStatement queryStatement)
        {
            var queryGenerator = SqlGenerator.Select(this, queryStatement);
            return queryGenerator.CreateCommand(Connection);
        }

        protected sealed override ReaderInvoker CreateReaderInvoker(Model model, MySqlCommand command)
        {
            return new MySqlReaderInvoker(this, model, command);
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

        protected sealed override MySqlCommand GetCreateTableCommand(Model model, bool isTempTable)
        {
            var sqlBuilder = new IndentedStringBuilder();
            model.GenerateCreateTableSql(sqlBuilder, MySqlVersion, isTempTable);
            return sqlBuilder.ToString().CreateSqlCommand(Connection);
        }

#if DEBUG
        // for unit test
        internal MySqlCommand InternalGetCreateTableCommand(Model model, bool isTempTable)
        {
            return GetCreateTableCommand(model, isTempTable);
        }
#endif

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
            return BuildJsonQuery(dataSet, targetModel, columnMappingsBuilder);
        }

        protected sealed override Task<int> ImportAsync<T>(DataSet<T> source, DbTable<T> target, CancellationToken cancellationToken)
        {
            var command = BuildImportCommand(source, target);
            return ExecuteNonQueryAsync(command, cancellationToken);
        }

        internal MySqlCommand BuildImportCommand<T>(DataSet<T> source, DbTable<T> target)
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

        internal MySqlCommand BuildImportKeyCommand<T>(DataSet<T> source, DbTable<KeyOutput> target)
            where T : class, IModelReference, new()
        {
            var statement = target.BuildInsertStatement(BuildImportKeyQuery(source), ColumnMapper.AutoSelectInsertable);
            return GetInsertCommand(statement);
        }

        protected sealed override MySqlCommand GetInsertCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Insert(this, statement).CreateCommand(Connection);
        }

#if DEBUG
        // for unit test.
        internal MySqlCommand InternalGetInsertCommand(DbSelectStatement statement)
        {
            return GetInsertCommand(statement);
        }
#endif

        protected sealed override async Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, bool updateIdentity, CancellationToken ct)
        {
            var command = BuildInsertCommand(source, target, columnMapper);
            var result = await ExecuteNonQueryAsync(command, ct);
            if (updateIdentity)
                UpdateIdentity(source, command.LastInsertedId);
            return result;
        }

        private static void UpdateIdentity<TSource>(DataSet<TSource> dataSet, long lastInsertId)
            where TSource : class, IModelReference, new()
        {
            var identityColumn = dataSet.Model.GetColumns()[dataSet.Model.GetIdentity(false).Column.Ordinal];

            throw new NotImplementedException();
        }

        internal MySqlCommand BuildInsertCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, Action<ColumnMapper, TSource, TTarget> columnMapper)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()

        {
            var import = BuildImportQuery(source);
            var statement = target.BuildInsertStatement(import, columnMapper);
            return GetInsertCommand(statement);
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

        protected sealed override async Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken ct)
        {
            var command = GetInsertScalarCommand(statement);
            var rowCount = await ExecuteNonQueryAsync(command, ct);
            return rowCount > 0 ? new InsertScalarResult(true, command.LastInsertedId) : default(InsertScalarResult);
        }

        internal MySqlCommand GetInsertScalarCommand(DbSelectStatement statement)
        {
            return SqlGenerator.InsertScalar(this, statement).CreateCommand(Connection);
        }

        protected sealed override MySqlCommand GetUpdateCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Update(this, statement).CreateCommand(Connection);
        }

        protected sealed override Task<int> UpdateAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo, CancellationToken ct)
        {
            var command = BuildUpdateCommand(source, target, columnMapper, joinTo);
            return ExecuteNonQueryAsync(command, ct);
        }

        internal MySqlCommand BuildUpdateCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo)
            where TSource : class, IModelReference, new()
            where TTarget : class, IModelReference, new()
        {
            var import = BuildImportQuery(source);
            var join = import.Model.PrimaryKey.UnsafeJoin(joinTo);
            var statement = target.BuildUpdateStatement(import, columnMapper, join);
            return GetUpdateCommand(statement);
        }

        protected sealed override MySqlCommand GetDeleteCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Delete(this, statement).CreateCommand(Connection);
        }

        protected sealed override Task<int> DeleteAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey joinTo, CancellationToken cancellationToken)
        {
            var command = BuildDeleteCommand(source, target, joinTo);
            return ExecuteNonQueryAsync(command, cancellationToken);
        }

        internal MySqlCommand BuildDeleteCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey joinTo)
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
            var commandText =
@"declare @id int

set @id = (select max(cast(right(name, len(name) - 4) AS int)) from tempdb.sys.schemas where name like 'Mock%');
set @id = isnull(@id, 0) + 1;

declare @mockschema nvarchar(24) = N'Mock' + cast(@id as nvarchar(20));
declare @sql nvarchar(50) = N'create schema ' + @mockschema;
exec tempdb..sp_executesql @sql;
select @mockschema;
";
            var command = new MySqlCommand(commandText, Connection);
            return command.ExecuteScalar();
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

                var json = dataSet.ForJson(null, isPretty: false, omitNullValues: true);

                var source = JsonTable<TSource>(json, SYS_DATASET_ORDINAL);
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

        public DbSet<T> JsonTable<T>(string json, string ordinalColumnName = null)
            where T : class, IModelReference, new()
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            return this.CreateJsonTable<T>(json, ordinalColumnName);
        }
    }
}
