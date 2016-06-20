using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    public partial class DbQueryBuilder
    {
        internal DbQueryBuilder(Model model)
        {
            Debug.Assert(model != null && (
                model.DataSource == null || 
                model.DataSource.Kind == DataSourceKind.DbTable ||
                model.DataSource.Kind == DataSourceKind.DbTempTable));

            Model = model;
            Offset = -1;
            Fetch = -1;
        }

        internal virtual void Initialize(DbSelectStatement query)
        {
            _subQueryEliminator = query.SubQueryEliminator;
            FromClause = query.From;
            _sourceModelSet.Add(FromClause);
            WhereExpression = query.Where;
            OrderByList = query.OrderBy;
        }

        private void Select(IList<ColumnMapping> select)
        {
            foreach (var columnMapping in select)
            {
                var source = EliminateSubQuery(columnMapping.SourceExpression);
                SelectCore(source, columnMapping.Target);
            }
        }

        internal Model Model { get; private set; }

        #region FROM

        ModelSet _sourceModelSet = new ModelSet();
        SubQueryEliminator _subQueryEliminator;
        Dictionary<ColumnKey, List<Column>> _sourceColumnsByKey = new Dictionary<ColumnKey, List<Column>>();

        public DbFromClause FromClause { get; private set; }

        public DbQueryBuilder From<T>(DbSet<T> dbSet, out T model)
            where T : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));

            model = dbSet._;
            if (_sourceModelSet.Count > 0)
                throw new InvalidOperationException(Strings.DbQueryBuilder_DuplicateFrom);

            From(model);
            return this;
        }

        private void From(Model model)
        {
            _subQueryEliminator = model.FromClause.SubQueryEliminator;
            if (_subQueryEliminator != null)
                WhereExpression = And(WhereExpression, _subQueryEliminator.WhereExpression);
            AddSourceModel(model);
            FromClause =_subQueryEliminator == null ? model.FromClause : _subQueryEliminator.FromClause;
            Debug.Assert(FromClause != null);
        }

        private void AddSourceModel(Model model)
        {
            Debug.Assert(!_sourceModelSet.Contains(model));

            _sourceModelSet.Add(model);
            foreach (var column in model.Columns)
            {
                var columnKey = column.Key;
                List<Column> sourceColumns;
                if (!_sourceColumnsByKey.TryGetValue(columnKey, out sourceColumns))
                {
                    sourceColumns = new List<Column>();
                    _sourceColumnsByKey.Add(columnKey, sourceColumns);
                }
                sourceColumns.Add(column);
            }
        }

        public DbQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : ModelKey
        {
            Join(dbSet, left, right(dbSet._), DbJoinKind.InnerJoin, out model);
            return this;
        }

        public DbQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : ModelKey
        {
            Join(dbSet, left, right(dbSet._), DbJoinKind.LeftJoin, out model);
            return this;
        }

        public DbQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : ModelKey
        {
            Join(dbSet, left, right(dbSet._), DbJoinKind.RightJoin, out model);
            return this;
        }

        private void Join<T, TKey>(DbSet<T> dbSet, TKey left, TKey right, DbJoinKind kind, out T model)
            where T : Model, new()
            where TKey : ModelKey
        {
            Check.NotNull(dbSet, nameof(dbSet));
            Check.NotNull(left, nameof(left));
            if (!_sourceModelSet.Contains(left.ParentModel))
                throw new ArgumentException(Strings.DbQueryBuilder_Join_InvalidLeftKey, nameof(left));
            Check.NotNull(right, nameof(right));
            if (right.ParentModel != dbSet.Model)
                throw new ArgumentException(Strings.DbQueryBuilder_Join_InvalidRightKey, nameof(right));

            Join(dbSet, kind, left.GetRelationship(right), out model);
        }

        private void Join<T>(DbSet<T> dbSet, DbJoinKind kind, IList<ColumnMapping> relationship, out T model)
            where T : Model, new()
        {
            model = (T)Join(dbSet.Model, kind, relationship);
        }

        private Model Join(Model model, DbJoinKind kind, IList<ColumnMapping> relationship)
        {
            Debug.Assert(relationship[0].Target.ParentModel == model);

            var result = MakeAlias(model);
            var resultFromClause = result.FromClause;
            if (result != model)
            {
                resultFromClause = resultFromClause.Clone(result);
                relationship = relationship.Select(x => new ColumnMapping(x.SourceExpression, result.Columns[x.Target.Ordinal])).ToList();
            }

            AddSourceModel(result);
            FromClause = new DbJoinClause(kind, FromClause, resultFromClause, EliminateSubQuery(relationship));
            return result;
        }

        internal DbExpression EliminateSubQuery(DbExpression expression)
        {
            return _subQueryEliminator == null ? expression : _subQueryEliminator.GetExpressioin(expression);
        }

        private IList<ColumnMapping> EliminateSubQuery(IList<ColumnMapping> relationship)
        {
            if (relationship == null)
                return relationship;

            ColumnMapping[] result = null;
            for (int i = 0; i < relationship.Count; i++)
            {
                var mapping = relationship[i];
                var source = mapping.SourceExpression;
                var replacedSource = EliminateSubQuery(source);
                if (source != replacedSource  && result == null)
                {
                    result = new ColumnMapping[relationship.Count];
                    for (int j = 0; j < i; j++)
                        result[j] = relationship[j];
                }
                if (result != null)
                    result[i] = new ColumnMapping(replacedSource, mapping.TargetExpression.Column);
            }
            return result ?? relationship;
        }

        public DbQueryBuilder CrossJoin<T>(DbSet<T> dbSet, out T model)
            where T : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));
            Join(dbSet, DbJoinKind.CrossJoin, null, out model);
            return this;
        }

        private Model MakeAlias(Model model)
        {
            Debug.Assert(model != null);

            if (!_sourceModelSet.Contains(model))
                return model;

            return model.Clone(true);
        }

        private static DbExpression And(DbExpression where1, DbExpression where2)
        {
            if (where1 == null)
                return where2;
            if (where2 == null)
                return where1;

            return new DbBinaryExpression(BinaryExpressionKind.And, where1, where2);
        }

        #endregion

        #region SELECT

        HashSet<Column> _targetColumns = new HashSet<Column>();
        List<ColumnMapping> _selectList = new List<ColumnMapping>();
        public List<ColumnMapping> SelectList
        {
            get { return _selectList; }
        }

        public DbQueryBuilder AutoSelect()
        {
            foreach (var targetColumn in Model.Columns)
            {
                if (_targetColumns.Contains(targetColumn))
                    continue;

                List<Column> sourceColumns;
                if (_sourceColumnsByKey.TryGetValue(targetColumn.Key, out sourceColumns))
                {
                    if (sourceColumns.Count == 1)
                        SelectCore(sourceColumns[0], targetColumn);
                }
            }
            return this;
        }

        public DbQueryBuilder Select<T>(T source, T target)
            where T : Column, new()
        {
            VerifyModelSet(source, nameof(source));
            VerifyTargetColumn(target);
            SelectCore(source, target);
            return this;
        }

        public DbQueryBuilder Select<T>(T sourceColumn, Adhoc adhoc, string name = null)
            where T : Column, new()
        {
            return Select(sourceColumn, adhoc.AddColumn(sourceColumn, false, c => c.DbColumnName = string.IsNullOrEmpty(name) ? sourceColumn.DbColumnName : name));
        }

        private void VerifyTargetColumn(Column target)
        {
            Check.NotNull(target, nameof(target));
            if (target.ParentModel != Model || _targetColumns.Contains(target))
                throw new ArgumentException(Strings.DbQueryBuilder_VerifyTargetColumn, nameof(target));
        }

        internal void SelectCore(Column source, Column target)
        {
            _targetColumns.Add(target);

            if (source == null)
            {
                SelectCore(source, DbConstantExpression.Null, target);
                return;
            }

            var sourceExpression = EliminateSubQuery(source.DbExpression);
            SelectCore(source, sourceExpression, target);
        }

        private void SelectCore(Column source, DbExpression sourceExpression, Column target)
        {
            SelectCore(sourceExpression, target);
            OnSelect(source, sourceExpression, target);
        }

        private void SelectCore(DbExpression source, Column target)
        {
            SelectList.Add(new ColumnMapping(source, target));
        }

        internal virtual void OnSelect(Column source, DbExpression sourceExpression, Column target)
        {
        }

        #endregion

        #region WHERE

        public DbExpression WhereExpression { get; private set; }

        public DbQueryBuilder Where(_Boolean condition)
        {
            Check.NotNull(condition, nameof(condition));
            VerifyModelSet(condition, nameof(condition));
            WhereExpression = And(WhereExpression, EliminateSubQuery(condition.DbExpression));

            return this;
        }

        internal void VerifyModelSet(Column column, string exceptionParamName)
        {
            VerifyModelSet(column, exceptionParamName, this.GetType() == typeof(DbAggregateQueryBuilder));
        }

        internal void VerifyModelSet(Column column, string paramName, bool allowsAggregate)
        {
            column.VerifyModelSet(paramName, _sourceModelSet, allowsAggregate);
        }

        #endregion

        #region ORDER BY

        public ReadOnlyCollection<DbExpressionSort> OrderByList { get; private set; }

        public int Offset { get; private set; }

        public int Fetch { get; private set; }

        public DbQueryBuilder OrderBy(params OrderBy[] orderByList)
        {
            return OrderBy(-1, -1, orderByList);
        }

        public DbQueryBuilder OrderBy(int offset, int fetch, params OrderBy[] orderByList)
        {
            Check.NotNull(orderByList, nameof(orderByList));
            VerifyOrderByList(orderByList);
            VerifyOffsetFetch(offset, fetch);
            OrderByList = new ReadOnlyCollection<DbExpressionSort>(EliminateSubQuery(orderByList));
            Offset = offset;
            Fetch = fetch;

            return this;
        }

        private IList<DbExpressionSort> EliminateSubQuery(IList<OrderBy> orderByList)
        {
            if (orderByList == null)
                return null;

            var result = new DbExpressionSort[orderByList.Count];
            for (int i = 0; i < orderByList.Count; i++)
            {
                var orderBy = orderByList[i];
                result[i] = new DbExpressionSort(EliminateSubQuery(orderBy.Column.DbExpression), orderBy.Direction);
            }

            return result;
        }

        private static void VerifyOffsetFetch(int offset, int fetch)
        {
            if (offset == -1 && fetch == -1)
                return;

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (fetch < -1 || fetch == 0)
                throw new ArgumentOutOfRangeException(nameof(fetch));
        }

        private void VerifyOrderByList(OrderBy[] orderByList)
        {
            Debug.Assert(orderByList != null);
            for (int i = 0; i < orderByList.Length; i++)
            {
                var orderBy = orderByList[i];
                VerifyModelSet(orderBy.Column, string.Format("orderByList[{0}]", i));
            }
        }

        #endregion

        internal virtual DbSelectStatement BuildSelectStatement(IList<ColumnMapping> select, DbFromClause from, DbExpression where, IList<DbExpressionSort> orderBy)
        {
            return new DbSelectStatement(Model, select, from, where, orderBy, Offset, Fetch);
        }

        private DbUnionStatement GetEliminatableUnionStatement(IList<ColumnMapping> selectList)
        {
            var fromQuery = FromClause as DbUnionStatement;
            if (fromQuery == null)
                return null;

            if (WhereExpression != null || OrderByList != null)
                return null;

            var fromColumns = fromQuery.Model.Columns;
            if (selectList.Count != fromColumns.Count)
                return null;

            for (int i = 0; i < selectList.Count; i++)
            {
                if (selectList[i].SourceExpression != fromColumns[i].DbExpression)
                    return null;
            }
            return fromQuery;
        }

        private DbUnionStatement EliminateUnionSubQuery(IList<ColumnMapping> selectList)
        {
            var fromQuery = GetEliminatableUnionStatement(selectList);
            return fromQuery == null ? null : new DbUnionStatement(Model, fromQuery.Query1, fromQuery.Query2, fromQuery.Kind);
        }

        private bool CanEliminateUnionSubQuery(IList<ColumnMapping> selectList)
        {
            return GetEliminatableUnionStatement(selectList) != null;
        }
    }
}
