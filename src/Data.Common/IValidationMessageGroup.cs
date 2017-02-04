using DevZest.Data.Primitives;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidationMessageGroup : IReadOnlyList<ValidationMessage>, IAbstractValidationMessageGroup
    {
        new IValidationMessageGroup Seal();

        new int Count { get; }

        IValidationMessageGroup Add(ValidationMessage value);
    }
}
