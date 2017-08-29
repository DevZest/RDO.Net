using DevZest.Data.Primitives;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidationMessageGroup : IReadOnlyList<ValidationMessage>
    {
        IValidationMessageGroup Seal();
        IValidationMessageGroup Add(ValidationMessage value);
    }
}
