using DevZest.Data.Utilities;
using System.Collections.Generic;

namespace DevZest.Data
{
    public class ValidationMessage
    {
        public ValidationMessage(ValidatorId validatorId, ValidationLevel level, string description, params Column[] columns)
            : this(validatorId, level, ColumnSet.Create(columns), description)
        {
        }

        public ValidationMessage(ValidatorId validatorId, ValidationLevel level, IColumnSet columns, string description)
        {
            Check.NotEmpty(description, nameof(description));

            ValidatorId = validatorId;
            Level = level;
            Columns = columns ?? ColumnSet.Empty;
            Description = description;
        }

        public readonly ValidatorId ValidatorId;

        public readonly ValidationLevel Level;

        public readonly string Description;

        public readonly IColumnSet Columns;
    }
}
