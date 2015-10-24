using System;
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

        internal UIElement Generate(DataRowView dataRowView)
        {
            var result = InternalGetOrCreate();
            result.InternalSetDataRowView(dataRowView);
            InternalInitialize(result);
            Refresh(result);
            return result;
        }

        internal override void Recycle(UIElement uiElement)
        {
            uiElement.InternalSetDataRowView(null);
            base.Recycle(uiElement);
        }
    }

    public abstract class SetGridItem<T> : SetGridItem
        where T : UIElement, new()
    {
        protected SetGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        protected DataRowView GetDataRowView(T uiElement)
        {
            return uiElement.InternalGetDataRowView();
        }

        internal sealed override UIElement InternalCreate()
        {
            return new T();
        }

        internal sealed override void InternalInitialize(UIElement uiElement)
        {
            Initialize((T)uiElement);
        }

        protected abstract void Initialize(T uiElement);

        internal sealed override void Refresh(UIElement uiElement)
        {
            Refresh((T)uiElement);
        }

        protected abstract void Refresh(T uiElement);

        internal sealed override void Recycle(UIElement uiElement)
        {
            Cleanup((T)uiElement);
            base.Recycle(uiElement);
        }

        protected abstract void Cleanup(T uiElement);
    }
}
