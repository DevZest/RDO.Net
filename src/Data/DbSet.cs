using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>
    /// Represents database recordset.
    /// </summary>
    /// <typeparam name="T">Entity type of database recordset.</typeparam>
    public abstract class DbSet<T> : DataSource, IDbSet
        where T : class, IEntity, new()
    {
        internal DbSet(T modelRef, DbSession dbSession)
        {
            Debug.Assert(dbSession != null);
            DbSession = dbSession;
            _ = modelRef;
        }

        /// <summary>
        /// Gets the database session which owns this recordset.
        /// </summary>
        public DbSession DbSession { get; private set; }

        /// <summary>
        /// Gets the entity of this database recordset.
        /// </summary>
        public T _ { get; private set; }

        /// <inheritdoc />
        public sealed override Model Model
        {
            get { return _.Model; }
        }

        internal abstract DbQueryStatement QueryStatement { get; }

        internal abstract DbQueryStatement SequentialQueryStatement { get; }

        internal abstract DbFromClause FromClause { get; }

        DbQueryStatement IDbSet.QueryStatement
        {
            get { return QueryStatement; }
        }

        DbFromClause IDbSet.FromClause
        {
            get { return FromClause; }
        }

        DbQueryStatement IDbSet.SequentialQueryStatement
        {
            get { return SequentialQueryStatement; }
        }

        /// <summary>
        /// Filters this database recordset.
        /// </summary>
        /// <param name="predicate">The filter condition.</param>
        /// <returns>The query which filters this database recordset.</returns>
        public DbQuery<T> Where(Func<T, _Boolean> predicate)
        {
            predicate.VerifyNotNull(nameof(predicate));
            return Where(predicate(_));
        }

        private Action<DbQueryBuilder> GetWhereQueryBuilder(_Boolean condition)
        {
            if (condition is null)
                return x => { };
            else
                return x => x.Where(condition);
        }

        private DbQuery<T> Where(_Boolean condition)
        {
            var queryStatement = GetSimpleQueryStatement(GetWhereQueryBuilder(condition), out T newModel);
            return DbSession.PerformCreateQuery(newModel, queryStatement);
        }

        /// <summary>
        /// Sorts this database recordset.
        /// </summary>
        /// <param name="fnOrderByList">The order by list to sort.</param>
        /// <returns>The query which sorts this database recordset.</returns>
        public DbQuery<T> OrderBy(params Func<T, ColumnSort>[] fnOrderByList)
        {
            return OrderBy(-1, -1, fnOrderByList);
        }

        /// <summary>
        /// Sorts this database recordset.
        /// </summary>
        /// <param name="offset">Specifies how many rows to skip within the query result.</param>
        /// <param name="fetch">Specifies how many rows to skip within the query result.</param>
        /// <param name="fnOrderByList">The order by list to sort.</param>
        /// <returns>The query which sorts this database recordset.</returns>
        public DbQuery<T> OrderBy(int offset, int fetch, params Func<T, ColumnSort>[] fnOrderByList)
        {
            var orderBy = GetOrderBy(fnOrderByList);
            return OrderBy(offset, fetch, orderBy);
        }

        /// <summary>
        /// Sorts this database recordset.
        /// </summary>
        /// <param name="fnOrderByList">The order by list to sort.</param>
        /// <returns>The query which sorts this database recordset.</returns>
        public DbQuery<T> OrderBy(Func<T, ColumnSort[]> fnOrderByList)
        {
            return OrderBy(-1, -1, fnOrderByList);
        }

        /// <summary>
        /// Sorts this database recordset.
        /// </summary>
        /// <param name="offset">Specifies how many rows to skip within the query result.</param>
        /// <param name="fetch">Specifies how many rows to skip within the query result.</param>
        /// <param name="fnOrderByList">The order by list to sort.</param>
        /// <returns>The query which sorts this database recordset.</returns>
        public DbQuery<T> OrderBy(int offset, int fetch, Func<T, ColumnSort[]> fnOrderByList)
        {
            var orderBy = fnOrderByList == null ? Array.Empty<ColumnSort>() : fnOrderByList(_);
            return OrderBy(offset, fetch, orderBy);
        }

        private ColumnSort[] GetOrderBy(Func<T, ColumnSort>[] fnOrderBy)
        {
            var orderBy = fnOrderBy == null ? Array.Empty<ColumnSort>() : new ColumnSort[fnOrderBy.Length];
            for (int i = 0; i < orderBy.Length; i++)
                orderBy[i] = fnOrderBy[i](_);
            return orderBy;
        }

        private DbQuery<T> OrderBy(int offset, int fetch, ColumnSort[] orderBy)
        {
            var queryStatement = GetSimpleQueryStatement(GetOrderByQueryBuilder(offset, fetch, orderBy), out T newModel);
            return DbSession.PerformCreateQuery(newModel, queryStatement);
        }

        private Action<DbQueryBuilder> GetOrderByQueryBuilder(int offset, int fetch, ColumnSort[] orderBy)
        {
            if (orderBy == null || orderBy.Length == 0)
                return x => { };
            else
                return x => x.OrderBy(offset, fetch, orderBy);
        }

        internal DbQueryStatement GetSimpleQueryStatement(Action<DbQueryBuilder> action = null)
        {
            return GetSimpleQueryStatement(action, out T newModel);
        }

        private DbQueryStatement GetSimpleQueryStatement(Action<DbQueryBuilder> action, out T newModelProvider)
        {
            var oldModelProvider = _;
            newModelProvider = oldModelProvider.MakeCopy(false);
            return new DbQueryBuilder(newModelProvider.Model).BuildQueryStatement(oldModelProvider.Model, action, null);
        }

        internal TChild VerifyCreateChild<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel)
            where TChild : Model, new()
        {
            if (Kind == DataSourceKind.DbTable)
                throw new InvalidOperationException(DiagnosticMessages.DbSet_VerifyCreateChild_InvalidDataSourceKind);

            _.Model.EnsureInitialized();

            var childModel = _.Verify(getChildModel, nameof(getChildModel));

            if (childModel.DataSource != null)
                throw new InvalidOperationException(DiagnosticMessages.DbSet_VerifyCreateChild_AlreadyCreated);

            childModel.Initialize(initializer);
            return childModel;
        }

        /// <summary>
        /// Saves this database recordset into DataSet.
        /// </summary>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The saved DataSet.</returns>
        public Task<DataSet<T>> ToDataSetAsync(CancellationToken ct = default(CancellationToken))
        {
            return ToDataSetAsync(null, ct);
        }

        /// <summary>
        /// Saves this database recordset into DataSet.
        /// </summary>
        /// <param name="initializer">The entity initializer.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The saved DataSet.</returns>
        public async Task<DataSet<T>> ToDataSetAsync(Action<T> initializer, CancellationToken ct = default(CancellationToken))
        {
            var result = this.MakeDataSet(initializer);
            await DbSession.RecursiveFillDataSetAsync(this, result, ct);
            return result;
        }

        /// <summary>
        /// Serializes this database recordset into JSON string.
        /// </summary>
        /// <param name="isPretty">Specifies whether serialized JSON string should be indented.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The serialized JSON string.</returns>
        public async Task<string> ToJsonStringAsync(bool isPretty, CancellationToken ct = default(CancellationToken))
        {
            return (await ToDataSetAsync(ct)).ToJsonString(isPretty);
        }

        /// <summary>
        /// Constructs a SQL UNION query.
        /// </summary>
        /// <param name="dbSet">The database recordset to union with.</param>
        /// <returns>The result SQL UNION query.</returns>
        public DbQuery<T> Union(DbSet<T> dbSet)
        {
            return Union(dbSet, DbUnionKind.Union);
        }

        /// <summary>
        /// Constructs a SQL UNION ALL query.
        /// </summary>
        /// <param name="dbSet">The database recordset to union with.</param>
        /// <returns>The result SQL UNION ALL query.</returns>
        public DbQuery<T> UnionAll(DbSet<T> dbSet)
        {
            return Union(dbSet, DbUnionKind.UnionAll);
        }

        private DbQuery<T> Union(DbSet<T> dbSet, DbUnionKind kind)
        {
            dbSet.VerifyNotNull(nameof(dbSet));

            var modelRef = _.MakeCopy(false);
            var queryStatement1 = this.GetSimpleQueryStatement();
            var queryStatement2 = dbSet.GetSimpleQueryStatement();
            return new DbQuery<T>(modelRef, DbSession, new DbUnionStatement(modelRef.Model, queryStatement1, queryStatement2, kind));
        }

        private DbQuery<Adhoc> BuildCountQuery()
        {
            return DbSession.CreateAggregateQuery((DbAggregateQueryBuilder builder, Adhoc adhoc) =>
            {
                builder.From(this, out T m)
                    .Select(this._.Model.Columns[0].CountRows(), adhoc, "Result");
            });
        }

        /// <summary>
        /// Gets the total number of records of this database recordset.
        /// </summary>
        /// <returns>The total number of records of this database recordset.</returns>
        public Task<int> CountAsync()
        {
            return CountAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the total number of records of this database recordset.
        /// </summary>
        /// <param name="cancellationToken">The async cancellation token.</param>
        /// <returns>The total number of records of this database recordset.</returns>
        public virtual async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var query = BuildCountQuery();
            using (var reader = await DbSession.ExecuteDbReaderAsync(query, cancellationToken))
            {
                int? result = null;
                if (await reader.ReadAsync(cancellationToken))
                    result = ((_Int32)query._.Columns[0])[reader];
                return result.HasValue ? result.GetValueOrDefault() : 0;
            }
        }

        private DbFromClause Join(IDbTable dbTable, IList<ColumnMapping> keyMappings)
        {
            return new DbJoinClause(DbJoinKind.InnerJoin, FromClause, dbTable.FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));
        }

        /// <summary>
        /// Generates a database query.
        /// </summary>
        /// <typeparam name="TDerived">The derived entity class.</typeparam>
        /// <returns>The result database query.</returns>
        public DbQuery<TDerived> ToDbQuery<TDerived>()
            where TDerived : class, T, new()
        {
            return DbSession.CreateQuery((DbQueryBuilder builder, TDerived _) =>
            {
                builder.From(this, out var s)
                    .AutoSelect();
            });
        }

        async Task<DataSet> IDbSet.ToDataSetAsync(CancellationToken ct)
        {
            var result = await ToDataSetAsync(ct);
            return result;
        }

        /// <summary>
        /// Executes the database reader.
        /// </summary>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The database reader.</returns>
        public Task<DbReader> ExecuteDbReaderAsync(CancellationToken ct = default(CancellationToken))
        {
            return DbSession.ExecuteDbReaderAsync(this, ct);
        }

        /// <summary>
        /// Gets the entity associated with this database recordset.
        /// </summary>
        public T Entity
        {
            get { return _; }
        }
    }
}
