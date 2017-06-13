using System.Collections.Generic;

namespace DevZest.Windows
{
    public interface IColumnSortService : IDataPresenterService
    {
        IReadOnlyList<ColumnSortDescriptor> Descriptors { get; set; }
    }
}
