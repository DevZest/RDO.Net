using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    partial class DbQueryBuilder
    {
        internal DbSelectStatement BuildInsertStatement(Model sourceDataModel, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            From(sourceDataModel);
            Initialize(columnMappings);
            return BuildInsertSelect(columnMappings, keyMappings);
        }

        internal DbSelectStatement BuildInsertStatement(DbSelectStatement selectStatement, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            Initialize(selectStatement);
            Initialize(columnMappings);
            return BuildInsertSelect(columnMappings, keyMappings);
        }

        private DbSelectStatement BuildInsertSelect(IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            var parentRelationship = Model.GetParentRelationship(columnMappings);
            if (parentRelationship != null)
                Join(Model.ParentModel, DbJoinKind.InnerJoin, parentRelationship);

            if (keyMappings != null)
            {
                Join(Model, DbJoinKind.LeftJoin, keyMappings);
                var isNullExpr = new DbFunctionExpression(FunctionKeys.IsNull, new DbExpression[] { keyMappings[0].TargetColumn.DbExpression });
                WhereExpression = And(WhereExpression, isNullExpr);
            }

            return new DbSelectStatement(Model, SelectList, FromClause, WhereExpression, OrderByList, Offset, Fetch);
        }
    }
}
