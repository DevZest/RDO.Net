using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public class ColumnValueViewItem<T> : ColumnViewItem<T>
        where T : UIElement, new()
    {
        public ColumnValueViewItem(Column column, Action<T> initializer)
            : base(column, initializer)
        {
        }
    }
}
