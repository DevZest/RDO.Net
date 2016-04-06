using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public interface IBlockPresenter : IReadOnlyList<RowPresenter>
    {
        DataPresenter DataPresenter { get; }
        int Index { get; }
        int Dimensions { get; }
    }
}
