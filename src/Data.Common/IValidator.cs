using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidator
    {
        ValidatorId Id { get; }

        ValidationMessage Validate(DataRow dataRow);
    }
}
