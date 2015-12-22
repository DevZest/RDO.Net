
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidator
    {
        ValidatorId Id { get; }

        ValidationLevel Level { get; }

        IColumnSet Columns { get; }

        _Boolean IsValidCondition { get; }

        _String Message { get; }
    }
}
