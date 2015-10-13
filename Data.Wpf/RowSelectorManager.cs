using System;

namespace DevZest.Data.Wpf
{
    public sealed class RowSelectorManager<T> : ModelViewManager<T>
        where T : RowSelector, new()
    {
        public RowSelectorManager(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }
    }
}
