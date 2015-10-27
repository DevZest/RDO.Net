using DevZest.Data.Primitives;
using System;
using System.Diagnostics;

namespace DevZest.Data.SqlServer
{
    internal static class DbJoinKindExtensions
    {
        internal static string ToSql(this DbJoinKind kind)
        {
            if (kind == DbJoinKind.InnerJoin)
                return "INNER JOIN";
            else if (kind == DbJoinKind.LeftJoin)
                return "LEFT JOIN";
            else if (kind == DbJoinKind.CrossJoin)
                return "CROSS JOIN";
            else
            {
                Debug.Assert(kind == DbJoinKind.RightJoin);
                return "RIGHT JOIN";
            }
        }
    }
}
