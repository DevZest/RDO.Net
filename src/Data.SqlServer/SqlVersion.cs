using System;

namespace DevZest.Data.SqlServer
{
    public enum SqlVersion
    {
        // <summary>
        // SQL Server 11 (2012).
        // </summary>
        Sql11 = 110,

        // <summary>
        // SQL Server 12 (2014).
        // </summary>
        Sql12 = 120,

        /// <summary>
        /// SQL Server 13 (2016).
        /// </summary>
        Sql13 = 130,

        /// <summary>
        /// SQL Server 14 (2017) or above.
        /// </summary>
        Sql14 = 140

        // Higher versions go here
    }
}
