namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a vistor to return a result from SQL FROM clause.
    /// </summary>
    /// <typeparam name="T">Type of result.</typeparam>
    public abstract class DbFromClauseVisitor<T>
    {
        /// <summary>
        /// Visits the <see cref="DbTableClause"/>.
        /// </summary>
        /// <param name="table">The table clause.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbTableClause table);

        /// <summary>
        /// Visits the <see cref="DbSelectStatement"/>.
        /// </summary>
        /// <param name="select">The select statement.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbSelectStatement select);

        /// <summary>
        /// Visits the <see cref="DbJoinClause"/>.
        /// </summary>
        /// <param name="join">The join clause.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbJoinClause join);

        /// <summary>
        /// Visits the <see cref="DbUnionStatement"/>.
        /// </summary>
        /// <param name="union">The union statement.</param>
        /// <returns>The result.</returns>
        public abstract T Visit(DbUnionStatement union);
    }
}
