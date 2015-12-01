using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidator
    {
        ValidatorId Id { get; }

        IEnumerable<ValidationResult> Validate(DataRow dataRow);
    }
}
