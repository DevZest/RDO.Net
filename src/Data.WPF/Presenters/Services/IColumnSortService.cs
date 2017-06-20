using DevZest.Data;
using System.Collections.Generic;

namespace DevZest.Data.Presenters.Services
{
    public interface IColumnSortService : IDataPresenterService
    {
        IReadOnlyList<ColumnSort> OrderBy { get; set; }
    }
}
