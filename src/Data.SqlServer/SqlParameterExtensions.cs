using DevZest.Data.Primitives;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevZest.Data.SqlServer
{
    internal static class SqlParameterExtensions
    {
        private static ConditionalWeakTable<SqlParameter, Tuple<SqlType, object, SqlVersion>> s_debugInfo = 
            new ConditionalWeakTable<SqlParameter, Tuple<SqlType, object, SqlVersion>>();

        internal static void SetDebugInfo(this SqlParameter sqlParameter, SqlType sqlType, object value, SqlVersion sqlVersion)
        {
            Debug.Assert(sqlParameter != null);
            Debug.Assert(sqlType != null);
            s_debugInfo.Add(sqlParameter, new Tuple<SqlType, object, SqlVersion>(sqlType, value, sqlVersion));
        }

        private static void GetDebugInfo(this SqlParameter sqlParameter, out SqlType sqlType, out object value, out SqlVersion sqlVersion)
        {
            Tuple<SqlType, object, SqlVersion> result = null;
            s_debugInfo.TryGetValue(sqlParameter, out result);
            sqlType = result.Item1;
            value = result.Item2;
            sqlVersion = result.Item3;
        }

        internal static void GenerateDebugSql(this SqlParameter sqlParameter, IndentedStringBuilder sqlBuilder)
        {
            sqlParameter.GetDebugInfo(out var sqlType, out var value, out var sqlVersion);
            var dataTypeSql = sqlType.GetDataTypeSql(sqlVersion);

            sqlBuilder.Append("DECLARE ")
                .Append(sqlParameter.ParameterName)
                .Append(" ")
                .Append(dataTypeSql);
            if (sqlParameter.Direction == ParameterDirection.Input || sqlParameter.Direction == ParameterDirection.InputOutput)
            {
                sqlBuilder.Append(" = ");
                ExpressionGenerator.GenerateConst(sqlBuilder, sqlVersion, sqlType.GetColumn(), value);
            }
            sqlBuilder.AppendLine(";");
        }
    }
}
