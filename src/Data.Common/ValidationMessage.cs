using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System.Collections.Generic;

namespace DevZest.Data
{
    public class ValidationMessage
    {
        public ValidationMessage(ValidatorId validatorId, ValidationSeverity severity, string description, params Column[] columns)
            : this(validatorId, severity, ColumnSet.New(columns), description)
        {
        }

        public ValidationMessage(ValidatorId validatorId, ValidationSeverity severity, IColumnSet columns, string description)
        {
            Check.NotEmpty(description, nameof(description));

            ValidatorId = validatorId;
            Severity = severity;
            Columns = columns ?? ColumnSet.Empty;
            Description = description;
        }

        public readonly ValidatorId ValidatorId;

        public readonly ValidationSeverity Severity;

        public readonly string Description;

        public readonly IColumnSet Columns;
    }
}
