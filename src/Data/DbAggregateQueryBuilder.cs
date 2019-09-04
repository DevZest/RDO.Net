using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
    /// <summary>
    /// Builds database aggregate query which can be translated to native SQL.
    /// </summary>
    public class DbAggregateQueryBuilder : DbQueryBuilder
    {
        private class GroupByCollection : ReadOnlyCollection<DbExpression>
        {
            public GroupByCollection()
                : base(new List<DbExpression>())
            {
            }

            public void Add(DbExpression item)
            {
                Debug.Assert(item != null);
                Items.Add(item);
            }
        }

        internal DbAggregateQueryBuilder(Model model)
            : base(model)
        {
            AutoGroupBy = true;
        }

        internal override void Initialize(DbSelectStatement query)
        {
            Debug.Assert(query.IsAggregate);

            base.Initialize(query);

            foreach (var groupBy in query.GroupBy)
                _groupByList.Add(groupBy);
            HavingExpression = query.Having;
        }

        /// <summary>
        /// Constructs SQL FROM clause.
        /// </summary>
        /// <typeparam name="T">Entity type of the DbSet.</typeparam>
        /// <param name="dbSet">The first DbSet in FROM clause.</param>
        /// <param name="_">The entity object for further SQL construction.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder From<T>(DbSet<T> dbSet, out T _)
            where T : class, IEntity, new()
        {
            base.From(dbSet, out _);
            return this;
        }

        /// <summary>
        /// Constructs SQL INNER JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T _)
            where T : class, IEntity<TKey>, new()
            where TKey : CandidateKey
        {
            base.InnerJoin(dbSet, left, out _);
            return this;
        }

        /// <summary>
        /// Constructs SQL INNER JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="right">The delegate to get right side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T _)
            where T : class, IEntity, new()
            where TKey : CandidateKey
        {
            base.InnerJoin(dbSet, left, right, out _);
            return this;
        }

        /// <summary>
        /// Constructs SQL LEFT JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T _)
            where T : class, IEntity<TKey>, new()
            where TKey : CandidateKey
        {
            base.LeftJoin(dbSet, left, out _);
            return this;
        }

        /// <summary>
        /// Constructs SQL LEFT JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="right">The delegate to get right side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T _)
            where T : class, IEntity, new()
            where TKey : CandidateKey
        {
            base.LeftJoin(dbSet, left, right, out _);
            return this;
        }

        /// <summary>
        /// Constructs SQL RIGHT JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T _)
            where T : class, IEntity<TKey>, new()
            where TKey : CandidateKey
        {
            base.RightJoin(dbSet, left, out _);
            return this;
        }


        /// <summary>
        /// Constructs SQL RIGHT JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="right">The delegate to get right side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T _)
            where T : class, IEntity, new()
            where TKey : CandidateKey
        {
            base.RightJoin(dbSet, left, right, out _);
            return this;
        }

        /// <summary>
        /// Constructs SQL RIGHT JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder CrossJoin<T>(DbSet<T> dbSet, out T _)
            where T : class, IEntity, new()
        {
            base.CrossJoin(dbSet, out _);
            return this;
        }

        /// <summary>
        /// Constructs SQL SELECT by automatically matching columns.
        /// </summary>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder AutoSelect()
        {
            base.AutoSelect();
            return this;
        }

        /// <summary>
        /// Constructs SQL SELECT by automatically matching columns between specified source model and target projection.
        /// </summary>
        /// <param name="from">The source model.</param>
        /// <param name="to">The target projection.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder AutoSelect(Model from, Projection to)
        {
            base.AutoSelect(from, to);
            return this;
        }

        /// <summary>
        /// Constructs SQL SELECT by matching between specified source column and target column.
        /// </summary>
        /// <typeparam name="T">Type of the columns.</typeparam>
        /// <param name="from">The source column.</param>
        /// <param name="to">The target column.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "For fluent API design")]
        public new DbAggregateQueryBuilder Select<T>(T from, T to)
            where T : Column, new()
        {
            base.Select(from, to);
            return this;
        }

        /// <summary>
        /// Constructs SQL SELECT by matching between specified source column and target new adhoc column.
        /// </summary>
        /// <typeparam name="T">Type of lthe column.</typeparam>
        /// <param name="from">The source column.</param>
        /// <param name="to">The target new adhoc model.</param>
        /// <param name="name">Name to create the adhoc column.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "For fluent API design")]
        public new DbAggregateQueryBuilder Select<T>(T from, Adhoc to, string name = null)
            where T : Column, new()
        {
            base.Select(from, to, name);
            return this;
        }

        /// <summary>
        /// Constructs SQL WHERE.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder Where(_Boolean condition)
        {
            base.Where(condition);
            return this;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether should group by selected columns automatically.
        /// </summary>
        public bool AutoGroupBy { get; set; }

        /// <summary>
        /// Sets a value that indicates whether should group by selected columns automatically.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public DbAggregateQueryBuilder WithAutoGroupBy(bool value)
        {
            AutoGroupBy = value;
            return this;
        }

        private GroupByCollection _groupByList = new GroupByCollection();
        /// <summary>
        /// Gets the SQL GROUP BY list.
        /// </summary>
        public ReadOnlyCollection<DbExpression> GroupByList
        {
            get { return _groupByList; }
        }

        /// <summary>
        /// Constructs SQL GROUP BY.
        /// </summary>
        /// <param name="column">The column to group by.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public DbAggregateQueryBuilder GroupBy(Column column)
        {
            VerifySourceColumn(column, nameof(column), false);
            _groupByList.Add(EliminateSubQuery(column.DbExpression));
            return this;
        }

        /// <summary>
        /// Constructs SQL HAVING.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public DbAggregateQueryBuilder Having(_Boolean condition)
        {
            VerifySourceColumn(condition, nameof(condition));
            HavingExpression = EliminateSubQuery(condition.DbExpression);
            return this;
        }

        /// <summary>
        /// Gets the SQL HAVING expression.
        /// </summary>
        public DbExpression HavingExpression { get; private set; }

        /// <summary>
        /// Constructs SQL ORDER BY.
        /// </summary>
        /// <param name="orderByList">The order by list.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder OrderBy(params ColumnSort[] orderByList)
        {
            base.OrderBy(orderByList);
            return this;
        }

        /// <summary>
        /// Constructs SQL ORDER BY with specified offset and fetch.
        /// </summary>
        /// <param name="offset">Specifies how many rows to skip within the query result.</param>
        /// <param name="fetch">Specifies how many rows to skip within the query result.</param>
        /// <param name="orderByList">The order by list.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder OrderBy(int offset, int fetch, params ColumnSort[] orderByList)
        {
            base.OrderBy(offset, fetch, orderByList);
            return this;
        }

        internal override void OnSelect(Column source, DbExpression sourceExpression, Column target)
        {
            if (source == null)
                return;

            if (target.IsSystem || (AutoGroupBy && source.AggregateSourceModels.Count == 0))
                _groupByList.Add(sourceExpression);
        }

        internal override DbSelectStatement BuildSelectStatement(IReadOnlyList<ColumnMapping> selectList, DbFromClause from, DbExpression where, IReadOnlyList<DbExpressionSort> orderBy)
        {
            return new DbSelectStatement(Model, selectList, from, where, GroupByList, HavingExpression, orderBy, Offset, Fetch);
        }

        /// <summary>
        /// Constructs SQL SELECT by matching between specified source column and target column, without type safety.
        /// </summary>
        /// <param name="from">The source column.</param>
        /// <param name="to">The target column.</param>
        /// <returns>This aggregate query builder for fluent coding.</returns>
        public new DbAggregateQueryBuilder UnsafeSelect(Column from, Column to)
        {
            base.UnsafeSelect(from, to);
            return this;
        }
    }
}
