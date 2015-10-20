using System;

namespace DevZest.Data.Wpf
{
    public class GridExpanderViewItem<T> : ModelViewItem<T>
        where T : GridExpander, new()
    {
        public GridExpanderViewItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }
    }
}
