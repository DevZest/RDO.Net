using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    internal sealed class FlushingErrorCollection : KeyedCollection<UIElement, FlushingError>
    {
        internal FlushingErrorCollection(InputManager inputManager)
        {
            Debug.Assert(inputManager != null);
            _inputManager = inputManager;
        }

        private readonly InputManager _inputManager;

        protected override UIElement GetKeyForItem(FlushingError item)
        {
            return item.Source;
        }

        public void SetFlushError(UIElement element, FlushingError flushError)
        {
            Remove(element);
            if (flushError != null)
                Add(flushError);
            _inputManager.InvalidateView();
        }
    }

    internal static class FlushingErrorCollectionExtensions
    {
        internal static FlushingError GetFlushingError(this FlushingErrorCollection flushingErrors, UIElement element)
        {
            if (flushingErrors == null)
                return null;
            return flushingErrors.Contains(element) ? flushingErrors[element] : null;
        }
    }
}
