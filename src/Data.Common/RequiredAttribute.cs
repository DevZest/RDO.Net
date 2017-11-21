using DevZest.Data.Primitives;
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

        protected override IColumnValidationMessages Validate(Column column, DataRow dataRow)
        {
            return column.IsNull(dataRow) ? new ColumnValidationMessage(MessageId, ValidationSeverity.Error, null, column) : ColumnValidationMessages.Empty;
        }

        protected override string GetDefaultMessage(Column column, DataRow dataRow)
        {
            return Strings.RequiredAttribute_DefaultErrorMessage(column);
        }
    }
}
