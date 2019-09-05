using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.MySql
{
    /// <summary>
    /// MySQL implementation of database session.
    /// </summary>
    public abstract partial class MySqlSession : DbSession<MySqlConnection, MySqlCommand, MySqlReader>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlSession"/>.
        /// </summary>
        /// <param name="mySqlConnection">The MySQL connection.</param>
        protected MySqlSession(MySqlConnection mySqlConnection)
            : base(mySqlConnection)
        {
        }

        private MySqlVersion _mySqlVersion;
        /// <summary>
        /// Gets or sets the MySQL version.
        /// </summary>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected sealed override MySqlCommand GetQueryCommand(DbQueryStatement queryStatement)
        {
            var queryGenerator = SqlGenerator.Select(this, queryStatement);
            return queryGenerator.CreateCommand(Connection);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override string AssignTempTableName(Model model)
        {
            return _tempTableNamesByModel.GetValue(model, GetUniqueTempTableName);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected sealed override async Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, bool updateIdentity, CancellationToken ct)
        {
            var command = BuildInsertCommand(source, target, columnMapper);
            var result = await ExecuteNonQueryAsync(command, ct);
            if (updateIdentity)
                IdentityUpdater.Execute(source, command.LastInsertedId);
            return result;
        }

        internal MySqlCommand BuildInsertCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, Action<ColumnMapper, TSource, TTarget> columnMapper)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()

        {
            var import = BuildImportQuery(source);
            var statement = target.BuildInsertStatement(import, columnMapper);
            return GetInsertCommand(statement);
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected sealed override MySqlCommand GetUpdateCommand(DbSelectStatement statement)
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

        internal MySqlCommand BuildUpdateCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey targetKey)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            var import = BuildImportQuery(source);
            var join = import.Model.PrimaryKey.UnsafeJoin(targetKey);
            var statement = target.BuildUpdateStatement(import, columnMapper, join);
            return GetUpdateCommand(statement);
        }

        /// <inheritdoc/>
        protected sealed override MySqlCommand GetDeleteCommand(DbSelectStatement statement)
        {
            return SqlGenerator.Delete(this, statement).CreateCommand(Connection);
        }

        /// <inheritdoc/>
        protected sealed override Task<int> DeleteAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey targetKey, CancellationToken cancellationToken)
        {
            var command = BuildDeleteCommand(source, target, targetKey);
            return ExecuteNonQueryAsync(command, cancellationToken);
        }

        internal MySqlCommand BuildDeleteCommand<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey targetKey)
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

            var commandText =
@"set @id = (select max(cast(right(SCHEMA_NAME, char_length(SCHEMA_NAME) - 4) as unsigned)) from INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME like 'Mock%');
set @id = IFNULL(@id, 0) + 1;
set @mockschema = concat('Mock', @id);
set @sqlText = concat('create schema ', @mockschema, ';');
prepare stmt from @sqlText;
execute stmt;
deallocate prepare stmt;
select @mockschema;
";
            var command = new MySqlCommand(commandText, Connection);
            return await command.ExecuteScalarAsync(ct);
        }

        /// <inheritdoc/>
        protected sealed override string GetMockTableName(string tableName, object tag)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ((string)tag).ToQuotedIdentifier(), tableName.ToQuotedIdentifier());
        }

        private const string SYS_DATASET_ORDINAL = "sys_dataset_ordinal";

        private DbQuery<TTarget> BuildJsonQuery<TSource, TTarget>(DataSet<TSource> dataSet, TTarget targetModel, Action<ColumnMapper, TSource, TTarget> columnMappingsBuilder)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            var result = CreateQuery<TTarget>(targetModel, (builder, _) =>
            {

                var columnMappings = ColumnMapping.Map(dataSet._, _, columnMappingsBuilder, true);

                var json = dataSet.ForJson(null, isPretty: false);

                var source = JsonTable<TSource>(json, SYS_DATASET_ORDINAL);
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
        public DbSet<T> JsonTable<T>(string json, string ordinalColumnName = null)
            where T : class, IEntity, new()
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            return this.CreateJsonTable<T>(json, ordinalColumnName);
        }
    }
}
