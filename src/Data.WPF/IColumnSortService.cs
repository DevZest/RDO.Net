using DevZest.Data;
using System.Collections.Generic;

namespace DevZest.Windows
{
    public interface IColumnSortService : IDataPresenterService
    {
        IReadOnlyList<ColumnSort> OrderBy { get; set; }
    }
}
