using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class GridItem
    {
        public GridTemplate Owner { get; private set; }

        public GridRange GridRange { get; private set; }

        internal void Seal(GridTemplate owner, GridRange gridRange)
        {
            Debug.Assert(owner != null);

            if (Owner != null)
                throw new InvalidOperationException(Strings.GridItem_OwnerAlreadySet);
            Owner = owner;
            GridRange = gridRange;
        }

        public bool IsSealed
        {
            get { return Owner != null; }
        }

        protected void VerifyNotSealed()
        {
            if (IsSealed)
                throw new InvalidOperationException(Strings.GridItem_VerifyNotSealed);
        }

        List<UIElement> _cachedUIElements;
        private UIElement GetOrCreate()
        {
            if (_cachedUIElements == null || _cachedUIElements.Count == 0)
                return Create();

            var last = _cachedUIElements.Count - 1;
            var result = _cachedUIElements[last];
            _cachedUIElements.RemoveAt(last);
            return result;
        }

        internal abstract UIElement Create();

        internal UIElement Generate()
        {
            return GetOrCreate();
        }

        internal abstract void OnMounted(UIElement uiElement);

        internal void Recycle(UIElement uiElement)
        {
            Debug.Assert(uiElement != null);

            OnUnmounting(uiElement);
            if (_cachedUIElements == null)
                _cachedUIElements = new List<UIElement>();
            _cachedUIElements.Add(uiElement);
        }

        internal abstract void OnUnmounting(UIElement uiElement);
    }
}
