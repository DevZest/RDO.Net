namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a visitor for database expression trees.
    /// </summary>
    public abstract class DbExpressionVisitor
    {
        /// <summary>
        /// Visits the <see cref="DbConstantExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract void Visit(DbConstantExpression expression);

        /// <summary>
        /// Visits the <see cref="DbParamExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract void Visit(DbParamExpression expression);

        /// <summary>
        /// Visits the <see cref="DbColumnExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract void Visit(DbColumnExpression expression);

        /// <summary>
        /// Visits the <see cref="DbUnaryExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract void Visit(DbUnaryExpression expression);

        /// <summary>
        /// Visits the <see cref="DbBinaryExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract void Visit(DbBinaryExpression expression);

        /// <summary>
        /// Visits the <see cref="DbFunctionExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract void Visit(DbFunctionExpression expression);

        /// <summary>
        /// Visits the <see cref="DbCastExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract void Visit(DbCastExpression expression);

        /// <summary>
        /// Visits the <see cref="DbCaseExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract void Visit(DbCaseExpression expression);
    }
}
