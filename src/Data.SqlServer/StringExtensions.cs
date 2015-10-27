using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data.SqlServer
{
    internal static class StringExtensions
    {
        internal static string ToSingleQuoted(this string s)
        {
            return string.Format(CultureInfo.InvariantCulture, "'{0}'", s);
        }

        internal static string ToTSqlLiteral(this string s, bool isUnicode)
        {
            var format = isUnicode ? "N'{0}'" : "'{0}'";
            return string.Format(CultureInfo.InvariantCulture, format, s.Replace("'", "''"));
        }

        public static string ToQuotedIdentifier(this string identifier)
        {
            identifier = identifier.Trim();
            if (identifier[0] == '[')
                return identifier;
            return string.Format(CultureInfo.InvariantCulture, "[{0}]", identifier.Replace("]", "]]"));
        }

        public static SqlCommand CreateSqlCommand(this string commandText, SqlConnection sqlConnection, IEnumerable<SqlParameter> parameters = null)
        {
            var result = new SqlCommand();
            result.Connection = sqlConnection;
            if (parameters != null)
            {
                foreach (var param in parameters)
                    result.Parameters.Add(param);
            }
            result.CommandText = commandText;
            result.CommandType = CommandType.Text;
            return result;
        }

        public static IList<string> ParseIdentifier(this string s)
        {
            var result = new List<string>();
            int index = 0;
            while (index < s.Length)
                result.Add(ReadIdentifier(s, ref index));
            return result;
        }

        private static string ReadIdentifier(string s, ref int index)
        {
            if (s[index] == '[')
                return ReadQuotedIdentifier(s, ref index);

            int startIndex = index;
            while (index < s.Length)
            {
                if (s[index] == '[')
                    return s.Substring(startIndex, index - startIndex);
                else if (s[index] == '.')
                {
                    index++;
                    return s.Substring(startIndex, index - startIndex - 1);
                }
                else
                    index++;
            }

            return s.Substring(startIndex, index - startIndex);
        }

        private static string ReadQuotedIdentifier(string s, ref int index)
        {
            Debug.Assert(s[index] == '[');

            index++;
            int startIndex = index;
            while (index < s.Length)
            {
                if (s[index] == ']')
                {
                    index++;
                    if (index == s.Length || s[index] != ']')
                        break;
                }
                index++;
            }

            var result = s.Substring(startIndex, index - startIndex - 1);
            result = result.Replace("]]", "]");
            if (index < s.Length && s[index] == '.')
                index++;
            return result;
        }
    }
}
