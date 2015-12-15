using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ListGridItem : GridItem
    {
        internal ListGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        internal abstract void UpdateTarget(DataRowManager dataRowManager, UIElement uiElement);

        internal abstract void UpdateSource(UIElement uiElement, DataRowManager dataRowManager);
    }
}
