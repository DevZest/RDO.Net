using System;

namespace DevZest.Data.Wpf
{
    public class SetSelectorViewItem<T> : ModelViewItem<T>
        where T : SetSelector, new()
    {
        public SetSelectorViewItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        internal sealed override bool Repeatable
        {
            get { return false; }
        }

        internal sealed override bool WithinDataRow
        {
            get { return false; }
        }
    }
}
