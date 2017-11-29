using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StringLengthAttribute : GeneralValidationColumnAttribute
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

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            return IsValid(((Column<string>)column)[dataRow]);
        }

        private bool IsValid(string value)
        {
            return value == null || (value.Length >= MinimumLength && value.Length <= MaximumLength);
        }

        protected override string GetDefaultMessage(Column column, DataRow dataRow)
        {
            if (MinimumLength == 0)
                return Strings.StringLengthAttribute_DefaultErrorMessage(column.DisplayName, MaximumLength);
            else
                return Strings.StringLengthAttribute_DefaultErrorMessageWithMinLength(column.DisplayName, MinimumLength, MaximumLength);
        }
    }
}
