using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
            _sourceModels = _sourceModels.Add(FromClause);
            WhereExpression = query.Where;
            OrderByList = query.OrderBy;
        }

        private void Select(IReadOnlyList<ColumnMapping> select)
        {
            foreach (var columnMapping in select)
            {
                var source = EliminateSubQuery(columnMapping.SourceExpression);
                SelectCore(source, columnMapping.Target);
            }
        }

        internal Model Model { get; private set; }

        #region FROM

        IModels _sourceModels = Models.Empty;
        SubQueryEliminator _subQueryEliminator;
        IAutoColumnSelector _autoColumnSelector;

        public DbFromClause FromClause { get; private set; }

        public DbQueryBuilder From<T>(DbSet<T> dbSet, out T model)
            where T : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));

            model = dbSet._;
            if (_sourceModels.Count > 0)
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
            Debug.Assert(!_sourceModels.Contains(model));

            _sourceModels = _sourceModels.Add(model);
            if (_autoColumnSelector == null)
                _autoColumnSelector = model.Columns;
            else
                _autoColumnSelector = _autoColumnSelector.Merge(model.Columns);
        }

        public DbQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T model)
            where T : Model<TKey>, new()
            where TKey : PrimaryKey
        {
            return InnerJoin(dbSet, left, GetPrimaryKey, out model);
        }

        private static T GetPrimaryKey<T>(Model<T> _)
            where T : PrimaryKey
        {
            return _.PrimaryKey;
        }

        public DbQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : PrimaryKey
        {
            Join(dbSet, left, right(dbSet._), DbJoinKind.InnerJoin, out model);
            return this;
        }

        public DbQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T model)
            where T : Model<TKey>, new()
            where TKey : PrimaryKey
        {
            return LeftJoin(dbSet, left, GetPrimaryKey, out model);
        }


        public DbQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : PrimaryKey
        {
            Join(dbSet, left, right(dbSet._), DbJoinKind.LeftJoin, out model);
            return this;
        }

        public DbQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T model)
            where T : Model<TKey>, new()
            where TKey : PrimaryKey
        {
            return RightJoin(dbSet, left, GetPrimaryKey, out model);
        }


        public DbQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T model)
            where T : Model, new()
            where TKey : PrimaryKey
        {
            Join(dbSet, left, right(dbSet._), DbJoinKind.RightJoin, out model);
            return this;
        }

        private void Join<T, TKey>(DbSet<T> dbSet, TKey left, TKey right, DbJoinKind kind, out T model)
            where T : Model, new()
            where TKey : PrimaryKey
        {
            Check.NotNull(dbSet, nameof(dbSet));
            Check.NotNull(left, nameof(left));
            if (!_sourceModels.Contains(left.ParentModel))
                throw new ArgumentException(Strings.DbQueryBuilder_Join_InvalidLeftKey, nameof(left));
            Check.NotNull(right, nameof(right));
            if (right.ParentModel != dbSet.Model)
                throw new ArgumentException(Strings.DbQueryBuilder_Join_InvalidRightKey, nameof(right));

            Join(dbSet, kind, left.Join(right), out model);
        }

        private void Join<T>(DbSet<T> dbSet, DbJoinKind kind, IReadOnlyList<ColumnMapping> relationship, out T model)
            where T : Model, new()
        {
            model = (T)Join(dbSet.Model, kind, relationship);
        }

        private Model Join(Model model, DbJoinKind kind, IReadOnlyList<ColumnMapping> relationship)
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

        private IReadOnlyList<ColumnMapping> EliminateSubQuery(IReadOnlyList<ColumnMapping> relationship)
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

            if (!_sourceModels.Contains(model))
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
            if (_autoColumnSelector == null)
                throw new InvalidOperationException(Strings.DbQueryBuilder_EmptyFrom);
            foreach (var targetColumn in Model.Columns)
            {
                if (_targetColumns.Contains(targetColumn))
                    continue;

                var sourceColumn = _autoColumnSelector.Select(targetColumn);
                if (sourceColumn != null)
                    SelectCore(sourceColumn, targetColumn);
            }
            return this;
        }

        public DbQueryBuilder AutoSelect(Model from, ModelExtension to)
        {
            if (to == null)
                throw new ArgumentNullException(nameof(to));
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (_sourceModels == null)
                throw new InvalidOperationException(Strings.DbQueryBuilder_EmptyFrom);
            if (!_sourceModels.Contains(from))
                throw new ArgumentException(Strings.DbQueryBuilder_InvalidAutoSelectSourceModel, nameof(from));

            var targetColumns = to.Columns;
            for (int i = 0; i < targetColumns.Count; i++)
            {
                var targetColumn = targetColumns[i];
                if (_targetColumns.Contains(targetColumn))
                    continue;
                VerifyTargetColumn(targetColumn, string.Format(CultureInfo.InvariantCulture, "{0}.Columns[{1}]", nameof(to), i));
                var sourceColumn = from.Columns.AutoSelect(targetColumn);
                if (sourceColumn != null)
                    SelectCore(sourceColumn, targetColumn);
            }

            return this;
        }

        public DbQueryBuilder Select<T>(T from, T to)
            where T : Column, new()
        {
            VerifySourceColumn(from, nameof(from));
            VerifyTargetColumn(to, nameof(to));
            SelectCore(from, to);
            return this;
        }

        public DbQueryBuilder Select<T>(T sourceColumn, Adhoc adhoc, string name = null)
            where T : Column, new()
        {
            return Select(sourceColumn, adhoc.AddColumn(sourceColumn, false, c => c.DbColumnName = string.IsNullOrEmpty(name) ? sourceColumn.DbColumnName : name));
        }

        private void VerifyTargetColumn(Column target, string paramName)
        {
            Check.NotNull(target, paramName);
            if (target.ParentModel != Model || _targetColumns.Contains(target))
                throw new ArgumentException(Strings.DbQueryBuilder_VerifyTargetColumn, paramName);
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
            VerifySourceColumn(condition, nameof(condition));
            WhereExpression = And(WhereExpression, EliminateSubQuery(condition.DbExpression));

            return this;
        }

        internal void VerifySourceColumn(Column column, string exceptionParamName)
        {
            VerifySourceColumn(column, exceptionParamName, this.GetType() == typeof(DbAggregateQueryBuilder));
        }

        internal void VerifySourceColumn(Column column, string paramName, bool allowsAggregate)
        {
            VerifySourceColumn(column, paramName, _sourceModels, allowsAggregate);
        }

        private void VerifySourceColumn(Column sourceColumn, string exceptionParamName, IModels sourceModelSet, bool allowsAggregate)
        {
            VerifyScalarSourceModels(sourceModelSet, sourceColumn, exceptionParamName);
            if (allowsAggregate)
                VerifyAggregateSourceModels(sourceColumn, exceptionParamName, sourceModelSet);
            else if (sourceColumn.AggregateSourceModels.Count > 0)
                throw new ArgumentException(Strings.DbQueryBuilder_AggregateNotAllowed, exceptionParamName);
        }

        private void VerifyScalarSourceModels(IModels containsBy, Column sourceColumn, string exceptionParamName)
        {
            if (sourceColumn.ScalarSourceModels.Count == 0 && sourceColumn.GetExpression() == null)
                throw new ArgumentException(Strings.Column_EmptyScalarSourceModels, exceptionParamName);

            foreach (var model in sourceColumn.ScalarSourceModels)
            {
                if (!containsBy.Contains(model))
                    throw new ArgumentException(Strings.DbQueryBuilder_InvalidScalarSourceModel(model), exceptionParamName);
            }
        }

        private static void VerifyAggregateSourceModels(Column sourceColumn, string exceptionParamName, IModels modelSet)
        {
            foreach (var model in sourceColumn.AggregateSourceModels)
            {
                if (!modelSet.Contains(model))
                    throw new ArgumentException(Strings.DbQueryBuilder_InvalidAggregateSourceModel(model), exceptionParamName);
            }
        }

        #endregion

        #region ORDER BY

        public IReadOnlyList<DbExpressionSort> OrderByList { get; private set; }

        public int Offset { get; private set; }

        public int Fetch { get; private set; }

        public DbQueryBuilder OrderBy(params ColumnSort[] orderByList)
        {
            return OrderBy(-1, -1, orderByList);
        }

        public DbQueryBuilder OrderBy(int offset, int fetch, params ColumnSort[] orderByList)
        {
            Check.NotNull(orderByList, nameof(orderByList));
            VerifyOrderByList(orderByList);
            VerifyOffsetFetch(offset, fetch);
            OrderByList = new ReadOnlyCollection<DbExpressionSort>(EliminateSubQuery(orderByList));
            Offset = offset;
            Fetch = fetch;

            return this;
        }

        private IList<DbExpressionSort> EliminateSubQuery(IList<ColumnSort> orderByList)
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

        private void VerifyOrderByList(ColumnSort[] orderByList)
        {
            Debug.Assert(orderByList != null);
            for (int i = 0; i < orderByList.Length; i++)
            {
                var orderBy = orderByList[i];
                VerifySourceColumn(orderBy.Column, string.Format("orderByList[{0}]", i));
            }
        }

        #endregion

        internal virtual DbSelectStatement BuildSelectStatement(IReadOnlyList<ColumnMapping> select, DbFromClause from, DbExpression where, IReadOnlyList<DbExpressionSort> orderBy)
        {
            return new DbSelectStatement(Model, select, from, where, orderBy, Offset, Fetch);
        }

        private DbUnionStatement GetEliminatableUnionStatement(IReadOnlyList<ColumnMapping> selectList)
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

        private DbUnionStatement EliminateUnionSubQuery(IReadOnlyList<ColumnMapping> selectList)
        {
            var fromQuery = GetEliminatableUnionStatement(selectList);
            return fromQuery == null ? null : new DbUnionStatement(Model, fromQuery.Query1, fromQuery.Query2, fromQuery.Kind);
        }

        private bool CanEliminateUnionSubQuery(IReadOnlyList<ColumnMapping> selectList)
        {
            return GetEliminatableUnionStatement(selectList) != null;
        }
    }
}
