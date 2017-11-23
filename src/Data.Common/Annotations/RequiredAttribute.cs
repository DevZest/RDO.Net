using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredAttribute : ValidatorColumnAttribute
    {
        protected override void Initialize(Column column)
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
