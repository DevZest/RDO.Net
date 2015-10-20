using System;

namespace DevZest.Data.Wpf
{
    public sealed class DataRowSelectorViewItem<T> : ModelViewItem<T>
        where T : DataRowSelector, new()
    {
        public DataRowSelectorViewItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }
    }
}
