using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IScalarValidationMessages : IReadOnlyList<ScalarValidationMessage>
    {
        IScalarValidationMessages Seal();
        IScalarValidationMessages Add(ScalarValidationMessage value);
    }
}
