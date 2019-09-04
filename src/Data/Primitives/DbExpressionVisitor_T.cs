namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a vistor to return a result from database expression trees.
    /// </summary>
    /// <typeparam name="T">Type of result.</typeparam>
    public abstract class DbExpressionVisitor<T>
    {
        /// <summary>
        /// Visits the <see cref="DbConstantExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbConstantExpression expression);

        /// <summary>
        /// Visits the <see cref="DbParamExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbParamExpression expression);

        /// <summary>
        /// Visits the <see cref="DbColumnExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbColumnExpression expression);

        /// <summary>
        /// Visits the <see cref="DbUnaryExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbUnaryExpression expression);

        /// <summary>
        /// Visits the <see cref="DbBinaryExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbBinaryExpression expression);

        /// <summary>
        /// Visits the <see cref="DbFunctionExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbFunctionExpression expression);

        /// <summary>
        /// Visits the <see cref="DbCastExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbCastExpression expression);

        /// <summary>
        /// Visits the <see cref="DbCaseExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbCaseExpression expression);
    }
}
