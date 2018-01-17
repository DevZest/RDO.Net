using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidationErrors : IReadOnlyList<ValidationError>
    {
        IValidationErrors Seal();
        IValidationErrors Add(ValidationError value);
    }
}
