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
        internal static DbQueryBuilder SelectAll(Model model, Model sourceModel)
        {
            var result = new DbQueryBuilder(model);
            result.From(sourceModel);
            var sourceColumns = sourceModel.Columns;
            var columns = model.Columns;
            Debug.Assert(columns.Count <= sourceColumns.Count);
            for (int i = 0; i < columns.Count; i++)
            {
                var newColumn = columns[i];
                result.SelectCore(sourceColumns[newColumn.Key], newColumn);
            }

            return result;
        }

        internal static DbQueryBuilder SelectAll(Model model, DbUnionStatement query, Identity sequentialKeyIdentity)
        {
            var sourceModel = query.Model;
            Debug.Assert(model.GetType() == sourceModel.GetType());

            var result = SelectAll(model, sourceModel);
            result._sequentialKeyIdentity = sequentialKeyIdentity;
            return result;
        }

        internal DbQueryBuilder(Model model, DbSelectStatement query, Identity sequentialKeyIdentity)
            : this(model)
        {
            _sequentialKeyIdentity = sequentialKeyIdentity;

            var sourceModel = query.Model;
            Debug.Assert(model.GetType() == sourceModel.GetType());

            if (SequentialKeyColumn != null && !object.ReferenceEquals(sourceModel.GetSysParentRowIdColumn(createIfNotExist: false), null))
                model.GetSysParentRowIdColumn(createIfNotExist: true);

            var columns = model.Columns;
            var sourceColumns = sourceModel.Columns;

            Debug.Assert(columns.Count <= sourceColumns.Count);
            for (int i = 0; i < columns.Count; i++)
                SelectList.Add(new ColumnMapping(query.Select[i].Source, columns[i]));
            FromClause = query.From;
            WhereExpression = query.Where;
            OrderByList = query.OrderBy;
        }

        internal DbQueryBuilder(Model model)
        {
            Debug.Assert(model != null && model.DataSource == null);

            Model = model;
            Offset = -1;
            Fetch = -1;
        }

        internal Model Model { get; private set; }


        private Identity _sequentialKeyIdentity;

        private IDbTable JoinTable
        {
            get { return _sequentialKeyIdentity == null ? null : (IDbTable)_sequentialKeyIdentity.Column.ParentModel.DataSource; }
        }

        internal Column SequentialKeyColumn
        {
            get { return _sequentialKeyIdentity == null ? null : _sequentialKeyIdentity.Column; }
        }

        private static Column GetSequentialKeyColumn(IDbTable sequentialKeyTempTable)
        {
            if (sequentialKeyTempTable == null)
                return null;

            var identity = sequentialKeyTempTable.Model.GetIdentity(true);
            return identity == null ? null : identity.Column;
        }

        #region FROM

        ModelSet _sourceModelSet = new ModelSet();
        Dictionary<ColumnKey, List<Column>> _sourceColumnsByKey = new Dictionary<ColumnKey, List<Column>>();

        public DbFromClause FromClause { get; private set; }

        public DbQueryBuilder From<T>(DbSet<T> dbSet, out T model)
            where T : Model, new()
        {
            model = dbSet._;
            VerifyModel(model);

            if (_sourceModelSet.Count > 0)
                throw new InvalidOperationException(Strings.DbQueryBuilder_DuplicateFrom);

            From(model);
            return this;
        }

        internal void From(Model model)
        {
            AddSourceModel(model);
            FromClause = model.FromClause;
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

        private void VerifyModel(Model model)
        {
            Check.NotNull(model, nameof(model));
            if (model.DataSource == null || model.DataSource.Kind == DataSourceKind.DataSet)
                throw new ArgumentException(Strings.DbQueryBuilder_VerifyModel, nameof(model));
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
            model = dbSet._;
            VerifyModel(model);
            Check.NotNull(left, nameof(left));
            if (!_sourceModelSet.Contains(left.ParentModel))
                throw new ArgumentException(Strings.DbQueryBuilder_Join_InvalidLeftKey, nameof(left));
            Check.NotNull(right, nameof(right));
            if (right.ParentModel != model)
                throw new ArgumentException(Strings.DbQueryBuilder_Join_InvalidRightKey, nameof(right));

            model = MakeAlias(model);
            AddSourceModel(model);
            FromClause = new DbJoinClause(kind, FromClause, model.FromClause, left.GetRelationship(right));
        }

        public DbQueryBuilder CrossJoin<T>(DbSet<T> dbSet, out T model)
            where T : Model, new()
        {
            model = dbSet._;
            VerifyModel(model);

            model = MakeAlias(model);
            AddSourceModel(model);
            FromClause = new DbJoinClause(DbJoinKind.CrossJoin, FromClause, model.FromClause, null);
            return this;
        }

        private T MakeAlias<T>(T model)
            where T : Model, new()
        {
            Debug.Assert(model != null);

            if (!_sourceModelSet.Contains(model))
                return model;

            return Data.Model.Clone<T>(model, true);
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
            if (_targetColumns.Contains(target))
                throw new ArgumentException(Strings.DbQueryBuilder_VerifyTargetColumn, nameof(target));
        }

        internal virtual void SelectCore(Column source, Column target)
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
            WhereExpression = condition.DbExpression;

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
            Check.NotNull(orderByList, nameof(orderByList));
            VerifyOrderByList(orderByList);
            OrderByList = new ReadOnlyCollection<DbExpressionSort>(orderByList.Select(x => new DbExpressionSort(x.Column.DbExpression, x.Direction)).ToList());

            return this;
        }

        public DbQueryBuilder OrderBy(int offset, int fetch, params ColumnSort[] orderByList)
        {
            Check.NotNull(orderByList, nameof(orderByList));
            VerifyOrderByList(orderByList);
            VerifyOffsetFetch(offset, fetch);
            OrderByList = new ReadOnlyCollection<DbExpressionSort>(orderByList.Select(x => new DbExpressionSort(x.Column.DbExpression, x.Direction)).ToList());
            Offset = offset;
            Fetch = fetch;

            return this;
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

        internal DbQueryStatement BuildQueryStatement()
        {
            var normalizedSelectList = NormalizeSelectList();

            var result = TryBuildSimpleQuery(normalizedSelectList);
            if (result == null)
            {
                var from = GetFromClause(normalizedSelectList);
                if (from == null)
                    throw new InvalidOperationException(Strings.DbQueryBuilder_EmptyFrom);
                result = BuildSelectStatement(normalizedSelectList, from, WhereExpression, GetOrderBy());
            }
            return result;
        }

        private DbQueryStatement TryBuildSimpleUnionQuery(IList<ColumnMapping> normalizedSelectList)
        {
            if (Model.ParentModel != null || JoinTable != null)
                return null;

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

        internal virtual DbQueryStatement TryBuildSimpleQuery(IList<ColumnMapping> normallizedSelectList)
        {
            if (Model.ParentModel != null || SequentialKeyColumn != null)
                return null;

            var result = TryBuildSimpleUnionQuery(normallizedSelectList);
            if (result != null)
                return result;

            var fromQuery = FromClause as DbSelectStatement;
            if (fromQuery == null)
                return null;
            if (!fromQuery.IsSimple)
                return null;

            var fromModel = fromQuery.Model;
            var columnReplacer = new ColumnReplacer(fromQuery);
            var replacedSelectList = new ColumnMapping[normallizedSelectList.Count];
            for (int i = 0; i < normallizedSelectList.Count; i++)
            {
                var selectItem = normallizedSelectList[i];
                var source = selectItem.Source;
                var target = selectItem.TargetColumn;

                Debug.Assert(target.ParentModel == Model);
                replacedSelectList[i] = new ColumnMapping(columnReplacer.Replace(source), target);
            }

            var where = GetSimpleQueryWhere(columnReplacer, fromQuery.Where, WhereExpression);
            var orderBy = GetSimpleQueryOrderBy(columnReplacer, OrderByList);
            return new DbSelectStatement(Model, replacedSelectList, fromQuery.From, where, orderBy, Offset, Fetch);
        }

        private static DbExpression GetSimpleQueryWhere(ColumnReplacer columnReplacer, DbExpression where1, DbExpression where2)
        {
            if (where2 == null)
                return where1;

            where2 = columnReplacer.Replace(where2);
            return where1 == null ? where2 : new DbBinaryExpression(BinaryExpressionKind.And, where1, where2);
        }

        private static IList<DbExpressionSort> GetSimpleQueryOrderBy(ColumnReplacer columnReplacer, ReadOnlyCollection<DbExpressionSort> orderByList)
        {
            if (orderByList == null)
                return null;

            var result = new DbExpressionSort[orderByList.Count];
            for (int i = 0; i < orderByList.Count; i++)
            {
                var orderBy = orderByList[i];
                result[i] = new DbExpressionSort(columnReplacer.Replace(orderBy.Expression), orderBy.Direction);
            }

            return result;
        }

        private IDbTable GetParentSequentialKeyTempTable()
        {
            return GetParentSequentialKeyTempTable(Model.ParentModel);
        }

        private static IDbTable GetParentSequentialKeyTempTable(Model parentModel)
        {
            if (parentModel == null)
                return null;

            var parentQuery = parentModel.DataSource as IDbSet;
            if (parentQuery == null || parentQuery.Kind != DataSourceKind.DbQuery)
                return null;

            var parentSequentialKeyTempTable = parentQuery.QueryStatement.SequentialKeyTempTable;
            Debug.Assert(parentSequentialKeyTempTable != null);
            return parentSequentialKeyTempTable;
        }

        private IList<ColumnMapping> NormalizeSelectList()
        {
            var allColumns = Model.Columns;
            var parentSequentialKeyTempTable = GetParentSequentialKeyTempTable();
            var indexParentRowId = allColumns.Count;
            var countParentRowId = parentSequentialKeyTempTable != null ? 1 : 0;
            var indexRowId = indexParentRowId + countParentRowId;
            var countRowId = SequentialKeyColumn != null ? 1 : 0;
            var result = new ColumnMapping[indexRowId + countRowId];

            foreach (var selectItem in SelectList)
                result[selectItem.TargetColumn.Ordinal] = selectItem;

            for (int i = 0; i < allColumns.Count; i++)
            {
                if (result[i].TargetColumn == null)
                    result[i] = new ColumnMapping(DbConstantExpression.Null, allColumns[i]);
            }

            if (parentSequentialKeyTempTable != null)
            {
                _Int32 targetColumn = Model.GetSysParentRowIdColumn(createIfNotExist: true);
                var sourceColumn = GetSequentialKeyColumn(parentSequentialKeyTempTable);
                if (countParentRowId > 0)
                    result[indexParentRowId] = new ColumnMapping(sourceColumn, targetColumn);
            }

            if (SequentialKeyColumn != null)
            {
                var sourceColumn = SequentialKeyColumn;
                _Int32 targetColumn = Model.GetSysRowIdColumn(createIfNotExist: true);
                result[indexRowId] = new ColumnMapping(sourceColumn, targetColumn);
            }

            return result;
        }

        internal Column GetParentSequentialColumn()
        {
            return GetSequentialKeyColumn(GetParentSequentialKeyTempTable());
        }

        private DbFromClause GetFromClause(IList<ColumnMapping> normalizedSelectList)
        {
            Debug.Assert(FromClause != null);

            var from = FromClause;

            var parentModel = Model.ParentModel;
            if (parentModel != null)
            {
                var parentSequentialKeyTempTable = GetParentSequentialKeyTempTable(parentModel);
                if (parentSequentialKeyTempTable != null)
                    from = new DbJoinClause(DbJoinKind.InnerJoin, from, parentSequentialKeyTempTable.FromClause,
                        GetParentRelationship(normalizedSelectList, parentSequentialKeyTempTable.Model));
                else
                    from = new DbJoinClause(DbJoinKind.InnerJoin, from, parentModel.FromClause, GetParentRelationship(normalizedSelectList, parentModel));
            }

            if (JoinTable != null)
                from = new DbJoinClause(DbJoinKind.InnerJoin, from, JoinTable.FromClause, GetSequentialRelationship(normalizedSelectList, JoinTable));

            return from;
        }

        private ReadOnlyCollection<ColumnMapping> GetParentRelationship(IList<ColumnMapping> normalizedSelectList, Model targetModel)
        {
            var mappings = Model.ParentRelationship;
            return GetRelationship(mappings, normalizedSelectList, targetModel);
        }

        private ReadOnlyCollection<ColumnMapping> GetSequentialRelationship(IList<ColumnMapping> normalizedSelectList, IDbTable sequentialKeyTempTable)
        {
            var targetModel = sequentialKeyTempTable.Model;
            var relationship = Model.PrimaryKey.GetRelationship(targetModel.PrimaryKey);
            return GetRelationship(relationship, normalizedSelectList, targetModel);
        }

        private static ReadOnlyCollection<ColumnMapping> GetRelationship(ReadOnlyCollection<ColumnMapping> relationship, IList<ColumnMapping> normalizedSelectList, Model targetModel)
        {
            var result = new ColumnMapping[relationship.Count];
            for (int i = 0; i < relationship.Count; i++)
            {
                var mapping = relationship[i];
                var source = normalizedSelectList[mapping.SourceColumn.Ordinal].Source;
                var targetColumn = GetCorrespondingPrimaryKeyColumn(mapping.TargetColumn, targetModel);
                result[i] = new ColumnMapping(source, targetColumn);
            }
            return new ReadOnlyCollection<ColumnMapping>(result);
        }

        private static Column GetCorrespondingPrimaryKeyColumn(Column column, Model targetModel)
        {
            Debug.Assert(column != null);
            Debug.Assert(targetModel != null);

            if (column.ParentModel == targetModel)
                return column;

            var primaryKey = column.ParentModel.PrimaryKey;
            for (int i = 0; i < primaryKey.Count(); i++)
            {
                if (column == primaryKey[i].Column)
                    return targetModel.PrimaryKey[i].Column;
            }

            Debug.Fail("Cannot match primary key column.");
            return null;
        }

        private IList<DbExpressionSort> GetOrderBy()
        {
            if (SequentialKeyColumn != null)
            {
                return new DbExpressionSort[]
                {
                    GetDbExpressionSort(JoinTable.Model.GetIdentity(true))
                };
            }

            var parentModel = Model.ParentModel;
            if (parentModel != null)
                return GetOrderBy(GetParentIdentity(parentModel));

            return OrderByList;
        }

        private static Identity GetParentIdentity(Model parentModel)
        {
            Debug.Assert(parentModel != null);

            var parentDbSet = parentModel.DataSource as IDbSet;
            Debug.Assert(parentDbSet != null);
            return parentDbSet.Kind == DataSourceKind.DbQuery
                ? parentDbSet.QueryStatement.SequentialKeyTempTable.Model.GetIdentity(true) 
                : parentModel.GetIdentity(true);
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

        internal virtual DbSelectStatement BuildSelectStatement(IList<ColumnMapping> select, DbFromClause from, DbExpression where, IList<DbExpressionSort> orderBy)
        {
            return new DbSelectStatement(Model, select, from, where, orderBy, Offset, Fetch);
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
