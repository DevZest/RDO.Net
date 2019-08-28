using System;
using System.Threading.Tasks;
using System.Threading;
using DevZest.Data.Addons;
using System.Runtime.CompilerServices;
using DevZest.Data.Annotations.Primitives;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public abstract partial class DbSession : AddonBag, IDisposable
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

        public DbConnection Connection => GetConnection();

        internal abstract DbConnection GetConnection();

        public abstract Task OpenConnectionAsync(CancellationToken cancellationToken = default(CancellationToken));

        public abstract void CloseConnection();

        protected internal abstract string AssignTempTableName(Model model);

        internal abstract Task CreateTableAsync(Model model, bool isTempTable, CancellationToken cancellationToken);

        protected internal abstract string GetSqlString(DbQueryStatement query);

        protected DbTable<T> GetTable<T>(ref DbTable<T> result, [CallerMemberName]string name = null)
            where T : Model, new()
        {
            if (Generator != null)
                return Generator.GetTable<T>(name);

            if (result == null)
            {
                name.VerifyNotEmpty(nameof(name));
                var model = new T();
                result = DbTable<T>.Create(model, this, name, DbTablePropertyAttribute.WireupAttributes);
            }
            return result;
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(CancellationToken ct = default(CancellationToken))
            where T : class, IEntity, new()
        {
            return CreateTempTableAsync<T>(null, null, ct);
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(T _, CancellationToken ct = default(CancellationToken))
            where T : class, IEntity, new()
        {
            return CreateTempTableAsync<T>(_, null, ct);
        }

        public Task<DbTable<T>> CreateTempTableAsync<T>(Action<T> initializer, CancellationToken ct = default(CancellationToken))
            where T : class, IEntity, new()
        {
            return CreateTempTableAsync<T>(null, initializer, ct);
        }

        public async Task<DbTable<T>> CreateTempTableAsync<T>(T _, Action<T> initializer, CancellationToken ct = default(CancellationToken))
            where T : class, IEntity, new()
        {
            var result = CreateTempTableInstance(_, initializer);
            await CreateTableAsync(result._.Model, true, ct);
            return result;
        }

        internal DbQuery<T> PerformCreateQuery<T>(T _, DbQueryStatement queryStatement)
            where T : class, IEntity, new()
        {
            return new DbQuery<T>(_, this, queryStatement);
        }

        public DbQuery<T> CreateQuery<T>(Action<DbQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            return CreateQuery(null, null, buildQuery);
        }

        public DbQuery<T> CreateQuery<T>(T _, Action<DbQueryBuilder, T> buildQuery)
           where T : class, IEntity, new()
        {
            return CreateQuery(_, null, buildQuery);
        }

        public DbQuery<T> CreateQuery<T>(Action<T> initializer, Action<DbQueryBuilder, T> buildQuery)
           where T : class, IEntity, new()
        {
            return CreateQuery(null, initializer, buildQuery);
        }

        public DbQuery<T> CreateQuery<T>(T _, Action<T> initializer, Action<DbQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            buildQuery.VerifyNotNull(nameof(buildQuery));

            var modelRef = _ == null ? new T() : (_.Model.DataSource == null ? _ : _.MakeCopy(false));
            modelRef.Initialize(initializer);
            var builder = new DbQueryBuilder(modelRef.Model);
            buildQuery(builder, modelRef);
            return PerformCreateQuery(modelRef, builder.BuildQueryStatement(null));
        }

        public DbQuery<T> CreateAggregateQuery<T>(Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            return CreateAggregateQuery(null, null, buildQuery);
        }

        public DbQuery<T> CreateAggregateQuery<T>(T _, Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            return CreateAggregateQuery(_, null, buildQuery);
        }

        public DbQuery<T> CreateAggregateQuery<T>(Action<T> initializer, Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            return CreateAggregateQuery(null, initializer, buildQuery);
        }

        public DbQuery<T> CreateAggregateQuery<T>(T _, Action<T> initializer, Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            buildQuery.VerifyNotNull(nameof(buildQuery));

            var modelRef = _ == null ? new T() : (_.Model.DataSource == null ? _ : _.MakeCopy(false));
            modelRef.Initialize(initializer);
            var builder = new DbAggregateQueryBuilder(modelRef.Model);
            buildQuery(builder, modelRef);
            return PerformCreateQuery(modelRef, builder.BuildQueryStatement(null));
        }

        internal abstract Task RecursiveFillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken);

        internal abstract Task FillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken);

        internal DbTable<T> CreateTempTableInstance<T>(T _, Action<T> initializer)
            where T : class, IEntity, new()
        {
            var modelRef = _ == null ? new T() : _.MakeCopy(false);
            modelRef.Initialize(initializer);
            modelRef.Model.AddTempTableIdentity();
            return CreateTempTableInstance(modelRef);
        }

        private DbTable<T> CreateTempTableInstance<T>(T _)
            where T : class, IEntity, new()
        {
            var tableName = AssignTempTableName(_.Model);
            return DbTable<T>.CreateTemp(_, this, tableName);
        }

        private async Task<DbTable<KeyOutput>> CreateKeyOutputAsync(Model sourceModel, CancellationToken cancellationToken)
        {
            var keyOutput = new KeyOutput(sourceModel);
            var result = CreateTempTableInstance(keyOutput);
            await CreateTableAsync(keyOutput, true, cancellationToken);
            return result;
        }

        internal abstract Task<int> InsertAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        protected internal abstract Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken cancellationToken);

        protected internal abstract Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMapper, TSource, TTarget> columnMapper, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new();

        internal abstract Task<int> UpdateAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        protected internal abstract Task<int> UpdateAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey joinTo, CancellationToken ct)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new();

        internal abstract Task<int> DeleteAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        protected internal abstract Task<int> DeleteAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey joinTo, CancellationToken ct)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new();

        internal abstract Task<DbReader> ExecuteDbReaderAsync<T>(DbSet<T> dbSet, CancellationToken cancellationToken)
            where T : class, IEntity, new();

        internal DbInitializer Generator { get; set; }

        private bool HasGenerator
        {
            get { return Generator != null; }
        }

        internal void VerifyNoGenerator()
        {
            if (HasGenerator)
                throw new InvalidOperationException(DiagnosticMessages.DbSession_VerifyNoGenerator);
        }

        protected internal abstract Task<object> CreateMockDbAsync(CancellationToken ct);

        protected internal abstract string GetMockTableName(string tableName, object tag);

        protected internal static DbForeignKeyConstraint CreateForeignKeyConstraint<TKey>(string name, string description, TKey foreignKey, Model<TKey> refTableModel, ForeignKeyRule deleteRule, ForeignKeyRule updateRule)
            where TKey : CandidateKey
        {
            foreignKey.VerifyNotNull(nameof(foreignKey));
            refTableModel.VerifyNotNull(nameof(refTableModel));

            var model = foreignKey.ParentModel;
            var foreignKeyConstraint = new DbForeignKeyConstraint(name, description, foreignKey, refTableModel.PrimaryKey, deleteRule, updateRule);
            if (refTableModel != model && string.IsNullOrEmpty(foreignKeyConstraint.ReferencedTableName))
                throw new ArgumentException(DiagnosticMessages.Model_InvalidRefTableModel, nameof(refTableModel));
            return foreignKeyConstraint;
        }

        public void SetLogger(Action<string> value)
        {
            SetLogger(value, LogCategory.All);
        }

        public abstract void SetLogger(Action<string> value, LogCategory logCategory);
    }
}
