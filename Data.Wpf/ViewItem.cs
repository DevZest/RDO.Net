using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ViewItem
    {
        internal DataSetView Owner { get; private set; }

        public GridRange GridRange { get; private set; }

        internal ViewItem Initialize(DataSetView owner, GridRange gridRange)
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

        internal abstract bool IsValidFor(Model model);

        internal abstract UIElement CreateUIElement();

        List<ViewElement> _cachedUIElements = new List<ViewElement>();
        internal ViewElement Generate()
        {
            if (_cachedUIElements.Count == 0)
                return new ViewElement(this, CreateUIElement());

            var last = _cachedUIElements.Count - 1;
            var result = _cachedUIElements[last];
            _cachedUIElements.RemoveAt(last);
            return result;
        }

        internal void Recycle(ViewElement element)
        {
            Debug.Assert(element != null && element.Owner == this);
            _cachedUIElements.Add(element);
        }

        internal abstract void InitUIElement(UIElement uiElement);
    }
}
