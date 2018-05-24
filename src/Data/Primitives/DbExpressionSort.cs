using System;

namespace DevZest.Data.Primitives
{
    public struct DbExpressionSort
    {
        public DbExpressionSort(DbExpression expression, SortDirection direction)
        {
            Expression = expression;
            Direction = direction;
        }

        public readonly DbExpression Expression;

        public readonly SortDirection Direction;
    }
}
