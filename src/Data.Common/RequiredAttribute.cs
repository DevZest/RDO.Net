using System;

namespace DevZest.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredAttribute : ColumnValidatorAttribute
    {
        protected internal override void Initialize(Column column)
        {
            base.Initialize(column);
            column.Nullable(false);
        }

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            return !column.IsNull(dataRow);
        }

        protected override string FormatMessage(Column column, DataRow dataRow)
        {
            return Strings.RequiredAttribute_DefaultErrorMessage(column);
        }
    }
}
