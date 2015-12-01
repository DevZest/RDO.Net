
using System.Collections.Generic;

namespace DevZest.Data
{
    public struct ValidationResult
    {
        public ValidationResult(ValidatorId validatorId, ValidationLevel level, string message, IReadOnlyList<Column> columns)
        {
            ValidatorId = validatorId;
            Level = level;
            Message = message;
            Columns = columns;
        }

        public readonly ValidatorId ValidatorId;

        public readonly ValidationLevel Level;

        public readonly string Message;

        public readonly IReadOnlyList<Column> Columns;
    }
}
