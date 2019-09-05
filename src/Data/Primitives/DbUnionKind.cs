namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Specifies the kind of SQL UNION.
    /// </summary>
    public enum DbUnionKind
    {
        /// <summary>
        /// SQL UNION for distinct values.
        /// </summary>
        Union,

        /// <summary>
        /// SQL UNION for all values.
        /// </summary>
        UnionAll
    }
}
