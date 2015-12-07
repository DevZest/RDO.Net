using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ScalarGridItem<T> : GridItem
        where T : UIElement, new()
    {
        protected ScalarGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        protected Model GetModel(T uiElement)
        {
            var dataSetView = uiElement.GetDataSetView();
            return dataSetView == null ? null : dataSetView.Model;
        }
    }
}
