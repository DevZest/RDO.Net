using DevZest.Data.Annotations.Primitives;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies that a data column must match the specified regular expression.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column<string>) })]
    public sealed class RegularExpressionAttribute : ValidationColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance <see cref="RegularExpressionAttribute"/>.
        /// </summary>
        /// <param name="pattern">The regular expression which is used for validation.</param>
        public RegularExpressionAttribute(string pattern)
        {
            Pattern = pattern.VerifyNotEmpty(nameof(pattern));
        }

        /// <summary>
        /// Gets the regular expression which is used for validation.
        /// </summary>
        public string Pattern { get; private set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override string DefaultMessageString
        {
            get { return UserMessages.RegularExpressionAttribute; }
        }

        /// <inheritdoc />
        protected override string FormatMessage(string columnDisplayName)
        {
            return string.Format(CultureInfo.CurrentCulture, MessageString, columnDisplayName, Pattern);
        }
    }
}
