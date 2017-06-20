using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IAsyncValidatorGroup : IReadOnlyList<AsyncValidator>
    {
        bool IsSealed { get; }
        IAsyncValidatorGroup Seal();
        IAsyncValidatorGroup Add(AsyncValidator value);
    }
}
