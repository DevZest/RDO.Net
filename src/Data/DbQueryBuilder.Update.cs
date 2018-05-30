using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    partial class DbQueryBuilder
    {
        internal DbSelectStatement BuildUpdateStatement(Model sourceDataModel, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> keyMappings)
        {
            From(sourceDataModel);
            Select(columnMappings);
            return BuildUpdateSelect(columnMappings, keyMappings);
        }

        internal DbSelectStatement BuildUpdateStatement(DbSelectStatement selectStatement, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> keyMappings)
        {
            Initialize(selectStatement);
            Select(columnMappings);
            return BuildUpdateSelect(columnMappings, keyMappings);
        }

        private DbSelectStatement BuildUpdateSelect(IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> keyMappings)
        {
            Debug.Assert(keyMappings != null);

            Join(Model, DbJoinKind.InnerJoin, keyMappings);

            var selectList = CanEliminateUnionSubQuery(SelectList) ? null : SelectList;
            return new DbSelectStatement(Model, selectList, FromClause, WhereExpression, OrderByList, Offset, Fetch);
        }
    }
}
