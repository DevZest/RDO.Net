using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    partial class Model
    {
        protected static class Validator
        {
            private sealed class SimpleValidator : IValidator
            {
                public SimpleValidator(string messageId, ValidationSeverity severity, Column[] columns, _Boolean isValidCondition, _String message)
                {
                    MessageId = messageId;
                    Severity = severity;
                    Columns = ColumnSet.New(columns);
                    ValidCondition = isValidCondition;
                    Message = message;
                }

                public string MessageId { get; private set; }
                public ValidationSeverity Severity { get; private set; }
                public IColumnSet Columns { get; private set; }
                public _Boolean ValidCondition { get; private set; }
                public _String Message { get; private set; }
            }

            public static IValidator Create(string messageId, ValidationSeverity severity, _Boolean isValidCondition, _String message, params Column[] columns)
            {
                Utilities.Check.NotNull(isValidCondition, nameof(isValidCondition));
                Utilities.Check.NotNull(message, nameof(message));

                return new SimpleValidator(messageId, severity, columns, isValidCondition, message);
            }
        }
    }
}
