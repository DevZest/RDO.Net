namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Defines the SQL JOIN operations.
    /// </summary>
    public enum DbJoinKind
    {
        /// <summary>
        /// SQL CROSS JOIN.
        /// </summary>
        CrossJoin,

        /// <summary>
        /// SQL INNER JOIN.
        /// </summary>
        InnerJoin,

        /// <summary>
        /// SQL LEFT JOIN.
        /// </summary>
        LeftJoin,

        /// <summary>
        /// SQL RIGHT JOIN.
        /// </summary>
        RightJoin
    }
}
