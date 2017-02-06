using DevZest.Data.Primitives;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidationMessageGroup : IReadOnlyList<ValidationMessage>, IAbstractValidationMessageGroup
    {
        new IValidationMessageGroup Seal();

        new int Count { get; }

        new ValidationMessage this[int index] { get; }

        new IEnumerator<ValidationMessage> GetEnumerator();

        IValidationMessageGroup Add(ValidationMessage value);
    }
}
