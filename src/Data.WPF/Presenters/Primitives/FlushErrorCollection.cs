using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    internal sealed class FlushErrorCollection : KeyedCollection<UIElement, FlushError>
    {
        internal FlushErrorCollection(InputManager inputManager)
        {
            Debug.Assert(inputManager != null);
            _inputManager = inputManager;
        }

        private readonly InputManager _inputManager;

        protected override UIElement GetKeyForItem(FlushError item)
        {
            return item.Source;
        }

        public void SetFlushError(UIElement element, FlushError flushError)
        {
            Remove(element);
            if (flushError != null)
                Add(flushError);
            _inputManager.InvalidateView();
        }
    }

    internal static class FlushErrorCollectionExtensions
    {
        internal static FlushError GetFlushError(this FlushErrorCollection flushErrors, UIElement element)
        {
            if (flushErrors == null)
                return null;
            return flushErrors.Contains(element) ? flushErrors[element] : null;
        }
    }
}
