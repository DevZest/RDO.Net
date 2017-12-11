namespace DevZest.Data.Annotations.Primitives
{
    public abstract class GeneralValidationColumnAttribute : ValidationColumnAttribute
    {
        protected sealed override IColumnValidationMessages Validate(Column column, DataRow dataRow)
        {
            return IsValid(column, dataRow) ? ColumnValidationMessages.Empty : new ColumnValidationMessage(Severity, GetMessage(column, dataRow), column);
        }

        protected abstract bool IsValid(Column column, DataRow dataRow);

        protected virtual ValidationSeverity Severity
        {
            get { return ValidationSeverity.Error; }
        }
    }
}
