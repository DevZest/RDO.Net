namespace DevZest.Data
{
    /// <summary>
    /// Represents the database initialization progress.
    /// </summary>
    public struct DbInitProgress
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbInitProgress"/>.
        /// </summary>
        /// <param name="dbTable">The database table.</param>
        /// <param name="index">The index of database table.</param>
        /// <param name="count">The total number of database tables.</param>
        public DbInitProgress(IDbTable dbTable, int index, int count)
        {
            DbTable = dbTable;
            Index = index;
            Count = count;
        }

        /// <summary>
        /// Gets the database table currently being initialized.
        /// </summary>
        public IDbTable DbTable { get; }

        /// <summary>
        /// Gets the index of the database table.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the total number of database tables.
        /// </summary>
        public int Count { get; }
    }
}
