using DevZest.Data.Annotations.Primitives;
using System;
using System.Text.RegularExpressions;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(Column<string>) })]
    public sealed class PhoneAttribute : ValidationColumnAttribute, ILogicalDataTypeAttribute
    {
        public LogicalDataType LogicalDataType => LogicalDataType.PhoneNumber;

        private static Regex s_regex = CreateRegEx();

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            var stringColumn = column as Column<string>;
            return stringColumn == null ? false : IsValid(stringColumn[dataRow]);
        }

        private static bool IsValid(string text)
        {
            return text == null || s_regex.Match(text).Length > 0;
        }

        protected override string DefaultMessageString
        {
            get { return UserMessages.PhoneAttribute; }
        }

        private static Regex CreateRegEx()
        {
            return new Regex("^(\\+\\s?)?((?<!\\+.*)\\(\\+?\\d+([\\s\\-\\.]?\\d+)?\\)|\\d+)([\\s\\-\\.]?(\\(\\d+([\\s\\-\\.]?\\d+)?\\)|\\d+))*(\\s?(x|ext\\.?)\\s?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        }
    }
}
