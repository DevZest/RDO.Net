using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IScalarValidationMessageGroup : IReadOnlyList<ScalarValidationMessage>
    {
        IScalarValidationMessageGroup Seal();
        IScalarValidationMessageGroup Add(ScalarValidationMessage value);
    }
}
