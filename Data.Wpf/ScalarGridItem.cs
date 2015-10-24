using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ScalarGridItem : GridItem
    {
        protected ScalarGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        internal abstract UIElement Generate(DataSetView dataSetView);

        public new abstract void Recycle(UIElement uiElement);
    }
}
