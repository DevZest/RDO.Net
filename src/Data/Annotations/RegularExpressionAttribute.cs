using DevZest.Data.Annotations.Primitives;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RegularExpressionAttribute : ValidationColumnAttribute
    {
        public RegularExpressionAttribute(string pattern)
        {
            Pattern = pattern.VerifyNotEmpty(nameof(pattern));
        }

        public string Pattern { get; private set; }

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            return IsValid(column.GetValue(dataRow));
        }

        private Regex Regex { get; set; }

        private bool IsValid(object value)
        {
            SetupRegex();
            string text = Convert.ToString(value, CultureInfo.CurrentCulture);
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
