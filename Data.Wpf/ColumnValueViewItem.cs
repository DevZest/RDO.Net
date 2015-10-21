using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public class ColumnValueGridItem<T> : ColumnGridItem<T>
        where T : UIElement, new()
    {
        public ColumnValueGridItem(Column column, Action<T> initializer)
            : base(column, initializer)
        {
        }
    }
}
