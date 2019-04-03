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
            if (mySqlParameter.Direction == ParameterDirection.Input || mySqlParameter.Direction == ParameterDirection.InputOutput)
            {
                mySqlParameter.GetDebugInfo(out var mySqlType, out var value, out var mySqlVersion);
                sqlBuilder.Append("SET ")
                    .Append(mySqlParameter.ParameterName)
                    .Append(" = ");
                ExpressionGenerator.GenerateConst(sqlBuilder, mySqlVersion, mySqlType.GetColumn(), value);
                sqlBuilder.AppendLine(";");
            }
        }
    }
}
