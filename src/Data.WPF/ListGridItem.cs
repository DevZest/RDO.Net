using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ListGridItem<T> : GridItem<T>
        where T : UIElement, new()
    {
        protected ListGridItem(Model parentModel)
            : base(parentModel)
        {
        }
    }
}
