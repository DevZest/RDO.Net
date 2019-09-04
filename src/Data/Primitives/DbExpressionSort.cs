namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a database expression and sorting direction.
    /// </summary>
    public struct DbExpressionSort
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbExpressionSort"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="direction">The sort direction.</param>
        public DbExpressionSort(DbExpression expression, SortDirection direction)
        {
            Expression = expression;
            Direction = direction;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public readonly DbExpression Expression;

        /// <summary>
        /// Gets the sorting direction.
        /// </summary>
        public readonly SortDirection Direction;
    }
}
