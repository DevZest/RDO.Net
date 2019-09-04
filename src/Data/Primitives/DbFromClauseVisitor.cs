namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a visitor for SQL FROM clause.
    /// </summary>
    public abstract class DbFromClauseVisitor
    {
        /// <summary>
        /// Visits the <see cref="DbTableClause"/>.
        /// </summary>
        /// <param name="table">The table clause.</param>
        public abstract void Visit(DbTableClause table);

        /// <summary>
        /// Visits the <see cref="DbSelectStatement"/>.
        /// </summary>
        /// <param name="select">The select statement.</param>
        public abstract void Visit(DbSelectStatement select);

        /// <summary>
        /// Visits the <see cref="DbJoinClause"/>.
        /// </summary>
        /// <param name="join">The join clause.</param>
        public abstract void Visit(DbJoinClause join);

        /// <summary>
        /// Visits the <see cref="DbUnionStatement"/>.
        /// </summary>
        /// <param name="union">The union statement.</param>
        public abstract void Visit(DbUnionStatement union);
    }
}
