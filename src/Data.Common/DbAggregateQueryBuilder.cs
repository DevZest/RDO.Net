using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
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

        public new DbAggregateQueryBuilder From<T>(DbSet<T> dbSet, out T model)
            where T : Model, new()
        {
            base.From(dbSet, out model);
            return this;
        }

        public new DbAggregateQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T model)
            where T : Model<TKey>, new()
            where TKey : PrimaryKey
        {
            base.InnerJoin(dbSet, left, out model);
            return this;
        }

        public new DbAggregateQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : PrimaryKey
        {
            base.InnerJoin(dbSet, left, right, out model);
            return this;
        }

        public new DbAggregateQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T model)
            where T : Model<TKey>, new()
            where TKey : PrimaryKey
        {
            base.LeftJoin(dbSet, left, out model);
            return this;
        }


        public new DbAggregateQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : PrimaryKey
        {
            base.LeftJoin(dbSet, left, right, out model);
            return this;
        }

        public new DbAggregateQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T model)
            where T : Model<TKey>, new()
            where TKey : PrimaryKey
        {
            base.RightJoin(dbSet, left, out model);
            return this;
        }


        public new DbAggregateQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : PrimaryKey
        {
            base.RightJoin(dbSet, left, right, out model);
            return this;
        }

        public new DbAggregateQueryBuilder CrossJoin<T>(DbSet<T> dbSet, out T model)
            where T : Model, new()
        {
            base.CrossJoin(dbSet, out model);
            return this;
        }

        public new DbAggregateQueryBuilder AutoSelect()
        {
            base.AutoSelect();
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "For fluent API design")]
        public new DbAggregateQueryBuilder Select<T>(T source, T target)
            where T : Column, new()
        {
            base.Select(source, target);
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods", Justification = "For fluent API design")]
        public new DbAggregateQueryBuilder Select<T>(T sourceColumn, Adhoc adhoc, string name = null)
            where T : Column, new()
        {
            base.Select(sourceColumn, adhoc, name);
            return this;
        }

        public new DbAggregateQueryBuilder Where(_Boolean condition)
        {
            base.Where(condition);
            return this;
        }

        public bool AutoGroupBy { get; set; }

        private GroupByCollection _groupByList = new GroupByCollection();
        public ReadOnlyCollection<DbExpression> GroupByList
        {
            get { return _groupByList; }
        }

        public DbAggregateQueryBuilder GroupBy(Column column)
        {
            VerifySourceColumn(column, nameof(column), false);
            _groupByList.Add(EliminateSubQuery(column.DbExpression));
            return this;
        }

        public DbAggregateQueryBuilder Having(_Boolean condition)
        {
            VerifySourceColumn(condition, nameof(condition));
            HavingExpression = EliminateSubQuery(condition.DbExpression);
            return this;
        }

        public DbExpression HavingExpression { get; private set; }

        public new DbAggregateQueryBuilder OrderBy(params ColumnSort[] orderByList)
        {
            base.OrderBy(orderByList);
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
    }
}
