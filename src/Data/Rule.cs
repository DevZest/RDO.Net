namespace DevZest.Data
{
    /// <summary>
    /// Specifies what action happens to rows have a referential relationship and the referenced row is deleted or updated from the parent table.
    /// The default is NoAction.
    /// </summary>
    public enum Rule
    {
        /// <summary>
        /// The Database Engine raises an error and the delete or update action on the row in the parent table is rejected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Corresponding rows are deleted or updated from the referencing table if that row is deleted or updated from the parent table.
        /// </summary>
        Cascade,

        /// <summary>
        /// All the values that make up the foreign key are set to NULL if the corresponding row in the parent table is deleted or updated.
        /// </summary>
        SetNull,

        /// <summary>
        /// All the values that make up the foreign key are set to their default values if the corresponding row in the parent table is deleted or updated. 
        /// </summary>
        SetDefault
    }
}
