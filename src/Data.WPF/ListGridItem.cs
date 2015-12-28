using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ListGridItem : GridItem
    {
        internal abstract void UpdateTarget(DataRowPresenter dataRowPresenter, UIElement uiElement);

        internal abstract void UpdateSource(UIElement uiElement, DataRowPresenter dataRowPresenter);
    }
}
