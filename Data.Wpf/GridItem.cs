using DevZest.Data.Wpf.Resources;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class GridItem
    {
        protected GridItem(Model parentModel)
        {
            ParentModel = parentModel;
        }

        public Model ParentModel { get; private set; }

        public GridTemplate Owner { get; private set; }

        public GridRange GridRange { get; private set; }

        internal void Initialize(GridTemplate owner, GridRange gridRange)
        {
            Owner = owner;
            GridRange = gridRange;
        }

        internal void Clear()
        {
            Owner = null;
            GridRange = new GridRange();
        }

        public bool IsSealed
        {
            get { return Owner == null ? false : Owner.IsSealed; }
        }

        protected void VerifyIsSealed()
        {
            if (IsSealed)
                throw Error.GridTemplate_VerifyIsSealed();
        }

        internal abstract UIElement CreateUIElement();

        List<GridElement> _cachedUIElements = new List<GridElement>();
        internal GridElement Generate()
        {
            if (_cachedUIElements.Count == 0)
                return new GridElement(this, CreateUIElement());

            var last = _cachedUIElements.Count - 1;
            var result = _cachedUIElements[last];
            _cachedUIElements.RemoveAt(last);
            return result;
        }

        internal void Recycle(GridElement element)
        {
            Debug.Assert(element != null && element.Owner == this);
            _cachedUIElements.Add(element);
        }

        internal abstract void InitUIElement(UIElement uiElement);
    }
}
