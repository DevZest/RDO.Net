using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public interface IBlockPresenter : IReadOnlyList<RowPresenter>
    {
        int Ordinal { get; }
    }
}
