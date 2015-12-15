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
            var dataSetManager = uiElement.GetDataSetManager();
            return dataSetManager == null ? null : dataSetManager.Model;
        }

        private RepeatMode _repeatMode;
        public RepeatMode RepeatMode
        {
            get { return _repeatMode; }
            set
            {
                VerifyNotSealed();
            }
        }
    }
}
