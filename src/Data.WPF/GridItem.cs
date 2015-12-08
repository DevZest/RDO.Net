using System;
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

        internal void Seal(GridTemplate owner, GridRange gridRange)
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
                throw new InvalidOperationException(Strings.GridTemplate_VerifyIsSealed);
        }

        internal abstract UIElement Generate();

        internal abstract void Initialize(UIElement uiElement);

        internal abstract void Recycle(UIElement uiElement);
    }
}
