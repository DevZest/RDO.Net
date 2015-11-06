using System;
using DevZest.Data.Utilities;
using System.Threading.Tasks;
using System.Threading;
using System.Data;

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
            return NewTempTable<T>(initializer, true);
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(Action<T> initializer = null)
            where T : Model, new()
        {
            return CreateTempTableAsync(initializer, CancellationToken.None);
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(Action<T> initializer, CancellationToken cancellationToken)
            where T : Model, new()
        {
            return NewTempTableAsync(initializer, true, cancellationToken);
        }

        public DbTable<T> CreateTempTable<T>(Action<DbQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = new T();
            var builder = new DbQueryBuilder(this, model);
            buildQuery(builder, model);
            return builder.ToTempTable<T>(model);
        }

        public DbTable<T> CreateTempTable<T>(Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = new T();
            var builder = new DbAggregateQueryBuilder(this, model);
            buildQuery(builder, model);
            return builder.ToTempTable<T>(model);
        }


        public Task<DbTable<T>> CreateTempTableAsync<T>(Action<DbQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            return CreateTempTableAsync<T>(buildQuery, CancellationToken.None);
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            return CreateTempTableAsync<T>(buildQuery, CancellationToken.None);
        }

        public async Task<DbTable<T>> CreateTempTableAsync<T>(Action<DbQueryBuilder, T> buildQuery, CancellationToken cancellationToken)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = new T();
            var builder = new DbQueryBuilder(this, model);
            buildQuery(builder, model);
            return await builder.ToTempTableAsync(model, cancellationToken);
        }

        public async Task<DbTable<T>> CreateTempTableAsync<T>(Action<DbAggregateQueryBuilder, T> buildQuery, CancellationToken cancellationToken)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = new T();
            var builder = new DbAggregateQueryBuilder(this, model);
            buildQuery(builder, model);
            return await builder.ToTempTableAsync(model, cancellationToken);
        }

        public DbQuery<T> CreateQuery<T>(Action<DbQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = new T();
            var builder = new DbQueryBuilder(this, model);
            buildQuery(builder, model);
            return builder.ToQuery<T>(model);
        }

        public DbQuery<T> CreateQuery<T>(Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));

            var model = new T();
            var builder = new DbAggregateQueryBuilder(this, model);
            buildQuery(builder, model);
            return builder.ToQuery<T>(model);
        }

        internal abstract void RecursiveFillDataSet(IDbSet dbSet, DataSet dataSet);

        internal abstract Task RecursiveFillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken);

        internal abstract void FillDataSet(IDbSet dbSet, DataSet dataSet);

        internal abstract Task FillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken);

        protected internal abstract bool ImportDataSetAsTempTable { get; }

        protected internal abstract DbSet<T> ImportDataSet<T>(DataSet<T> dataSet)
            where T : Model, new();

        protected internal abstract Task<DbSet<T>> ImportDataSetAsync<T>(DataSet<T> dataSet, CancellationToken cancellationToken)
            where T : Model, new();

        internal DbTable<T> NewTempTable<T>(Action<T> initializer = null, bool addRowId = true)
            where T : Model, new()
        {
            var result = NewTempTableObject(initializer, addRowId);
            CreateTable(result._, result.Name, true);
            return result;
        }

        internal async Task<DbTable<T>> NewTempTableAsync<T>(Action<T> initializer, bool addRowId, CancellationToken cancellationToken)
            where T : Model, new()
        {
            var result = NewTempTableObject(initializer, addRowId);
            await CreateTableAsync(result._, result.Name, true, cancellationToken);
            return result;
        }

        internal DbTable<T> NewTempTableObject<T>(Action<T> initializer = null, bool addRowId = true)
            where T : Model, new()
        {
            var model = new T();
            if (initializer != null)
                initializer(model);
            if (addRowId)
                model.AddTempTableIdentity();
            var tableName = AssignTempTableName(model);
            return DbTable<T>.CreateTemp(model, this, tableName);
        }

        internal abstract int Insert(DbSelectStatement statement);

        internal abstract Task<int> InsertAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        protected internal abstract int Insert<T, TSource>(DbTable<T> targetTable, DbSet<TSource> sourceData, DbSelectStatement statement, DbTable<IdentityMapping> identityMappings)
            where T : Model, new()
            where TSource : Model, new();

        protected internal abstract Task<int> InsertAsync<T, TSource>(DbTable<T> targetTable, DbSet<TSource> sourceData, DbSelectStatement statement, DbTable<IdentityMapping> identityMappings, CancellationToken cancellationToken)
            where T : Model, new()
            where TSource : Model, new();

        internal abstract InsertScalarResult InsertScalar(DbSelectStatement statement, bool outputIdentity);

        internal abstract Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken cancellationToken);

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
