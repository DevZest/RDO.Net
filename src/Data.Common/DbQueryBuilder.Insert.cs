using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    partial class DbQueryBuilder
    {
        internal DbSelectStatement BuildInsertStatement(Model sourceDataModel, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings, bool joinParent)
        {
            From(sourceDataModel);
            Select(columnMappings);
            return BuildInsertSelect(columnMappings, keyMappings, joinParent);
        }

        internal DbSelectStatement BuildInsertStatement(DbSelectStatement selectStatement, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings, bool joinParent)
        {
            Initialize(selectStatement);
            Select(columnMappings);
            return BuildInsertSelect(columnMappings, keyMappings, joinParent);
        }

        private DbSelectStatement BuildInsertSelect(IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings, bool joinParent)
        {
            if (joinParent)
            {
                var parentRelationship = Model.GetParentRelationship(columnMappings);
                Debug.Assert(parentRelationship != null);
                Join(Model.ParentModel, DbJoinKind.InnerJoin, parentRelationship);
            }

            if (keyMappings != null)
            {
                Join(Model, DbJoinKind.LeftJoin, keyMappings);
                var isNullExpr = new DbFunctionExpression(FunctionKeys.IsNull, new DbExpression[] { keyMappings[0].Target.DbExpression });
                WhereExpression = And(WhereExpression, isNullExpr);
            }

            var selectList = CanEliminateUnionSubQuery(SelectList) ? null : SelectList;
            return new DbSelectStatement(Model, selectList, FromClause, WhereExpression, OrderByList, Offset, Fetch);
        }
    }
}
