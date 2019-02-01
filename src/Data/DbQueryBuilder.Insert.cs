using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    partial class DbQueryBuilder
    {
        internal DbSelectStatement BuildInsertStatement(Model sourceDataModel, IReadOnlyList<ColumnMapping> columnMappings, bool joinParent)
        {
            From(sourceDataModel);
            Select(columnMappings);
            return BuildInsertSelect(columnMappings, joinParent);
        }

        internal DbSelectStatement BuildInsertStatement(DbSelectStatement selectStatement, IReadOnlyList<ColumnMapping> columnMappings, bool joinParent)
        {
            Initialize(selectStatement);
            Select(columnMappings);
            return BuildInsertSelect(columnMappings, joinParent);
        }

        private DbSelectStatement BuildInsertSelect(IReadOnlyList<ColumnMapping> columnMappings, bool joinParent)
        {
            if (joinParent)
            {
                var parentRelationship = Model.GetParentRelationship(columnMappings);
                Debug.Assert(parentRelationship != null);
                Join(Model.ParentModel, DbJoinKind.InnerJoin, parentRelationship);
            }

            var selectList = CanEliminateUnionSubQuery(SelectList) ? null : SelectList;
            return new DbSelectStatement(Model, selectList, FromClause, WhereExpression, OrderByList, Offset, Fetch);
        }
    }
}
