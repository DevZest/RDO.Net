using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    partial class DbQueryBuilder
    {
        internal virtual DbQueryStatement BuildQueryStatement(DbSelectStatement query, Action<DbQueryBuilder> action, DbTable<SequentialKeyModel> sequentialKeys)
        {
            var sourceModel = query.Model;
            Debug.Assert(Model.GetType() == sourceModel.GetType());

            Column sysParentRowId = sourceModel.GetSysParentRowIdColumn(createIfNotExist: false);
            if (sysParentRowId != null && sysParentRowId.Ordinal < query.Select.Count)
                Model.GetSysParentRowIdColumn(createIfNotExist: true);

            Column sysRowId = sourceModel.GetSysRowIdColumn(createIfNotExist: false);
            if (sysRowId != null && sysRowId.Ordinal < query.Select.Count)
                Model.GetSysRowIdColumn(createIfNotExist: true);

            var columns = Model.Columns;
            var sourceColumns = sourceModel.Columns;

            Debug.Assert(columns.Count <= sourceColumns.Count);
            Debug.Assert(columns.Count <= query.Select.Count);
            for (int i = 0; i < columns.Count; i++)
                SelectList.Add(new ColumnMapping(query.Select[i].Source, columns[i]));
            FromClause = query.From;
            WhereExpression = query.Where;
            OrderByList = query.OrderBy;

            if (action != null)
                action(this);

            return BuildQueryStatement(sequentialKeys);
        }

        internal DbQueryStatement BuildQueryStatement(Model sourceModel, Action<DbQueryBuilder> action, DbTable<SequentialKeyModel> sequentialKeys)
        {
            From(sourceModel);
            var sourceColumns = sourceModel.Columns;
            var targetColumns = Model.Columns;
            Debug.Assert(targetColumns.Count <= sourceColumns.Count);
            for (int i = 0; i < targetColumns.Count; i++)
            {
                var targetColumn = targetColumns[i];
                SelectCore(sourceColumns[targetColumn.Key], targetColumn);
            }

            if (action != null)
                action(this);

            return BuildQueryStatement(sequentialKeys);
        }

        internal DbQueryStatement BuildQueryStatement(DbTable<SequentialKeyModel> sequentialKeys)
        {
            var parentRowIdIdentity = SelectSysParentRowId();
            var rowIdIdentity = SelectSysRowId(sequentialKeys);

            var select = NormalizeSelectList();
            if (parentRowIdIdentity == null && rowIdIdentity == null)
            {
                var result = EliminateUnionSubQuery(select);
                if (result != null)
                    return result;
            }
            var orderBy = GetOrderBy(parentRowIdIdentity, rowIdIdentity);
            return BuildSelectStatement(select, FromClause, WhereExpression, orderBy);
        }

        private DbQueryStatement EliminateUnionSubQuery(IList<ColumnMapping> normalizedSelectList)
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
            return result;
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

        internal void Where(DataRow parentRow, IList<ColumnMapping> parentRelationship)
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
