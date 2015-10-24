using DevZest.Data.Windows.Resources;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
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

        public bool IsSealed
        {
            get { return Owner == null ? false : Owner.IsSealed; }
        }

        protected void VerifyIsSealed()
        {
            if (IsSealed)
                throw Error.GridTemplate_VerifyIsSealed();
        }

        internal static T GetOrCreate<T>(List<T> cachedUIElements)
            where T : UIElement, new()
        {
            if (cachedUIElements == null || cachedUIElements.Count == 0)
                return new T();

            var last = cachedUIElements.Count - 1;
            var result = cachedUIElements[last];
            cachedUIElements.RemoveAt(last);
            return result;
        }

        internal static void Recycle<T>(List<T> cachedUIElements, T uiElement)
            where T : UIElement, new()
        {
            Debug.Assert(uiElement != null);
            if (cachedUIElements == null)
                cachedUIElements = new List<T>();
            cachedUIElements.Add(uiElement);
        }
    }
}
