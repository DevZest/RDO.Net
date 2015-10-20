using System;

namespace DevZest.Data.Wpf
{
    public class DataSetSelectorViewItem<T> : ModelViewItem<T>, IScalarViewItem
        where T : DataSetSelector, new()
    {
        public DataSetSelectorViewItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }
    }
}
