using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public class ColumnHeaderManager<T> : ColumnViewManager<T>
        where T : ColumnHeader, new()
    {
        public ColumnHeaderManager(Column column, Action<T> initializer)
            : base(column, initializer)
        {
        }
    }
}
