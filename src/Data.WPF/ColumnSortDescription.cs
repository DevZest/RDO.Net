using DevZest.Data;
using System;

namespace DevZest.Windows
{
    public struct ColumnSortDescription
    {
        public ColumnSortDescription(ColumnComparer columnComparer, SortDirection direction)
        {
            if (columnComparer == null)
                throw new ArgumentNullException(nameof(columnComparer));
            ColumnComparer = columnComparer;
            Direction = direction;
        }

        public readonly ColumnComparer ColumnComparer;
        public readonly SortDirection Direction;
    }
}
