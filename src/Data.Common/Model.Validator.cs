namespace DevZest.Data
{
    partial class Model
    {
        protected static class Validator
        {
            private sealed class SimpleValidator : IValidator
            {
                public SimpleValidator(string messageId, ValidationSeverity severity, IColumns columns, _Boolean isValid, _String message)
                {
                    MessageId = messageId;
                    Severity = severity;
                    Columns = columns;
                    IsValid = isValid;
                    Message = message;
                }

                public string MessageId { get; private set; }
                public ValidationSeverity Severity { get; private set; }
                public IColumns Columns { get; private set; }
                public _Boolean IsValid { get; private set; }
                public _String Message { get; private set; }

                public string GetMessage(DataRow dataRow)
                {
                    return Message[dataRow];
                }

                IColumnValidationMessages IValidator.Validate(DataRow dataRow)
                {
                    return IsValid[dataRow] == true ? ColumnValidationMessages.Empty : new ColumnValidationMessage(MessageId, Severity, Message[dataRow], Columns);
                }
            }

            public static IValidator Create(string messageId, ValidationSeverity severity, IColumns columns, _Boolean isValid, _String message)
            {
                Utilities.Check.NotEmpty(messageId, nameof(messageId));
                if (columns == null)
                    columns = Data.Columns.Empty;
                Utilities.Check.NotNull(isValid, nameof(isValid));
                Utilities.Check.NotNull(message, nameof(message));

                return new SimpleValidator(messageId, severity, columns, isValid, message);
            }
        }
    }
}
