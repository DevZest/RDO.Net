
using DevZest.Data.Primitives;
using System.Globalization;

namespace DevZest.Data.SqlServer
{
    internal static class IndentedStringBuilderExtensions
    {
        internal static IndentedStringBuilder AppendSingleQuoted(this IndentedStringBuilder builder, string s)
        {
            return builder.AppendSingleQuoted(s, false);
        }

        internal static IndentedStringBuilder AppendSingleQuoted(this IndentedStringBuilder builder, string s, bool isUnicode)
        {
            string format = isUnicode ? "N'{0}'" : "'{0}'";
            return builder.AppendFormat(CultureInfo.InvariantCulture, format, s.Replace("'", "''"));
        }

        internal static IndentedStringBuilder AppendSingleQuoted(this IndentedStringBuilder builder, char value, bool isUnicode)
        {
            if (isUnicode)
                builder.Append('N');
            builder.Append("'");
            if (value == '\'')
                builder.Append("''");
            else
                builder.Append(value);
            builder.Append("'");
            return builder;
        }
    }
}
