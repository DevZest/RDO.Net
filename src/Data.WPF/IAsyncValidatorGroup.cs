using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public interface IAsyncValidatorGroup : IReadOnlyList<AsyncValidator>
    {
        bool IsSealed { get; }
        IAsyncValidatorGroup Seal();
        IAsyncValidatorGroup Add(AsyncValidator value);
    }
}
