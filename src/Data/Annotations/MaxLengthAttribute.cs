using DevZest.Data.Annotations.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column<Binary>) })]
    public sealed class MaxLengthAttribute : ValidationColumnAttribute
    {
        public int Length { get; }

        protected override string DefaultMessageString => UserMessages.MaxLengthAttribute;

        public MaxLengthAttribute(int length)
        {
            if (length <= 0)
                throw new ArgumentException(UserMessages.MaxLengthAttribute_InvalidMaxLength, nameof(length));
            Length = length;
        }

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

        protected override string FormatMessage(string columnDisplayName)
        {
            return string.Format(CultureInfo.CurrentCulture, MessageString, columnDisplayName, Length);
        }
    }
}
