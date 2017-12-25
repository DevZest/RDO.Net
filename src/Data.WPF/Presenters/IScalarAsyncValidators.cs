using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IScalarAsyncValidators : IReadOnlyList<ScalarAsyncValidator>
    {
        bool IsSealed { get; }
        IScalarAsyncValidators Seal();
        IScalarAsyncValidators Add(ScalarAsyncValidator value);
        ScalarAsyncValidator this[IScalars sourceScalars] { get; }
    }
}
