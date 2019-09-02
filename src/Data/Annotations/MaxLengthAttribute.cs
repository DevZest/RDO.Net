using DevZest.Data.Annotations.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the maximum length of binary data allowed in a column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column<Binary>) })]
    public sealed class MaxLengthAttribute : ValidationColumnAttribute
    {
        /// <summary>
        /// Gets the maximum allowable length.
        /// </summary>
        public int Length { get; }

        /// <inheritdoc />
        protected override string DefaultMessageString => UserMessages.MaxLengthAttribute;

        /// <summary>
        /// Initializes a new instance of <see cref="MaxLengthAttribute"/> based on the length parameter.
        /// </summary>
        /// <param name="length"></param>
        public MaxLengthAttribute(int length)
        {
            if (length <= 0)
                throw new ArgumentException(UserMessages.MaxLengthAttribute_InvalidMaxLength, nameof(length));
            Length = length;
        }

        /// <inheritdoc />
        protected override bool IsValid(Column column, DataRow dataRow)
        {
            if (column is Column<Binary> binaryColumn)
                return IsValid(binaryColumn[dataRow]);

            return true;
        }

        private bool IsValid(Binary value)
        {
            return value == null || value.Length <= Length;
        }

        /// <inheritdoc />
        protected override string FormatMessage(string columnDisplayName)
        {
            return string.Format(CultureInfo.CurrentCulture, MessageString, columnDisplayName, Length);
        }
    }
}
