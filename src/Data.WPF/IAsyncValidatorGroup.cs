using System.Collections.Generic;

namespace DevZest.Windows
{
    public interface IAsyncValidatorGroup : IReadOnlyList<AsyncValidator>
    {
        bool IsSealed { get; }
        IAsyncValidatorGroup Seal();
        IAsyncValidatorGroup Add(AsyncValidator value);
    }
}
