using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public sealed class DbQuery<T> : DbSet<T>
        where T : Model, new()
    {
        internal DbQuery(T model, DbSession dbSession, DbQueryStatement queryStatement)
            : base(model, dbSession)
        {
            _originalQueryStatement = queryStatement;
            this.Model.SetDataSource(this);
        }

        private DbQueryStatement _originalQueryStatement;
        private DbQueryStatement _queryStatement;
        internal override DbQueryStatement QueryStatement
        {
            get
            {
                if (_queryStatement == null)
                    _queryStatement = _originalQueryStatement.RemoveSystemColumns();
                return _queryStatement;
            }
        }

        public sealed override DataSourceKind Kind
        {
            get { return DataSourceKind.DbQuery; }
        }

        internal override DbFromClause FromClause
        {
            get { return QueryStatement; }
        }

        private DbQueryStatement _sequentialQueryStatement;
        internal override DbQueryStatement SequentialQueryStatement
        {
            get
            {
                if (_sequentialQueryStatement == null)
                {
                    var sequentialKeys = QueryStatement.SequentialKeyTempTable;
                    if (sequentialKeys == null)
                        _sequentialQueryStatement = _originalQueryStatement;
                    else
                        _sequentialQueryStatement = _originalQueryStatement.BuildQueryStatement(Model.Clone(false), null, sequentialKeys);
                }
                return _sequentialQueryStatement;
            }
        }

        public override string ToString()
        {
            return DbSession.GetSqlString(QueryStatement);
        }

        private void EnsureSequentialTempTableCreated(DbSession dbSession)
        {
            QueryStatement.EnsureSequentialTempTableCreated(dbSession);
        }

        private Task EnsureSequentialTempTableCreatedAsync(DbSession dbSession, CancellationToken cancellationToken)
        {
            return QueryStatement.EnsureSequentialTempTableCreatedAsync(dbSession, cancellationToken);
        }

        private DbTable<KeyOutput> SequentialKeyTempTable
        {
            get { return QueryStatement.SequentialKeyTempTable; }
        }

        public override int Count()
        {
            // If SequentialKeyTempTable created, return its InitialRowCount directly. This will save one database query.
            return SequentialKeyTempTable == null ? base.Count() : SequentialKeyTempTable.InitialRowCount;
        }

        public override Task<int> CountAsync(CancellationToken cancellationToken)
        {
            // If SequentialKeyTempTable created, return its InitialRowCount directly. This will save one database query.
            return SequentialKeyTempTable == null ? base.CountAsync(cancellationToken) : Task.FromResult(SequentialKeyTempTable.InitialRowCount);
        }

        public DbQuery<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData)
            where TChild : Model, new()
        {
            return CreateChild(null, getChildModel, sourceData);
        }


        public DbQuery<TChild> CreateChild<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, DbSet<TChild> sourceData)
            where TChild : Model, new()
        {
            Check.NotNull(sourceData, nameof(sourceData));
            var model = VerifyCreateChild(initializer, getChildModel);

            EnsureSequentialTempTableCreated(DbSession);
            if (SequentialKeyTempTable.InitialRowCount == 0)
                return null;
            return DbSession.PerformCreateQuery(model, sourceData.QueryStatement.BuildQueryStatement(model, null, null));
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData)
            where TChild : Model, new()
        {
            return CreateChildAsync(getChildModel, sourceData, CancellationToken.None);
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            return CreateChildAsync(null, getChildModel, sourceData, cancellationToken);
        }

        public async Task<DbQuery<TChild>> CreateChildAsync<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            Check.NotNull(sourceData, nameof(sourceData));
            var model = VerifyCreateChild(initializer, getChildModel);

            await EnsureSequentialTempTableCreatedAsync(DbSession, cancellationToken);
            if (SequentialKeyTempTable.InitialRowCount == 0)
                return null;
            return DbSession.PerformCreateQuery(model, sourceData.QueryStatement.BuildQueryStatement(model, null, null));
        }

        public DbQuery<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            return CreateChild(null, getChildModel, buildQuery);
        }

        public DbQuery<TChild> CreateChild<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(initializer, getChildModel);

            EnsureSequentialTempTableCreated(DbSession);
            if (SequentialKeyTempTable.InitialRowCount == 0)
                return null;
            var queryBuilder = new DbQueryBuilder(childModel);
            buildQuery(queryBuilder, childModel);
            return DbSession.PerformCreateQuery(childModel, queryBuilder.BuildQueryStatement(null));
        }

        public DbQuery<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            return CreateChild(null, getChildModel, buildQuery);
        }

        public DbQuery<TChild> CreateChild<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(initializer, getChildModel);

            EnsureSequentialTempTableCreated(DbSession);
            if (SequentialKeyTempTable.InitialRowCount == 0)
                return null;
            var queryBuilder = new DbAggregateQueryBuilder(childModel);
            buildQuery(queryBuilder, childModel);
            return DbSession.PerformCreateQuery(childModel, queryBuilder.BuildQueryStatement(null));
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            return CreateChildAsync(getChildModel, buildQuery, CancellationToken.None);
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            return CreateChildAsync(getChildModel, buildQuery, CancellationToken.None);
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            return CreateChildAsync(null, getChildModel, buildQuery, cancellationToken);
        }

        public async Task<DbQuery<TChild>> CreateChildAsync<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(initializer, getChildModel);

            await EnsureSequentialTempTableCreatedAsync(DbSession, cancellationToken);
            if (SequentialKeyTempTable.InitialRowCount == 0)
                return null;
            var queryBuilder = new DbQueryBuilder(childModel);
            buildQuery(queryBuilder, childModel);
            return DbSession.PerformCreateQuery(childModel, queryBuilder.BuildQueryStatement(null));
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            return CreateChildAsync(null, getChildModel, buildQuery, cancellationToken);
        }


        public async Task<DbQuery<TChild>> CreateChildAsync<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(initializer, getChildModel);

            await EnsureSequentialTempTableCreatedAsync(DbSession, cancellationToken);
            if (SequentialKeyTempTable.InitialRowCount == 0)
                return null;
            var queryBuilder = new DbAggregateQueryBuilder(childModel);
            buildQuery(queryBuilder, childModel);
            return DbSession.PerformCreateQuery(childModel, queryBuilder.BuildQueryStatement(null));
        }

        public DbQuery<TChild> GetChild<TChild>(Func<T, TChild> getChildModel)
            where TChild : Model, new()
        {
            Check.NotNull(getChildModel, nameof(getChildModel));
            var childModel = getChildModel(_);
            if (childModel == null)
                return null;
            return childModel.DataSource as DbQuery<TChild>;
        }
    }
}
