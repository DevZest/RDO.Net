using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public class ColumnValueManager<T> : ColumnViewManager<T>
        where T : UIElement, new()
    {
        public ColumnValueManager(Column column, Action<T> initializer)
            : base(column, initializer)
        {
        }
    }
}
