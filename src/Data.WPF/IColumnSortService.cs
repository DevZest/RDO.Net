using System.Collections.Generic;

namespace DevZest.Windows
{
    public interface IColumnSortService : IDataPresenterService
    {
        IReadOnlyList<ColumnSortDescription> SortDescriptions { get; set; }
    }
}
