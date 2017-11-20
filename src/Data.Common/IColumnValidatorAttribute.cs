using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IColumnValidatorAttribute
    {
        IValidator GetValidatorToAdd(IReadOnlyList<IValidator> existingValidators, Column column);
    }
}
