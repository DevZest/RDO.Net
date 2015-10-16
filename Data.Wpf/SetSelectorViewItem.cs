using System;

namespace DevZest.Data.Wpf
{
    public class HeaderSelectorViewItem<T> : ModelViewItem<T>
        where T : SetSelector, new()
    {
        public HeaderSelectorViewItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        internal sealed override ViewItemKind Kind
        {
            get { return ViewItemKind.SetSelector; }
        }
    }
}
