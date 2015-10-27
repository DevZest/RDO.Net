using DevZest.Data.Primitives;
using System;
using System.Data.SqlClient;
using System.Text;

namespace DevZest.Data.SqlServer
{
    internal static class SqlCommandExtensions
    {
        internal static string ToTraceString(this SqlCommand sqlCommand)
        {
            var result = new IndentedStringBuilder();

            var parameters = sqlCommand.Parameters;
            var paramCount = parameters.Count;
            for (int i = 0; i < paramCount; i++)
            {
                var param = parameters[i];
                param.GenerateDebugSql(result);
            }

            if (paramCount > 0)
                result.AppendLine();

            result.Append(sqlCommand.CommandText);
            return result.ToString();
        }
    }
}
