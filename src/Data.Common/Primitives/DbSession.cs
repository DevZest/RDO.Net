using System;
using DevZest.Data.Utilities;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public abstract class DbSession : Interceptable, IDisposable
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
        }

        #endregion

        internal abstract void InternalOpenConnection();

        internal abstract Task InternalOpenConnectionAsync(CancellationToken cancellationToken);

        public abstract int TransactionCount { get; }

        protected internal abstract string AssignTempTableName(Model model);

        internal abstract void CreateTable(Model model, string tableName, bool isTempTable);

        internal abstract Task CreateTableAsync(Model model, string tableName, bool isTempTable, CancellationToken cancellationToken);

        protected internal abstract string GetSqlString(DbQueryStatement query);

        protected virtual DbTable<T> GetTable<T>(ref DbTable<T> result, string name, Action<T> initializer = null)
            where T : Model, new()
        {
            if (Mock != null)
                return Mock.GetMockTable<T>(name, initializer);

            if (result == null)
            {
                Check.NotEmpty(name, nameof(name));
                var model = new T();
                model.Initialize(initializer);
                result = DbTable<T>.Create(model, this, name);
            }
            return result;
        }

        public DbTable<T> CreateTempTable<T>(T fromModel = null, Action<T> initializer = null)
            where T : Model, new()
        {
            var result = CreateTempTableInstance(fromModel, initializer);
            CreateTable(result._, result.Name, true);
            return result;
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(T fromModel = null, Action<T> initializer = null)
            where T : Model, new()
        {
            return CreateTempTableAsync(fromModel, initializer, CancellationToken.None);
        }

        public async Task<DbTable<T>> CreateTempTableAsync<T>(T fromModel, Action<T> initializer, CancellationToken cancellationToken)
            where T : Model, new()
        {
            var result = CreateTempTableInstance(fromModel, initializer);
            await CreateTableAsync(result._, result.Name, true, cancellationToken);
            return result;
        }

        private DbTable<T> Import<T>(DataSet<T> source)
            where T : Model, new()
        {
            var result = CreateTempTable(source._);
            Import(source, result);
            return result;
        }

        protected abstract int Import<T>(DataSet<T> source, DbTable<T> target)
            where T : Model, new();

        private async Task<DbTable<T>> ImportAsync<T>(DataSet<T> source, CancellationToken cancellationToken)
            where T : Model, new()
        {
            var result = await CreateTempTableAsync(source._, null, cancellationToken);
            await ImportAsync(source, result, cancellationToken);
            return result;
        }

        protected abstract Task<int> ImportAsync<T>(DataSet<T> source, DbTable<T> target, CancellationToken cancellationToken)
            where T : Model, new();

        private DbTable<KeyOutput> ImportKey<T>(DataSet<T> source)
            where T : Model, new()
        {
            var result = CreateKeyOutput(source._);
            ImportKey(source, result);
            return result;
        }

        protected abstract int ImportKey<T>(DataSet<T> source, DbTable<KeyOutput> target)
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

        internal abstract void RecursiveFillDataSet(IDbSet dbSet, DataSet dataSet);

        internal abstract Task RecursiveFillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken);

        internal abstract void FillDataSet(IDbSet dbSet, DataSet dataSet);

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

        private DbTable<KeyOutput> CreateKeyOutput(Model sourceModel)
        {
            var result = CreateKeyOutputInstance(sourceModel);
            CreateTable(result.Model, result.Name, true);
            return result;
        }

        private async Task<DbTable<KeyOutput>> CreateKeyOutputAsync(Model sourceModel, CancellationToken cancellationToken)
        {
            var result = CreateKeyOutputInstance(sourceModel);
            await CreateTableAsync(result.Model, result.Name, true, cancellationToken);
            return result;
        }

        internal abstract int Insert(DbSelectStatement statement);

        internal abstract Task<int> InsertAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        internal abstract InsertScalarResult InsertScalar(DbSelectStatement statement, bool outputIdentity);

        internal abstract Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken cancellationToken);

        protected internal virtual int Insert<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool autoJoin, DbTable<IdentityMapping> identityMappings)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var tempTable = Import(sourceData);
            return Insert(tempTable, targetTable, columnMappingsBuilder, autoJoin, identityMappings);
        }

        protected internal virtual async Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool autoJoin, DbTable<IdentityMapping> identityMappings, CancellationToken cancellationToken)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var tempTable = await ImportAsync(sourceData, cancellationToken);
            return await InsertAsync(tempTable, targetTable, columnMappingsBuilder, autoJoin, identityMappings, cancellationToken);
        }

        protected internal abstract int Insert<TSource, TTarget>(DbTable<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool autoJoin, DbTable<IdentityMapping> identityMappings)
            where TSource : Model, new()
            where TTarget : Model, new();

        protected internal abstract Task<int> InsertAsync<TSource, TTarget>(DbTable<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool autoJoin, DbTable<IdentityMapping> identityMappings, CancellationToken cancellationToken)
            where TSource : Model, new()
            where TTarget : Model, new();

        internal abstract int Update(DbSelectStatement statement);

        internal abstract Task<int> UpdateAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        protected internal virtual int Update<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var tempTable = Import(sourceData);
            return Update(targetTable.BuildUpdateStatement(tempTable, columnMappingsBuilder));
        }

        protected internal virtual async Task<int> UpdateAsync<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, CancellationToken cancellationToken)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var tempTable = await ImportAsync(sourceData, cancellationToken);
            return await UpdateAsync(targetTable.BuildUpdateStatement(tempTable, columnMappingsBuilder), cancellationToken);
        }

        internal abstract int Delete(DbSelectStatement statement);

        internal abstract Task<int> DeleteAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        protected internal virtual int Delete<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable, Func<TTarget, KeyBase> joinOn)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keys = ImportKey(sourceData);
            return Delete(targetTable.BuildDeleteStatement(keys, joinOn));
        }

        protected internal async virtual Task<int> DeleteAsync<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Func<TTarget, KeyBase> joinOn, CancellationToken cancellationToken)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keys = await ImportKeyAsync(sourceData, cancellationToken);
            return await DeleteAsync(targetTable.BuildDeleteStatement(keys, joinOn), cancellationToken);
        }

        internal abstract DbReader ExecuteDbReader<T>(DbSet<T> dbSet)
            where T : Model, new();

        internal abstract Task<DbReader> ExecuteDbReaderAsync<T>(DbSet<T> dbSet, CancellationToken cancellationToken)
            where T : Model, new();

        internal IMockDb Mock { get; set; }

        protected internal abstract object CreateMockDb();

        protected internal abstract string GetMockTableName(string tableName, object tag);

        protected internal static void ForeignKey<TKey>(string constraintName, TKey foreignKey, Model<TKey> refTableModel, ForeignKeyAction onDelete, ForeignKeyAction onUpdate)
            where TKey : KeyBase
        {
            Utilities.Check.NotNull(foreignKey, nameof(foreignKey));
            Utilities.Check.NotNull(refTableModel, nameof(refTableModel));

            var model = foreignKey.ParentModel;
            var foreignKeyConstraint = new ForeignKeyConstraint(constraintName, foreignKey, refTableModel.PrimaryKey, onDelete, onUpdate);
            if (refTableModel != model && string.IsNullOrEmpty(foreignKeyConstraint.ReferencedTableName))
                throw new ArgumentException(Strings.Model_InvalidRefTableModel, nameof(refTableModel));
            model.AddDbTableConstraint(foreignKeyConstraint, false);
        }
    }
}
