using DevZest.Data.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public sealed class DbQuery<T> : DbSet<T>
        where T : class, IModelReference, new()
    {
        internal DbQuery(T modelRef, DbSession dbSession, DbQueryStatement queryStatement)
            : base(modelRef, dbSession)
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

        private Task EnsureSequentialTempTableCreatedAsync(DbSession dbSession, CancellationToken cancellationToken)
        {
            return QueryStatement.EnsureSequentialTempTableCreatedAsync(dbSession, cancellationToken);
        }

        private DbTable<SequentialKey> SequentialKeyTempTable
        {
            get { return QueryStatement.SequentialKeyTempTable; }
        }

        public override Task<int> CountAsync(CancellationToken ct = default(CancellationToken))
        {
            // If SequentialKeyTempTable created, return its InitialRowCount directly. This will save one database query.
            return SequentialKeyTempTable == null ? base.CountAsync(ct) : Task.FromResult(SequentialKeyTempTable.InitialRowCount);
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData, CancellationToken ct = default(CancellationToken))
            where TChild : Model, new()
        {
            return CreateChildAsync(null, getChildModel, sourceData, ct);
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery, CancellationToken ct = default(CancellationToken))
            where TChild : Model, new()
        {
            return CreateChildAsync(null, getChildModel, buildQuery, ct);
        }

        public async Task<DbQuery<TChild>> CreateChildAsync<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, CancellationToken ct = default(CancellationToken))
            where TChild : Model, new()
        {
            sourceData.VerifyNotNull(nameof(sourceData));
            var model = VerifyCreateChild(initializer, getChildModel);

            await EnsureSequentialTempTableCreatedAsync(DbSession, ct);
            if (SequentialKeyTempTable.InitialRowCount == 0)
                return null;
            return DbSession.PerformCreateQuery(model, sourceData.QueryStatement.BuildQueryStatement(model, null, null));
        }

        public async Task<DbQuery<TChild>> CreateChildAsync<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery, CancellationToken ct = default(CancellationToken))
            where TChild : Model, new()
        {
            buildQuery.VerifyNotNull(nameof(buildQuery));
            var childModel = VerifyCreateChild(initializer, getChildModel);

            await EnsureSequentialTempTableCreatedAsync(DbSession, ct);
            if (SequentialKeyTempTable.InitialRowCount == 0)
                return null;
            var queryBuilder = new DbQueryBuilder(childModel);
            buildQuery(queryBuilder, childModel);
            return DbSession.PerformCreateQuery(childModel, queryBuilder.BuildQueryStatement(null));
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery, CancellationToken ct = default(CancellationToken))
            where TChild : Model, new()
        {
            return CreateChildAsync(null, getChildModel, buildQuery, ct);
        }


        public async Task<DbQuery<TChild>> CreateChildAsync<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery, CancellationToken ct = default(CancellationToken))
            where TChild : Model, new()
        {
            buildQuery.VerifyNotNull(nameof(buildQuery));
            var childModel = VerifyCreateChild(initializer, getChildModel);

            await EnsureSequentialTempTableCreatedAsync(DbSession, ct);
            if (SequentialKeyTempTable.InitialRowCount == 0)
                return null;
            var queryBuilder = new DbAggregateQueryBuilder(childModel);
            buildQuery(queryBuilder, childModel);
            return DbSession.PerformCreateQuery(childModel, queryBuilder.BuildQueryStatement(null));
        }

        public DbQuery<TChild> GetChild<TChild>(Func<T, TChild> getChildModel)
            where TChild : Model, new()
        {
            getChildModel.VerifyNotNull(nameof(getChildModel));
            var childModel = getChildModel(_);
            if (childModel == null)
                return null;
            return childModel.DataSource as DbQuery<TChild>;
        }
    }
}
