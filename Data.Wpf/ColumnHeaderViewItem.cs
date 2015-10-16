using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public class ColumnHeaderViewItem<T> : ColumnViewItem<T>
        where T : ColumnHeader, new()
    {
        public ColumnHeaderViewItem(Column column, Action<T> initializer)
            : base(column, initializer)
        {
        }

        internal sealed override ViewItemKind Kind
        {
            get { return ViewItemKind.ColumnHeader; }
        }
    }
}
