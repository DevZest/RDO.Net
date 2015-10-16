using System;

namespace DevZest.Data.Wpf
{
    public sealed class DataRowSelectorViewItem<T> : ModelViewItem<T>
        where T : RowSelector, new()
    {
        public DataRowSelectorViewItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        internal sealed override bool Repeatable
        {
            get { return true; }
        }

        internal sealed override bool WithinDataRow
        {
            get { return true; }
        }
    }
}
