
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidator
    {
        ValidatorId Id { get; }

        ValidationSeverity Severity { get; }

        IColumnSet Columns { get; }

        _Boolean IsValidCondition { get; }

        _String Message { get; }
    }
}
