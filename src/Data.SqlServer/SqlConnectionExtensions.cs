using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data.SqlServer
{
    internal static class SqlConnectionExtensions
    {
        internal static SqlVersion GetSqlVersion(this SqlConnection sqlConnection)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                sqlConnection.Open();

            var serverVersion = sqlConnection.ServerVersion;
            var indexOfFirstDot = serverVersion.IndexOf('.');
            if (indexOfFirstDot != -1)
                serverVersion = serverVersion.Substring(0, indexOfFirstDot);
            var majorVersion = Int32.Parse(serverVersion, CultureInfo.InvariantCulture);

            if (majorVersion >= 14)
                return SqlVersion.Sql14;
            else if (majorVersion == 13)
                return SqlVersion.Sql13;
            else
                throw new NotSupportedException(DiagnosticMessages.SqlVersionNotSupported(serverVersion));
        }
    }
}
