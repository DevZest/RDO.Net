using System;

namespace DevZest.Data.Wpf
{
    public sealed class DataRowSelectorGridItem<T> : ModelGridItem<T>
        where T : DataRowSelector, new()
    {
        public DataRowSelectorGridItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }
    }
}
