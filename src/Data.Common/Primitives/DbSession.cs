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
                if (initializer != null)
                    initializer(model);
                result = DbTable<T>.Create(model, this, name);
            }
            return result;
        }

        public DbTable<T> CreateTempTable<T>(Action<T> initializer = null)
            where T : Model, new()
        {
            var result = NewTempTableObject(initializer);
            CreateTable(result._, result.Name, true);
            return result;
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(Action<T> initializer = null)
            where T : Model, new()
        {
            return CreateTempTableAsync(initializer, CancellationToken.None);
        }

        public async Task<DbTable<T>> CreateTempTableAsync<T>(Action<T> initializer, CancellationToken cancellationToken)
            where T : Model, new()
        {
            var result = NewTempTableObject(initializer);
            await CreateTableAsync(result._, result.Name, true, cancellationToken);
            return result;
        }

        internal DbQuery<T> CreateQuery<T>(T model, DbQueryBuilder2 builder, DbTable<SequentialKeyModel> sequentialKeys = null)
            where T : Model, new()
        {
            Debug.Assert(model == builder.Model);
            var selectStatement = builder.BuildQueryStatement(sequentialKeys);
            return new DbQuery<T>(model, this, selectStatement);
        }


        public DbQuery<T> CreateQuery<T>(Action<DbQueryBuilder2, T> buildQuery)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = new T();
            var builder = new DbQueryBuilder2(model);
            buildQuery(builder, model);
            return CreateQuery(model, builder);
        }

        public DbQuery<T> CreateQuery<T>(Action<DbAggregateQueryBuilder2, T> buildQuery)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = new T();
            var builder = new DbAggregateQueryBuilder2(model);
            buildQuery(builder, model);
            return CreateQuery(model, builder);
        }

        internal abstract void RecursiveFillDataSet(IDbSet dbSet, DataSet dataSet);

        internal abstract Task RecursiveFillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken);

        internal abstract void FillDataSet(IDbSet dbSet, DataSet dataSet);

        internal abstract Task FillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken);

        internal DbTable<T> NewTempTableObject<T>(Action<T> initializer = null)
            where T : Model, new()
        {
            var model = new T();
            if (initializer != null)
                initializer(model);
            model.AddTempTableIdentity();
            var tableName = AssignTempTableName(model);
            return DbTable<T>.CreateTemp(model, this, tableName);
        }

        internal abstract int Insert(DbSelectStatement statement);

        internal abstract Task<int> InsertAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        internal abstract InsertScalarResult InsertScalar(DbSelectStatement statement, bool outputIdentity);

        internal abstract Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken cancellationToken);

        protected internal abstract int Insert<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool autoJoin)
            where TSource : Model, new()
            where TTarget : Model, new();

        protected internal abstract Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool autoJoin, CancellationToken cancellationToken)
            where TSource : Model, new()
            where TTarget : Model, new();

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

        internal abstract int Delete(DbSelectStatement statement);

        internal abstract Task<int> DeleteAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        internal abstract DbReader ExecuteDbReader<T>(DbSet<T> dbSet)
            where T : Model, new();

        internal abstract Task<DbReader> ExecuteDbReaderAsync<T>(DbSet<T> dbSet, CancellationToken cancellationToken)
            where T : Model, new();

        internal IMockDb Mock { get; set; }

        protected internal abstract object CreateMockDb();

        protected internal abstract string GetMockTableName(string tableName, object tag);

        protected internal static void ForeignKey<TKey>(string constraintName, TKey foreignKey, Model<TKey> refTableModel, ForeignKeyAction onDelete, ForeignKeyAction onUpdate)
            where TKey : ModelKey
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
