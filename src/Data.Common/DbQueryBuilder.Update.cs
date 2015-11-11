using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    partial class DbQueryBuilder
    {
        internal DbSelectStatement BuildUpdateStatement(Model sourceDataModel, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            From(sourceDataModel);
            Select(columnMappings);
            return BuildUpdateSelect(columnMappings, keyMappings);
        }

        internal DbSelectStatement BuildUpdateStatement(DbSelectStatement selectStatement, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            Initialize(selectStatement);
            Select(columnMappings);
            return BuildUpdateSelect(columnMappings, keyMappings);
        }

        private DbSelectStatement BuildUpdateSelect(IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            Debug.Assert(keyMappings != null);

            Join(Model, DbJoinKind.InnerJoin, keyMappings);

            var selectList = CanEliminateUnionSubQuery(SelectList) ? null : SelectList;
            return new DbSelectStatement(Model, selectList, FromClause, WhereExpression, OrderByList, Offset, Fetch);
        }
    }
}
