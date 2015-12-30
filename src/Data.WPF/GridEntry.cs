using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class GridEntry
    {
        internal GridEntry(Func<UIElement> constructor)
        {
            Debug.Assert(constructor != null);

        }

        public GridTemplate Owner { get; private set; }

        public GridRange GridRange { get; private set; }

        public int Ordinal { get; private set; }

        internal void Seal(GridTemplate owner, GridRange gridRange, int ordinal)
        {
            Debug.Assert(owner != null);

            if (Owner != null)
                throw new InvalidOperationException(Strings.GridEntry_OwnerAlreadySet);
            Owner = owner;
            GridRange = gridRange;
            Ordinal = ordinal;
        }

        Func<UIElement> _constructor;
        List<UIElement> _cachedUIElements;
        private UIElement GetOrCreate()
        {
            if (_cachedUIElements == null || _cachedUIElements.Count == 0)
                return _constructor();

            var last = _cachedUIElements.Count - 1;
            var result = _cachedUIElements[last];
            _cachedUIElements.RemoveAt(last);
            return result;
        }

        internal UIElement Generate()
        {
            return GetOrCreate();
        }

        internal virtual void OnMounted(UIElement uiElement)
        {
        }

        internal virtual void Refresh(UIElement uiElement)
        {
        }

        internal void Recycle(UIElement uiElement)
        {
            Debug.Assert(uiElement != null);

            OnUnmounting(uiElement);
            if (_cachedUIElements == null)
                _cachedUIElements = new List<UIElement>();
            _cachedUIElements.Add(uiElement);
        }

        internal virtual void OnUnmounting(UIElement uiElement)
        {
        }
    }
}
