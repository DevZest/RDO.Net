using DevZest.Data.Annotations.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column<Binary>) })]
    public sealed class MaxLengthAttribute : ValidationColumnAttribute
    {
        private const int MaxAllowableLength = -1;

        public int Length { get; private set; }

        protected override string DefaultMessageString => UserMessages.MaxLengthAttribute;

        public MaxLengthAttribute(int length)
        {
            if (length <= 0)
                throw new ArgumentException(UserMessages.MaxLengthAttribute_InvalidMaxLength, nameof(length));
            Length = length;
        }

        public MaxLengthAttribute()
        {
            Length = MaxAllowableLength;
        }

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            if (column is Column<Binary> binaryColumn)
                return IsValid(binaryColumn[dataRow]);

            return true;
        }

        private bool IsValid(Binary value)
        {
            return value == null || Length == MaxAllowableLength || value.Length <= Length;
        }

        protected override string FormatMessage(string columnDisplayName)
        {
            return string.Format(CultureInfo.CurrentCulture, MessageString, columnDisplayName, Length);
        }
    }
}
