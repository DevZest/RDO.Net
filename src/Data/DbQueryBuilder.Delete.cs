using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    partial class DbQueryBuilder
    {
        internal DbSelectStatement BuildDeleteStatement(Model sourceDataModel, IReadOnlyList<ColumnMapping> keyMappings)
        {
            From(sourceDataModel);
            return BuildDeleteSelect(keyMappings);
        }

        internal DbSelectStatement BuildDeleteStatement(DbSelectStatement selectStatement, IReadOnlyList<ColumnMapping> keyMappings)
        {
            Initialize(selectStatement);
            return BuildDeleteSelect(keyMappings);
        }

        private DbSelectStatement BuildDeleteSelect(IReadOnlyList<ColumnMapping> keyMappings)
        {
            Debug.Assert(keyMappings != null);

            Join(Model, DbJoinKind.InnerJoin, keyMappings);

            return new DbSelectStatement(Model, null, FromClause, WhereExpression, OrderByList, Offset, Fetch);
        }
    }
}
