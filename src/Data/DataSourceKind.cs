using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents the kind of DataSources
    /// </summary>
    public enum DataSourceKind
    {
        /// <summary>
        /// DataSet, an in-memory collection of data.
        /// </summary>
        DataSet,

        /// <summary>
        /// Database query.
        /// </summary>
        DbQuery,

        /// <summary>
        /// Permanent database table.
        /// </summary>
        DbTable,

        /// <summary>
        /// Temporary database table.
        /// </summary>
        DbTempTable
    }
}
