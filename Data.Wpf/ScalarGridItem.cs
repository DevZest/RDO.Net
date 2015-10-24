using System;
using System.Collections.Generic;
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

        internal abstract void Refresh(DataSetView dataSetView, UIElement uiElement);

        internal abstract void Recycle(DataSetView dataSetView, UIElement uiElement);
    }

    public abstract class ScalarGridItem<T> : ScalarGridItem
        where T : UIElement, new()
    {
        protected ScalarGridItem(Model parentModel, Action<T> initializer)
            : base(parentModel)
        {
            _initializer = initializer;
        }

        Action<T> _initializer;
        List<T> _cachedUIElements;

        internal sealed override UIElement Generate(DataSetView dataSetView)
        {
            var result = GetOrCreate(_cachedUIElements);
            if (_initializer != null)
                _initializer(result);
            Initialize(dataSetView, result);
            Refresh(dataSetView, result);
            return result;
        }

        protected virtual void Initialize(DataSetView dataSetView, T uiElement)
        {
        }

        internal sealed override void Refresh(DataSetView dataSetView, UIElement uiElement)
        {
            Refresh(dataSetView, (T)uiElement);
        }

        protected abstract void Refresh(DataSetView dataSetView, T uiElement);

        internal sealed override void Recycle(DataSetView dataSetView, UIElement uiElement)
        {
            var element = (T)uiElement;
            Cleanup(dataSetView, element);
            Recycle(_cachedUIElements, element);
        }

        protected virtual void Cleanup(DataSetView dataSetView, T uiElement)
        {
        }
    }
}
