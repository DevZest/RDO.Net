using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IAsyncValidators : IReadOnlyList<AsyncValidator>
    {
        bool IsSealed { get; }
        IAsyncValidators Seal();
        IAsyncValidators Add(AsyncValidator value);
    }
}
