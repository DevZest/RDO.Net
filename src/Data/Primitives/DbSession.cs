using System;
using System.Threading.Tasks;
using System.Threading;
using DevZest.Data.Addons;
using System.Runtime.CompilerServices;
using DevZest.Data.Annotations.Primitives;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents database session.
    /// </summary>
    public abstract partial class DbSession : AddonBag, IDisposable
    {
        #region IDisposable

        /// <summary>
        /// Releases the resources owned by this database session.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer of this class.
        /// </summary>
        ~DbSession()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases the resources owned by this database session.
        /// </summary>
        /// <param name="disposing">If set to <see langword="true" />, the method is invoked directly and will dispose manage
        /// and unmanaged resources; If set to <see langword="false"/> the method is being called by the garbage collector finalizer
        /// and should only release unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                CloseConnection();
        }

        #endregion

        /// <summary>
        /// Gets the ADO.Net database connection.
        /// </summary>
        public DbConnection Connection => GetConnection();

        internal abstract DbConnection GetConnection();

        /// <summary>
        /// Opens the database connection.
        /// </summary>
        /// <param name="cancellationToken">The async cancellation token.</param>
        /// <returns>The async task.</returns>
        public abstract Task OpenConnectionAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public abstract void CloseConnection();

        /// <summary>
        /// Assigns name to temporary table.
        /// </summary>
        /// <param name="model">The model of the temporary table.</param>
        /// <returns>The name assigned to the temporary table</returns>
        protected internal abstract string AssignTempTableName(Model model);

        internal abstract Task CreateTableAsync(Model model, bool isTempTable, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the native SQL string for query statement.
        /// </summary>
        /// <param name="query">The query statement.</param>
        /// <returns>The native SQL string.</returns>
        protected internal abstract string GetSqlString(DbQueryStatement query);

        /// <summary>
        /// Gets the DbTable object.
        /// </summary>
        /// <typeparam name="T">Model type of the table.</typeparam>
        /// <param name="cachedValue">The reference of cached value.</param>
        /// <param name="name">The name of the database table.</param>
        /// <returns>The DbTable object.</returns>
        protected DbTable<T> GetTable<T>(ref DbTable<T> cachedValue, [CallerMemberName]string name = null)
            where T : Model, new()
        {
            if (Generator != null)
                return Generator.GetTable<T>(name);

            if (cachedValue == null)
            {
                name.VerifyNotEmpty(nameof(name));
                var model = new T();
                cachedValue = DbTable<T>.Create(model, this, name, DbTablePropertyAttribute.WireupAttributes);
            }
            return cachedValue;
        }

        /// <summary>
        /// Creates temporary database table.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The created temporary table.</returns>
        public Task<DbTable<T>> CreateTempTableAsync<T>(CancellationToken ct = default(CancellationToken))
            where T : class, IEntity, new()
        {
            return CreateTempTableAsync<T>(null, null, ct);
        }

        /// <summary>
        /// Create temporary database table for specified entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="_">The specified entity.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The created temporary table.</returns>
        public Task<DbTable<T>> CreateTempTableAsync<T>(T _, CancellationToken ct = default(CancellationToken))
            where T : class, IEntity, new()
        {
            return CreateTempTableAsync<T>(_, null, ct);
        }

        /// <summary>
        /// Creates temporary database table.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="initializer">The entity initializer.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The created temporary table.</returns>
        public Task<DbTable<T>> CreateTempTableAsync<T>(Action<T> initializer, CancellationToken ct = default(CancellationToken))
            where T : class, IEntity, new()
        {
            return CreateTempTableAsync<T>(null, initializer, ct);
        }

        /// <summary>
        /// Create temporary database table for specified entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="_">The specified entity.</param>
        /// <param name="initializer">The entity initializer.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The created temporary table.</returns>
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

        /// <summary>
        /// Creates database query.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="buildQuery">The query builder.</param>
        /// <returns>The created database query.</returns>
        public DbQuery<T> CreateQuery<T>(Action<DbQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            return CreateQuery(null, null, buildQuery);
        }

        /// <summary>
        /// Creates database query for specified entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="_">The specified entity.</param>
        /// <param name="buildQuery">The query builder.</param>
        /// <returns>The created database query.</returns>
        public DbQuery<T> CreateQuery<T>(T _, Action<DbQueryBuilder, T> buildQuery)
           where T : class, IEntity, new()
        {
            return CreateQuery(_, null, buildQuery);
        }

        /// <summary>
        /// Creates database query.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="initializer">The entity initializer.</param>
        /// <param name="buildQuery">The query builder.</param>
        /// <returns>The created database query.</returns>
        public DbQuery<T> CreateQuery<T>(Action<T> initializer, Action<DbQueryBuilder, T> buildQuery)
           where T : class, IEntity, new()
        {
            return CreateQuery(null, initializer, buildQuery);
        }

        /// <summary>
        /// Creates database query for specified entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="_">The specified entity.</param>
        /// <param name="initializer">The entity initializer.</param>
        /// <param name="buildQuery">The query builder.</param>
        /// <returns>The created database query.</returns>
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

        /// <summary>
        /// Creates aggregate database query.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="buildQuery">The aggregate query builder.</param>
        /// <returns>The created database query.</returns>
        public DbQuery<T> CreateAggregateQuery<T>(Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            return CreateAggregateQuery(null, null, buildQuery);
        }

        /// <summary>
        /// Creates aggregate database query for specified entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="_">The specified entity.</param>
        /// <param name="buildQuery">The aggregate query builder.</param>
        /// <returns>The created database query.</returns>
        public DbQuery<T> CreateAggregateQuery<T>(T _, Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            return CreateAggregateQuery(_, null, buildQuery);
        }

        /// <summary>
        /// Creates aggregate database query.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="initializer">The entity initializer.</param>
        /// <param name="buildQuery">The aggregate query builder.</param>
        /// <returns>The created database query.</returns>
        public DbQuery<T> CreateAggregateQuery<T>(Action<T> initializer, Action<DbAggregateQueryBuilder, T> buildQuery)
            where T : class, IEntity, new()
        {
            return CreateAggregateQuery(null, initializer, buildQuery);
        }

        /// <summary>
        /// Creates aggregate database query for specified entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="_">The specified entity.</param>
        /// <param name="initializer">The entity initializer.</param>
        /// <param name="buildQuery">The aggregate query builder.</param>
        /// <returns>The created database query.</returns>
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

        /// <summary>
        /// Executes query to insert scalar values into table.
        /// </summary>
        /// <param name="statement">The SQL SELECT statement.</param>
        /// <param name="outputIdentity">Specifies whether newly generated identity value should be returned.</param>
        /// <param name="cancellationToken">The async cancellation token.</param>
        /// <returns>The <see cref="InsertScalarResult"/>.</returns>
        protected internal abstract Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken cancellationToken);

        /// <summary>
        /// Executes query to insert DataSet data into table.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <typeparam name="TTarget">Entity type of target table.</typeparam>
        /// <param name="sourceData">The source DataSet.</param>
        /// <param name="targetTable">The target database table.</param>
        /// <param name="columnMapper">Provides column mappings between source DataSet and target database table.</param>
        /// <param name="updateIdentity">Specifies whether newly generated identity values should be updated back to the DataSet.</param>
        /// <param name="cancellationToken">The async cancellation token.</param>
        /// <returns>Number of rows inserted.</returns>
        protected internal abstract Task<int> InsertAsync<TSource, TTarget>(DataSet<TSource> sourceData, DbTable<TTarget> targetTable,
            Action<ColumnMapper, TSource, TTarget> columnMapper, bool updateIdentity, CancellationToken cancellationToken)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new();

        internal abstract Task<int> UpdateAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        /// <summary>
        /// Executes query to update database table from DataSet.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <typeparam name="TTarget">Entity type of target database table.</typeparam>
        /// <param name="source">The source DataSet.</param>
        /// <param name="target">The target database table.</param>
        /// <param name="columnMapper">Provides column mappings between source DataSet and target database table.</param>
        /// <param name="targetKey">The key of target database table.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of rows updated.</returns>
        protected internal abstract Task<int> UpdateAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target,
            Action<ColumnMapper, TSource, TTarget> columnMapper, CandidateKey targetKey, CancellationToken ct)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new();

        internal abstract Task<int> DeleteAsync(DbSelectStatement statement, CancellationToken cancellationToken);

        /// <summary>
        /// Executes query to delete database table data from DataSet.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DataSet.</typeparam>
        /// <typeparam name="TTarget">Entity type of target database table.</typeparam>
        /// <param name="source">The source DataSet.</param>
        /// <param name="target">The target database table.</param>
        /// <param name="targetKey">The key of target database table.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of rows deleted.</returns>
        protected internal abstract Task<int> DeleteAsync<TSource, TTarget>(DataSet<TSource> source, DbTable<TTarget> target, CandidateKey targetKey, CancellationToken ct)
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

        /// <summary>
        /// Creates mock database.
        /// </summary>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>A tag object which will be passed to <see cref="GetMockTableName(string, object)"/>.</returns>
        protected internal abstract Task<object> CreateMockDbAsync(CancellationToken ct);

        /// <summary>
        /// Gets the name of mock table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="tag">The tag object returned by <see cref="CreateMockDbAsync(CancellationToken)"/>.</param>
        /// <returns>The name of the mock table.</returns>
        protected internal abstract string GetMockTableName(string tableName, object tag);

        /// <summary>
        /// Sets the logger.
        /// </summary>
        /// <param name="value">The delegate to receive the logging messages.</param>
        public void SetLogger(Action<string> value)
        {
            SetLogger(value, LogCategory.All);
        }

        /// <summary>
        /// Sets the logger.
        /// </summary>
        /// <param name="value">The delegate to receive the logging messages.</param>
        /// <param name="logCategory">Specifies logging category.</param>
        public abstract void SetLogger(Action<string> value, LogCategory logCategory);
    }
}
