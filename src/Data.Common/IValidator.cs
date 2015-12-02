
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidator
    {
        ValidatorId Id { get; }

        ValidationLevel Level { get; }

        IReadOnlyList<Column> Columns { get; }

        _Boolean IsValidCondition { get; }

        _String Message { get; }
    }
}
