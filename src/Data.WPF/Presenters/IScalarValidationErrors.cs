using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IScalarValidationErrors : IReadOnlyList<ScalarValidationError>
    {
        bool IsSealed { get; }
        IScalarValidationErrors Seal();
        IScalarValidationErrors Add(ScalarValidationError value);
    }
}
