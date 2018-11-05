﻿using DevZest.Data.Primitives;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace DevZest.Data.SqlServer
{
    internal static class SqlParameterExtensions
    {
        private static ConditionalWeakTable<SqlParameter, Tuple<SqlType, object, SqlVersion>> s_debugInfo = 
            new ConditionalWeakTable<SqlParameter, Tuple<SqlType, object, SqlVersion>>();

        internal static void SetDebugInfo(this SqlParameter sqlParameter, SqlType columnMapper, object value, SqlVersion sqlVersion)
        {
            Debug.Assert(sqlParameter != null);
            Debug.Assert(columnMapper != null);
            s_debugInfo.Add(sqlParameter, new Tuple<SqlType, object, SqlVersion>(columnMapper, value, sqlVersion));
        }

        private static void GetDebugInfo(this SqlParameter sqlParameter, out SqlType columnMapper, out object value, out SqlVersion sqlVersion)
        {
            Tuple<SqlType, object, SqlVersion> result = null;
            s_debugInfo.TryGetValue(sqlParameter, out result);
            columnMapper = result.Item1;
            value = result.Item2;
            sqlVersion = result.Item3;
        }

        internal static void GenerateDebugSql(this SqlParameter sqlParameter, IndentedStringBuilder sqlBuilder)
        {
            SqlType columnMapper;
            object value;
            SqlVersion sqlVersion;
            sqlParameter.GetDebugInfo(out columnMapper, out value, out sqlVersion);
            var dataTypeSql = columnMapper.GetDataTypeSql(sqlVersion);

            sqlBuilder.Append("DECLARE ")
                .Append(sqlParameter.ParameterName)
                .Append(" ")
                .Append(dataTypeSql);
            if (sqlParameter.Direction == ParameterDirection.Input || sqlParameter.Direction == ParameterDirection.InputOutput)
            {
                sqlBuilder.Append(" = ");
                ExpressionGenerator.GenerateConst(sqlBuilder, sqlVersion, columnMapper.GetColumn(), value);
            }
            sqlBuilder.AppendLine(";");
        }
    }
}
