using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class SetGridItem : GridItem
    {
        protected SetGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        internal virtual GridTemplate Template
        {
            get { return null; }
        }

        internal abstract UIElement Generate(DataRowView dataRowView);

        internal abstract void Refresh(DataRowView dataRowView, UIElement uiElement);

        internal abstract void Recycle(DataRowView dataRowView, UIElement uiElement);
    }

    public abstract class SetGridItem<T> : SetGridItem
        where T : UIElement, new()
    {
        protected SetGridItem(Model parentModel, Action<T> initializer)
            : base(parentModel)
        {
            _initializer = initializer;
        }

        Action<T> _initializer;
        List<T> _cachedUIElements;

        internal override UIElement Generate(DataRowView dataRowView)
        {
            var result = GetOrCreate(_cachedUIElements);
            if (_initializer != null)
                _initializer(result);
            Initialize(dataRowView, result);
            Refresh(dataRowView, result);
            return result;
        }

        protected virtual void Initialize(DataRowView dataRowView, T uiElement)
        {
        }

        internal sealed override void Refresh(DataRowView dataRowView, UIElement uiElement)
        {
            Refresh(dataRowView, (T)uiElement);
        }

        protected abstract void Refresh(DataRowView dataRowView, T uiElement);

        internal sealed override void Recycle(DataRowView dataRowView, UIElement uiElement)
        {
            var element = (T)uiElement;
            Cleanup(dataRowView, element);
            Recycle(_cachedUIElements, element);
        }

        protected virtual void Cleanup(DataRowView dataRowView, T uiElement)
        {
        }
    }
}
