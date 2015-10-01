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
        public override DbQueryStatement QueryStatement
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

        public override DbFromClause FromClause
        {
            get { return QueryStatement; }
        }

        private DbQueryStatement _sequentialQueryStatement;
        public override DbQueryStatement SequentialQueryStatement
        {
            get
            {
                if (_sequentialQueryStatement == null)
                {
                    if (QueryStatement.SequentialKeyTempTable == null)
                        _sequentialQueryStatement = _originalQueryStatement;
                    else
                        _sequentialQueryStatement = _originalQueryStatement.MakeQueryBuilder(DbSession, Model.Clone(false), true).BuildQueryStatement(false);
                }
                return _sequentialQueryStatement;
            }
        }

        public override string ToString()
        {
            return DbSession.GetSqlString(QueryStatement);
        }

        public DbQuery<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData)
            where TChild : Model, new()
        {
            Check.NotNull(sourceData, nameof(sourceData));
            var model = VerifyCreateChild(getChildModel);

            QueryStatement.EnsureSequentialTempTableCreated(DbSession);
            return sourceData.QueryStatement.MakeQueryBuilder(DbSession, model, false).ToQuery<TChild>(model);
        }

        public Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData)
            where TChild : Model, new()
        {
            return CreateChildAsync(getChildModel, sourceData, CancellationToken.None);
        }

        public async Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            Check.NotNull(sourceData, nameof(sourceData));
            var model = VerifyCreateChild(getChildModel);

            await QueryStatement.EnsureSequentialTempTableCreatedAsync(DbSession, cancellationToken);
            return sourceData.QueryStatement.MakeQueryBuilder(DbSession, model, false).ToQuery<TChild>(model);
        }

        public DbQuery<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(getChildModel);

            QueryStatement.EnsureSequentialTempTableCreated(DbSession);
            var queryBuilder = new DbQueryBuilder(DbSession, childModel);
            buildQuery(queryBuilder, childModel);
            return queryBuilder.ToQuery<TChild>(childModel);
        }

        public DbQuery<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(getChildModel);

            QueryStatement.EnsureSequentialTempTableCreated(DbSession);
            var queryBuilder = new DbAggregateQueryBuilder(DbSession, childModel);
            buildQuery(queryBuilder, childModel);
            return queryBuilder.ToQuery<TChild>(childModel);
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

        public async Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(getChildModel);

            await QueryStatement.EnsureSequentialTempTableCreatedAsync(DbSession, cancellationToken);
            var queryBuilder = new DbQueryBuilder(DbSession, childModel);
            buildQuery(queryBuilder, childModel);
            return queryBuilder.ToQuery<TChild>(childModel);
        }

        public async Task<DbQuery<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(getChildModel);

            await QueryStatement.EnsureSequentialTempTableCreatedAsync(DbSession, cancellationToken);
            var queryBuilder = new DbAggregateQueryBuilder(DbSession, childModel);
            buildQuery(queryBuilder, childModel);
            return queryBuilder.ToQuery<TChild>(childModel);
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

        public override int GetInitialRowCount()
        {
            var select = QueryStatement;
            select.EnsureSequentialTempTableCreated(DbSession);
            return select.SequentialKeyTempTable.InitialRowCount;
        }

        public override async Task<int> GetInitialRowCountAsync(CancellationToken cancellationToken)
        {
            var select = QueryStatement;
            await select.EnsureSequentialTempTableCreatedAsync(DbSession, cancellationToken);
            return select.SequentialKeyTempTable.InitialRowCount;
        }

        internal override Column GetSourceColumn(int ordinal)
        {
            return _originalQueryStatement.GetSourceColumn(ordinal);
        }
    }
}
