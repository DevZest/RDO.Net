using DevZest.Data.MySql.Addons;
using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevZest.Data.MySql
{
    internal static class MySqlParameterExtensions
    {
        private static ConditionalWeakTable<MySqlParameter, Tuple<MySqlType, object, MySqlVersion>> s_debugInfo =
            new ConditionalWeakTable<MySqlParameter, Tuple<MySqlType, object, MySqlVersion>>();

        internal static void SetDebugInfo(this MySqlParameter mySqlParameter, MySqlType mySqlType, object value, MySqlVersion mySqlVersion)
        {
            Debug.Assert(mySqlParameter != null);
            Debug.Assert(mySqlType != null);
            s_debugInfo.Add(mySqlParameter, new Tuple<MySqlType, object, MySqlVersion>(mySqlType, value, mySqlVersion));
        }

        private static void GetDebugInfo(this MySqlParameter mySqlParameter, out MySqlType mySqlType, out object value, out MySqlVersion mySqlVersion)
        {
            Tuple<MySqlType, object, MySqlVersion> result = null;
            s_debugInfo.TryGetValue(mySqlParameter, out result);
            mySqlType = result.Item1;
            value = result.Item2;
            mySqlVersion = result.Item3;
        }

        internal static void GenerateDebugSql(this MySqlParameter mySqlParameter, IndentedStringBuilder sqlBuilder)
        {
            mySqlParameter.GetDebugInfo(out var sqlType, out var value, out var sqlVersion);
            var dataTypeSql = sqlType.GetDataTypeSql(sqlVersion);

            sqlBuilder.Append("DECLARE ")
                .Append(mySqlParameter.ParameterName)
                .Append(" ")
                .Append(dataTypeSql);
            if (mySqlParameter.Direction == ParameterDirection.Input || mySqlParameter.Direction == ParameterDirection.InputOutput)
            {
                sqlBuilder.Append(" = ");
                ExpressionGenerator.GenerateConst(sqlBuilder, sqlVersion, sqlType.GetColumn(), value);
            }
            sqlBuilder.AppendLine(";");
        }
    }
}
