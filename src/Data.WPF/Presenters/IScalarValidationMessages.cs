using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IScalarValidationMessages : IReadOnlyList<ScalarValidationMessage>
    {
        bool IsSealed { get; }
        IScalarValidationMessages Seal();
        IScalarValidationMessages Add(ScalarValidationMessage value);
    }
}
