namespace DevZest.Data
{
    /// <summary>
    /// Represents a database table.
    /// </summary>
    public interface IDbTable : IDbSet
    {
        /// <summary>
        /// Gets the name of the database table.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the database table.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the identifier of the database table.
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Gets a value indicates whether this database table is in design mode.
        /// </summary>
        bool DesignMode { get; }
    }
}
