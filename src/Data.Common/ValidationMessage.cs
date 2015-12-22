using System.Collections.Generic;

namespace DevZest.Data
{
    public class ValidationMessage
    {
        public ValidationMessage(ValidatorId validatorId, ValidationLevel level, string description, IColumnSet columns)
        {
            ValidatorId = validatorId;
            Level = level;
            Description = description;
            Columns = columns ?? ColumnSet.Empty;
        }

        public readonly ValidatorId ValidatorId;

        public readonly ValidationLevel Level;

        public readonly string Description;

        public readonly IColumnSet Columns;
    }
}
