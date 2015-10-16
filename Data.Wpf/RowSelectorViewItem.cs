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

        internal sealed override ViewItemKind Kind
        {
            get { return ViewItemKind.RowSelector; }
        }
    }
}
