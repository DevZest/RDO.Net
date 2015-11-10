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

        internal DbAggregateQueryBuilder(Model model, DbSelectStatement sourceData)
            : base(model, sourceData)
        {
            Debug.Assert(sourceData.IsAggregate);
            foreach (var groupBy in sourceData.GroupBy)
                _groupByList.Add(groupBy);
            HavingExpression = sourceData.Having;
        }

        public new DbAggregateQueryBuilder From<T>(DbSet<T> dbSet, out T model)
            where T : Model, new()
        {
            base.From(dbSet, out model);
            return this;
        }

        public new DbAggregateQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : ModelKey
        {
            base.InnerJoin(dbSet, left, right, out model);
            return this;
        }

        public new DbAggregateQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : ModelKey
        {
            base.LeftJoin(dbSet, left, right, out model);
            return this;
        }

        public new DbAggregateQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : ModelKey
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
            VerifyModelSet(column, nameof(column), false);
            _groupByList.Add(EliminateSubQuery(column.DbExpression));
            return this;
        }

        public DbAggregateQueryBuilder Having(_Boolean condition)
        {
            VerifyModelSet(condition, nameof(condition));
            HavingExpression = EliminateSubQuery(condition.DbExpression);
            return this;
        }

        public DbExpression HavingExpression { get; private set; }

        public new DbAggregateQueryBuilder OrderBy(params ColumnSort[] orderByList)
        {
            base.OrderBy(orderByList);
            return this;
        }

        internal override void SelectCore(IModelSet sourceAggregateModelSet, DbExpression source, Column target)
        {
            base.SelectCore(sourceAggregateModelSet, source, target);

            if (AutoGroupBy && sourceAggregateModelSet.Count == 0)
                _groupByList.Add(source);
        }

        internal override DbSelectStatement BuildSelectStatement(IList<ColumnMapping> selectList, DbFromClause from, DbExpression where, IList<DbExpressionSort> orderBy)
        {
            return new DbSelectStatement(Model, selectList, from, where, GroupByList, HavingExpression, orderBy, Offset, Fetch);
        }
    }
}
