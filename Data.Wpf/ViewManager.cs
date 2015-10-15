
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ViewManager
    {
        public DataSetControl Owner { get; private set; }

        public GridRange GridRange { get; private set; }

        internal bool Repeatable
        {
            get { return Kind != ViewManagerKind.SetSelector; }
        }

        internal ViewManager Initialize(DataSetControl owner, GridRange gridRange)
        {
            Owner = owner;
            GridRange = gridRange;
            return this;
        }

        internal void Clear()
        {
            Owner = null;
            GridRange = new GridRange();
        }

        internal abstract ViewManagerKind Kind { get; }

        internal abstract bool IsValidFor(Model model);

        internal abstract UIElement CreateUIElement();

        List<UIElement> _cachedUIElements = new List<UIElement>();
        internal UIElement GetUIElement2()
        {
            if (_cachedUIElements.Count == 0)
                return CreateUIElement().SetViewManager(this);

            var last = _cachedUIElements.Count - 1;
            var result = _cachedUIElements[last];
            _cachedUIElements.RemoveAt(last);
            return result;
        }

        internal void ReturnUIElement(UIElement uiElement)
        {
            Debug.Assert(uiElement != null && uiElement.GetViewManager() == this);

            uiElement.SetViewManager(null);
            _cachedUIElements.Add(uiElement);
        }

        internal abstract void InitUIElement(UIElement uiElement);
    }
}
