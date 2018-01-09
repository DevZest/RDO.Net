using System.Collections.ObjectModel;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    internal sealed class FlushErrorCollection : KeyedCollection<UIElement, FlushErrorMessage>
    {
        protected override UIElement GetKeyForItem(FlushErrorMessage item)
        {
            return item.Source;
        }
    }
}
