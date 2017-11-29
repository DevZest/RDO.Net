using DevZest.Data.Annotations.Primitives;
using System;
using System.Text.RegularExpressions;

namespace DevZest.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PhoneAttribute : GeneralValidationColumnAttribute
    {
        private static Regex s_regex = CreateRegEx();

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            var stringColumn = column as Column<string>;
            return stringColumn == null ? false : IsValid(stringColumn[dataRow]);
        }

        private static bool IsValid(string text)
        {
            return text != null && s_regex.Match(text).Length > 0;
        }

        protected override string GetDefaultMessage(Column column, DataRow dataRow)
        {
            return Strings.PhoneAttribute_DefaultErrorMessage;
        }

        private static Regex CreateRegEx()
        {
            return new Regex("^(\\+\\s?)?((?<!\\+.*)\\(\\+?\\d+([\\s\\-\\.]?\\d+)?\\)|\\d+)([\\s\\-\\.]?(\\(\\d+([\\s\\-\\.]?\\d+)?\\)|\\d+))*(\\s?(x|ext\\.?)\\s?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        }
    }
}
