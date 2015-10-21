using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public class ColumnHeaderGridItem<T> : ColumnGridItem<T>, IScalarViewItem
        where T : ColumnHeader, new()
    {
        public ColumnHeaderGridItem(Column column, Action<T> initializer)
            : base(column, initializer)
        {
        }
    }
}
