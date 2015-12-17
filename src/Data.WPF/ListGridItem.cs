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

        internal abstract void UpdateTarget(DataRowPresenter dataRowPresenter, UIElement uiElement);

        internal abstract void UpdateSource(UIElement uiElement, DataRowPresenter dataRowPresenter);
    }
}
