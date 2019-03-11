using DevZest.Data.Annotations.Primitives;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column<string>) })]
    public sealed class RegularExpressionAttribute : ValidationColumnAttribute
    {
        public RegularExpressionAttribute(string pattern)
        {
            Pattern = pattern.VerifyNotEmpty(nameof(pattern));
        }

        public string Pattern { get; private set; }

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            var stringColumn = column as Column<string>;
            return stringColumn == null ? false : IsValid(stringColumn[dataRow]);
        }

        private Regex Regex { get; set; }

        private bool IsValid(string text)
        {
            SetupRegex();
            if (text == null)
                return true;
            Match match = Regex.Match(text);
            return match.Success && match.Index == 0 && match.Length == text.Length;
        }

        private void SetupRegex()
        {
            if (Regex == null)
                Regex = new Regex(this.Pattern);
        }

        protected override string DefaultMessageString
        {
            get { return UserMessages.RegularExpressionAttribute; }
        }

        protected override string FormatMessage(string columnDisplayName)
        {
            return string.Format(CultureInfo.CurrentCulture, MessageString, columnDisplayName, Pattern);
        }
    }
}
