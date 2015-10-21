using System;

namespace DevZest.Data.Wpf
{
    public class DataSetSelectorGridItem<T> : ModelGridItem<T>, IScalarViewItem
        where T : DataSetSelector, new()
    {
        public DataSetSelectorGridItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }
    }
}
