using System;
using System.Data;
using System.Globalization;

namespace DevZest.Data.SqlServer
{
    internal static class SqlDbTypeExtensions
    {
        public static bool IsUnicode(this SqlDbType sqlDbType)
        {
            return sqlDbType == SqlDbType.NVarChar || sqlDbType == SqlDbType.NChar;
        }
    }
}
