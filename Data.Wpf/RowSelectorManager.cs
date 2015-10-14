using System;

namespace DevZest.Data.Wpf
{
    public sealed class DataRowSelectorManager<T> : ModelViewManager<T>
        where T : RowSelector, new()
    {
        public DataRowSelectorManager(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        internal sealed override ViewManagerKind Kind
        {
            get { return ViewManagerKind.RowSelector; }
        }
    }
}
