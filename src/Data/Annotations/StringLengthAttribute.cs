using DevZest.Data.Annotations.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the minimum and maximum length of characters that are allowed in a string column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column<string>) })]
    public sealed class StringLengthAttribute : ValidationColumnAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="StringLengthAttribute" /> class by using a specified maximum length.</summary>
        /// <param name="maximumLength">The maximum length of a string. </param>
        public StringLengthAttribute(int maximumLength)
        {
            if (MaximumLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumLength));
            MaximumLength = maximumLength;
        }

        /// <summary>Gets or sets the maximum length of a string.</summary>
		/// <returns>The maximum length a string. </returns>
        public int MaximumLength { get; private set; }

        private int _minimumLength;
        /// <summary>Gets or sets the minimum length of a string.</summary>
        /// <returns>The minimum length of a string.</returns>
        public int MinimumLength
        {
            get { return _minimumLength; }
            set
            {
                if (value < 0 || value > MaximumLength)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _minimumLength = value;
            }
        }

        /// <inheritdoc />
        protected override bool IsValid(Column column, DataRow dataRow)
        {
            if (column is Column<string> stringColumn)
                return IsValid(stringColumn[dataRow]);

            return true;
        }

        private bool IsValid(string value)
        {
            return value == null || (value.Length >= MinimumLength && value.Length <= MaximumLength);
        }

        /// <inheritdoc />
        protected override string DefaultMessageString
        {
            get
            {
                if (MinimumLength == 0)
                    return UserMessages.StringLengthAttribute;
                else
                    return UserMessages.StringLengthAttribute_WithMinLength;
            }
        }

        /// <inheritdoc />
        protected override string FormatMessage(string columnDisplayName)
        {
            return string.Format(CultureInfo.CurrentCulture, MessageString, columnDisplayName, MaximumLength, MinimumLength);
        }
    }
}
