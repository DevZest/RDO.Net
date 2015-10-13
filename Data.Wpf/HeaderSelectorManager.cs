using System;

namespace DevZest.Data.Wpf
{
    public class HeaderSelectorManager<T> : ModelViewManager<T>
        where T : HeaderSelector, new()
    {
        public HeaderSelectorManager(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }
    }
}
