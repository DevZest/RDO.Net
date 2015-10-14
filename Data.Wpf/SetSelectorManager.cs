using System;

namespace DevZest.Data.Wpf
{
    public class HeaderSelectorManager<T> : ModelViewManager<T>
        where T : SetSelector, new()
    {
        public HeaderSelectorManager(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        internal sealed override ViewManagerKind Kind
        {
            get { return ViewManagerKind.SetSelector; }
        }
    }
}
