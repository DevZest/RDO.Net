using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data
{
    public class DbQueryBuilder
    {
        internal DbQueryBuilder(Model model)
        {
            Debug.Assert(model != null && model.DataSource == null);

            Model = model;
            Offset = -1;
            Fetch = -1;
        }

        internal DbQueryBuilder(Model model, DbSelectStatement query)
            : this(model)
        {
            var sourceModel = query.Model;
            Debug.Assert(model.GetType() == sourceModel.GetType());

            Column sysParentRowId = sourceModel.GetSysParentRowIdColumn(createIfNotExist: false);
            if (sysParentRowId != null && sysParentRowId.Ordinal < query.Select.Count)
                model.GetSysParentRowIdColumn(createIfNotExist: true);

            Column sysRowId = sourceModel.GetSysRowIdColumn(createIfNotExist: false);
            if (sysRowId != null && sysRowId.Ordinal < query.Select.Count)
                model.GetSysRowIdColumn(createIfNotExist: true);

            var columns = model.Columns;
            var sourceColumns = sourceModel.Columns;

            Debug.Assert(columns.Count <= sourceColumns.Count);
            Debug.Assert(columns.Count <= query.Select.Count);
            for (int i = 0; i < columns.Count; i++)
                SelectList.Add(new ColumnMapping(query.Select[i].Source, columns[i]));
            FromClause = query.From;
            WhereExpression = query.Where;
            OrderByList = query.OrderBy;
        }

        internal DbQueryBuilder(Model model, Model sourceModel)
            : this(model)
        {
            From(sourceModel);
            var sourceColumns = sourceModel.Columns;
            var targetColumns = model.Columns;
            Debug.Assert(targetColumns.Count <= sourceColumns.Count);
            for (int i = 0; i < targetColumns.Count; i++)
            {
                var targetColumn = targetColumns[i];
                SelectCore(sourceColumns[targetColumn.Key], targetColumn);
            }
        }

        internal Model Model { get; private set; }

        #region FROM

        ModelSet _sourceModelSet = new ModelSet();
        Dictionary<Model, SubQueryEliminator> _subQueryEliminators = new Dictionary<Model, SubQueryEliminator>();
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

        internal void From(Model model)
        {
            InitSubQueryEliminator(model);
            AddSourceModel(model);
            FromClause = EliminateSubQuery(model);
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
            model = dbSet._;
            model = MakeAlias(model);
            Join(model, kind, relationship);
        }

        private void Join(Model model, DbJoinKind kind, IList<ColumnMapping> relationship)
        {
            AddSourceModel(model);
            FromClause = new DbJoinClause(kind, FromClause, EliminateSubQuery(model), EliminateSubQuery(relationship));
        }

        private DbFromClause EliminateSubQuery(Model model)
        {
            SubQueryEliminator subQueryEliminator;
            if (_subQueryEliminators.TryGetValue(model, out subQueryEliminator))
                return subQueryEliminator.FromClause;
            else
                return model.FromClause;
        }

        internal DbExpression EliminateSubQuery(DbExpression expression)
        {
            foreach (var subQueryEliminator in _subQueryEliminators.Values)
                expression = subQueryEliminator.GetExpressioin(expression);
            return expression;
        }

        private Column EliminateSubQuery(Column column)
        {
            if (column == null)
                return null;

            var expression = EliminateSubQuery(column.DbExpression);
            if (expression == column.DbExpression)
                return column;

            var dbColumnExpression = expression as DbColumnExpression;
            return dbColumnExpression == null ? null : dbColumnExpression.Column;
        }

        private IList<ColumnMapping> EliminateSubQuery(IList<ColumnMapping> relationship)
        {
            if (relationship == null)
                return relationship;

            ColumnMapping[] result = null;
            for (int i = 0; i < relationship.Count; i++)
            {
                var mapping = relationship[i];
                var source = mapping.Source;
                var target = mapping.Target.Column;
                var replacedSource = EliminateSubQuery(source);
                var replacedTarget = EliminateSubQuery(target);
                if ((source != replacedSource || target != replacedTarget) && result == null)
                {
                    result = new ColumnMapping[relationship.Count];
                    for (int j = 0; j < i; j++)
                        result[j] = relationship[j];
                }
                if (result != null)
                    result[i] = new ColumnMapping(replacedSource, replacedTarget);
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

        private T MakeAlias<T>(T model)
            where T : Model, new()
        {
            Debug.Assert(model != null);

            if (!_sourceModelSet.Contains(model))
            {
                InitSubQueryEliminator(model);
                return model;
            }

            return Data.Model.Clone<T>(model, true);
        }

        private void InitSubQueryEliminator(Model model)
        {
            var subQueryEliminator = model.FromClause.SubQueryEliminator;
            if (subQueryEliminator != null)
            {
                _subQueryEliminators.Add(model, subQueryEliminator);
                WhereExpression = And(WhereExpression, subQueryEliminator.WhereExpression);
            }
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
                SelectCore(null, DbConstantExpression.Null, target);
                return;
            }

            var replacedSource = EliminateSubQuery(source);
            if (replacedSource == null)
                SelectCore(null, EliminateSubQuery(source.DbExpression), target);
            else
                SelectCore(replacedSource.AggregateModelSet, replacedSource.DbExpression, target);
        }

        internal virtual void SelectCore(IModelSet sourceAggregateModelSet, DbExpression source, Column target)
        {
            SelectList.Add(new ColumnMapping(source, target));
        }

        #endregion

        #region WHERE

        public DbExpression WhereExpression { get; private set; }

        public DbQueryBuilder Where(_Boolean condition)
        {
            Check.NotNull(condition, nameof(condition));
            VerifyModelSet(condition, nameof(condition));
            WhereExpression = EliminateSubQuery(condition.DbExpression);

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
                VerifyModelSet(orderBy.Column, string.Format("orderByList[{0}]", i));
            }
        }

        #endregion

        internal virtual DbSelectStatement BuildSelectStatement(IList<ColumnMapping> select, DbFromClause from, DbExpression where, IList<DbExpressionSort> orderBy)
        {
            return new DbSelectStatement(Model, select, from, where, orderBy, Offset, Fetch);
        }

        internal DbQueryStatement BuildQueryStatement(DbTable<SequentialKeyModel> sequentialKeys)
        {
            var parentRowIdIdentity = SelectSysParentRowId();
            var rowIdIdentity = SelectSysRowId(sequentialKeys);

            var select = NormalizeSelectList();
            if (parentRowIdIdentity == null && rowIdIdentity == null)
            {
                var result = TryBuildSimpleUnionQuery(select);
                if (result != null)
                    return result;
            }
            var orderBy = GetOrderBy(parentRowIdIdentity, rowIdIdentity);
            return BuildSelectStatement(select, FromClause, WhereExpression, orderBy);
        }

        private DbQueryStatement TryBuildSimpleUnionQuery(IList<ColumnMapping> normalizedSelectList)
        {
            var fromQuery = FromClause as DbUnionStatement;
            if (fromQuery == null)
                return null;

            if (WhereExpression != null || OrderByList != null)
                return null;

            var fromColumns = fromQuery.Model.Columns;
            if (normalizedSelectList.Count != fromColumns.Count)
                return null;

            for (int i = 0; i < normalizedSelectList.Count; i++)
            {
                if (normalizedSelectList[i].Source != fromColumns[i].DbExpression)
                    return null;
            }

            return new DbUnionStatement(Model, fromQuery.Query1, fromQuery.Query2, fromQuery.Kind);
        }

        private IList<DbExpressionSort> GetOrderBy(Identity parentRowIdIdentity, Identity rowIdIdentity)
        {
            if (rowIdIdentity != null)
            {
                return new DbExpressionSort[]
                {
                    GetDbExpressionSort(rowIdIdentity)
                };
            }

            if (parentRowIdIdentity != null)
                return GetOrderBy(parentRowIdIdentity);

            return OrderByList;
        }

        private static DbExpressionSort GetDbExpressionSort(Identity identity)
        {
            return new DbExpressionSort(identity.Column.DbExpression, identity.Increment > 0 ? SortDirection.Ascending : SortDirection.Descending);
        }

        private IList<DbExpressionSort> GetOrderBy(Identity parentIdentity)
        {
            Debug.Assert(parentIdentity != null);
            var orderByListCount = OrderByList == null ? 0 : OrderByList.Count;
            var result = new DbExpressionSort[orderByListCount + 1];
            result[0] = GetDbExpressionSort(parentIdentity);
            for (int i = 0; i < orderByListCount; i++)
                result[i + 1] = OrderByList[i];

            return result;
        }

        private Identity SelectSysParentRowId()
        {
            var parentSequentialKeyModel = Model.ParentSequentialKeyModel;
            if (parentSequentialKeyModel == null)
                return null;

            var sysParentRowId = Model.GetSysParentRowIdColumn(createIfNotExist: true);
            var relationship = ResolveRelationship(Model.ParentRelationship, parentSequentialKeyModel);
            Join(parentSequentialKeyModel, DbJoinKind.InnerJoin, relationship);
            var result = parentSequentialKeyModel.GetIdentity(true);
            SelectCore(result.Column, sysParentRowId);
            return result;
        }

        private Identity SelectSysRowId(DbTable<SequentialKeyModel> sequentialKeys)
        {
            if (sequentialKeys == null)
                return null;
            var sequentialKeyModel = sequentialKeys.Model;

            var sysRowId = Model.GetSysRowIdColumn(createIfNotExist: true);
            var relationship = ResolveRelationship(Model.PrimaryKey.GetRelationship(sequentialKeyModel.PrimaryKey), sequentialKeyModel);
            Join(sequentialKeyModel, DbJoinKind.InnerJoin, relationship);
            var result = sequentialKeyModel.GetIdentity(true);
            Select(result.Column, sysRowId);
            return result;
        }

        private IList<ColumnMapping> ResolveRelationship(IList<ColumnMapping> relationship, Model targetModel)
        {
            var result = new ColumnMapping[relationship.Count];
            for (int i = 0; i < relationship.Count; i++)
            {
                var mapping = relationship[i];
                var source = GetSource(mapping.SourceColumn.Ordinal);
                var targetColumn = GetCorrespondingPrimaryKeyColumn(mapping.TargetColumn, targetModel);
                result[i] = new ColumnMapping(source, targetColumn);
            }
            return new ReadOnlyCollection<ColumnMapping>(result);
        }

        private DbExpression GetSource(int ordinal)
        {
            foreach (var select in SelectList)
            {
                if (select.TargetColumn.Ordinal == ordinal)
                    return select.Source;
            }

            return DbConstantExpression.Null;
        }

        private static Column GetCorrespondingPrimaryKeyColumn(Column column, Model targetModel)
        {
            Debug.Assert(column != null);
            Debug.Assert(targetModel != null);

            if (column.ParentModel == targetModel)
                return column;

            var primaryKey = column.ParentModel.PrimaryKey;
            for (int i = 0; i < primaryKey.Count; i++)
            {
                if (column == primaryKey[i].Column)
                    return targetModel.PrimaryKey[i].Column;
            }

            Debug.Fail("Cannot match primary key column.");
            return null;
        }

        private IList<ColumnMapping> NormalizeSelectList()
        {
            var result = new ColumnMapping[Model.Columns.Count];

            foreach (var selectItem in SelectList)
                result[selectItem.TargetColumn.Ordinal] = selectItem;

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i].TargetColumn == null)
                    result[i] = new ColumnMapping(DbConstantExpression.Null, Model.Columns[i]);
            }

            return result;
        }

        internal void Where(DataRow parentRow, ReadOnlyCollection<ColumnMapping> parentRelationship)
        {
            var primaryKey = parentRow.Model.PrimaryKey;
            Debug.Assert(primaryKey != null);
            foreach (var columnSort in primaryKey)
            {
                var column = columnSort.Column;
                var param = column.CreateParam(parentRow).DbExpression;
                var sourceColumnOrdinal = parentRelationship.Where(x => x.TargetColumn.Ordinal == column.Ordinal).Single().SourceColumn.Ordinal;
                var sourceExpression = SelectList[sourceColumnOrdinal].Source;
                var equalCondition = new DbBinaryExpression(BinaryExpressionKind.Equal, sourceExpression, param);
                WhereExpression = WhereExpression == null ? equalCondition : new DbBinaryExpression(BinaryExpressionKind.And, equalCondition, WhereExpression);
            }
        }
    }
}
