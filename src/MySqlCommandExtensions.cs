using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql
{
    internal static class SqlCommandExtensions
    {
        internal static string ToTraceString(this MySqlCommand mySqlCommand)
        {
            var result = new IndentedStringBuilder();

            var parameters = mySqlCommand.Parameters;
            var paramCount = parameters.Count;
            for (int i = 0; i < paramCount; i++)
            {
                var param = parameters[i];
                param.GenerateDebugSql(result);
            }

            if (paramCount > 0)
                result.AppendLine();

            result.Append(mySqlCommand.CommandText);
            return result.ToString();
        }
    }
}
