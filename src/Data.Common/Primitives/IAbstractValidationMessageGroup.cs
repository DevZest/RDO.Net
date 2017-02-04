using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public interface IAbstractValidationMessageGroup : IReadOnlyList<AbstractValidationMessage>
    {
        bool IsSealed { get; }

        IAbstractValidationMessageGroup Seal();

        IAbstractValidationMessageGroup Add(AbstractValidationMessage value);
    }
}
