using System;
using DevZest.Data.Utilities;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Data.Primitives
{
    public abstract class DbSession : ExtensibleObject, IDisposable
    {
        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DbSession()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                CloseConnection();
        }

        #endregion

        public abstract Task OpenConnectionAsync(CancellationToken cancellationToken = default(CancellationToken));

        public abstract void CloseConnection();

        public abstract int TransactionCount { get; }

        protected internal abstract string AssignTempTableName(Model model);

        internal abstract Task CreateTableAsync(Model model, string name, string description, bool isTempTable, CancellationToken cancellationToken);

        protected internal abstract string GetSqlString(DbQueryStatement query);

        protected virtual DbTable<T> GetTable<T>(ref DbTable<T> result, string name, params Func<T, DbForeignKey>[] foreignKeys)
            where T : Model, new()
        {
            if (Mock != null)
                return Mock.GetMockTable<T>(name, foreignKeys);

            if (result == null)
            {
                Check.NotEmpty(name, nameof(name));
                var model = new T().ApplyForeignKey(foreignKeys);
                result = DbTable<T>.Create(model, this, name);
            }
            return result;
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(Action<T> initializer)
            where T : Model, new()
        {
            return CreateTempTableAsync<T>(null, initializer);
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(T fromModel = null, Action<T> initializer = null)
            where T : Model, new()
        {
            return CreateTempTableAsync<T>(CancellationToken.None);
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(CancellationToken ct)
            where T : Model, new()
        {
            return CreateTempTableAsync<T>(null, null, ct);
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(T fromModel, CancellationToken ct)
            where T : Model, new()
        {
            return CreateTempTableAsync<T>(fromModel, null, ct);
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(Action<T> initializer, CancellationToken ct)
            where T : Model, new()
        {
            return CreateTempTableAsync<T>(null, initializer, ct);
        }

        public async Task<DbTable<T>> CreateTempTableAsync<T>(T fromModel, Action<T> initializer, CancellationToken cancellationToken)
            where T : Model, new()
        {
            var result = CreateTempTableInstance(fromModel, initializer);
            await CreateTableAsync(result._, result.Name, null, true, cancellationToken);
            return result;
        }

        private async Task<DbTable<T>> ImportAsync<T>(DataSet<T> source, CancellationToken cancellationToken)
            where T : Model, new()
        {
            var result = await CreateTempTableAsync(source._, null, cancellationToken);
            await ImportAsync(source, result, cancellationToken);
            return result;
        }

        protected abstract Task<int> ImportAsync<T>(DataSet<T> source, DbTable<T> target, CancellationToken cancellationToken)
            where T : Model, new();

        private async Task<DbTable<KeyOutput>> ImportKeyAsync<T>(DataSet<T> source, CancellationToken cancellationToken)
            where T : Model, new()
        {
            var result = await CreateKeyOutputAsync(source.Model, cancellationToken);
            await ImportKeyAsync(source, result, cancellationToken);
            return result;
        }

        protected abstract Task<int> ImportKeyAsync<T>(DataSet<T> source, DbTable<KeyOutput> target, CancellationToken cancellationToken)
            where T : Model, new();

        internal DbQuery<T> PerformCreateQuery<T>(T model, DbQueryStatement queryStatement)
            where T : Model, new()
        {
            return new DbQuery<T>(model, this, queryStatement);
        }

        public DbQuery<T> CreateQuery<T>(Action<DbQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            return CreateQuery(null, null, buildQuery);
        }

        public DbQuery<T> CreateQuery<T>(T fromModel, Action <DbQueryBuilder, T> buildQuery)
           where T : Model, new()
        {
            return CreateQuery(fromModel, null, buildQuery);
        }

        public DbQuery<T> CreateQuery<T>(Action<T> initializer, Action<DbQueryBuilder, T> buildQuery)
           where T : Model, new()
        {
            return CreateQuery(null, initializer, buildQuery);
        }

        public DbQuery<T> CreateQuery<T>(T fromModel, Action<T> initializer, Action<DbQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = fromModel == null ? new T() : (fromModel.DataSource == null ? fromModel : Model.Clone(fromModel, false));
            model.Initialize(initializer);
            var builder = new DbQueryBuilder(model);
            buildQuery(builder, model);
            return PerformCreateQuery(model, builder.BuildQueryStatement(null));
        }

        public DbQuery<T> CreateAggregateQuery<T>(Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            return CreateAggregateQuery(null, null, buildQuery);
        }

        public DbQuery<T> CreateAggregateQuery<T>(T fromModel, Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            return CreateAggregateQuery(fromModel, null, buildQuery);
        }

        public DbQuery<T> CreateAggregateQuery<T>(Action<T> initializer, Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            return CreateAggregateQuery(null, initializer, buildQuery);
        }

        public DbQuery<T> CreateAggregateQuery<T>(T fromModel, Action<T> initializer, Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = fromModel == null ? new T() : (fromModel.DataSource == null ? fromModel : Model.Clone(fromModel, false));
            model.Initialize(initializer);
            var builder = new DbAggregateQueryBuilder(model);
            buildQuery(builder, model);
            return PerformCreateQuery(model, builder.BuildQueryStatement(null));
        }

        internal abstract Task RecursiveFillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken);

        internal abstract Task FillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken);

        internal DbTable<T> CreateTempTableInstance<T>(T fromModel, Action<T> initializer)
            where T : Model, new()
        {
            var model = fromModel == null ? new T() : Model.Clone(fromModel, false);
            model.Initialize(initializer);
            model.AddTempTableIdentity();
            return CreateTempTableInstance(model);
        }

        private DbTable<T> CreateTempTableInstance<T>(T model)
            where T : Model, new()
        {
            var tableName = AssignTempTableName(model);
            return DbTable<T>.CreateTemp(model, this, tableName);
        }

        internal DbTable<KeyOutput> CreateKeyOutputInstance(Model sourceModel)
        {
            var model = new KeyOutput(sourceModel, false);
            return CreateTempTableInstance(model);
        }

        private async Task<DbTable<KeyOutput>> CreateKeyOutputAsync(Model sourceModel, CancellationToken cancellationToken)
        {
            var result = CreateKeyOutputInstance(sourceModel);
            await CreateTableAsync(result.Model, result.Name, null, true, cancellationToken);
            return result;
        }

        internal abstract Task<int> InsertAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        internal abstract Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken cancellationToken);

        protected internal virtual async Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMapper, TSource, TTarget> columnMapper, PrimaryKey joinTo, DbTable<IdentityMapping> identityMappings, CancellationToken cancellationToken)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var tempTable = await ImportAsync(sourceData, cancellationToken);
            return await InsertAsync(tempTable, targetTable, columnMapper, joinTo, identityMappings, cancellationToken);
        }

        protected internal abstract Task<int> InsertAsync<TSource, TTarget>(DbTable<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMapper, TSource, TTarget> columnMapper, PrimaryKey joinTo, DbTable<IdentityMapping> identityMappings, CancellationToken cancellationToken)
            where TSource : Model, new()
            where TTarget : Model, new();

        internal abstract Task<int> UpdateAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        protected internal virtual async Task<int> UpdateAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, PrimaryKey joinTo, CancellationToken ct)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var import = await ImportAsync(source, ct);
            var join = import._.PrimaryKey.Join(joinTo);
            return await UpdateAsync(target.BuildUpdateStatement(import, columnMapper, join), ct);
        }

        internal abstract Task<int> DeleteAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        protected internal async virtual Task<int> DeleteAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, PrimaryKey joinTo, CancellationToken ct)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keys = await ImportKeyAsync(source, ct);
            var columnMappings = keys._.PrimaryKey.Join(joinTo);
            return await DeleteAsync(target.BuildDeleteStatement(keys, columnMappings), ct);
        }

        internal abstract DbReader ExecuteDbReader<T>(DbSet<T> dbSet)
            where T : Model, new();

        internal abstract Task<DbReader> ExecuteDbReaderAsync<T>(DbSet<T> dbSet, CancellationToken cancellationToken)
            where T : Model, new();

        internal IMockDb Mock { get; set; }

        protected internal abstract object CreateMockDb();

        protected internal abstract string GetMockTableName(string tableName, object tag);

        protected internal static DbForeignKey DbForeignKey<TKey>(string name, string description, TKey foreignKey, Model<TKey> refTableModel, ForeignKeyAction onDelete, ForeignKeyAction onUpdate)
            where TKey : PrimaryKey
        {
            Utilities.Check.NotNull(foreignKey, nameof(foreignKey));
            Utilities.Check.NotNull(refTableModel, nameof(refTableModel));

            var model = foreignKey.ParentModel;
            var foreignKeyConstraint = new DbForeignKey(name, description, foreignKey, refTableModel.PrimaryKey, onDelete, onUpdate);
            if (refTableModel != model && string.IsNullOrEmpty(foreignKeyConstraint.ReferencedTableName))
                throw new ArgumentException(DiagnosticMessages.Model_InvalidRefTableModel, nameof(refTableModel));
            return foreignKeyConstraint;
        }
    }
}
