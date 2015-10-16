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

        internal override bool Repeatable
        {
            get { return true; }
        }

        internal override bool WithinDataRow
        {
            get { return false; }
        }
    }
}
