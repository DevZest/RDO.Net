using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ScalarGridItem : GridItem
    {
        protected ScalarGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        internal UIElement Generate(DataSetView dataSetView)
        {
            var result = InternalGetOrCreate();
            result.InternalSetDataSetView(dataSetView);
            InternalInitialize(result);
            Refresh(result);
            return result;
        }

        internal override void Recycle(UIElement uiElement)
        {
            uiElement.InternalSetDataSetView(null);
            base.Recycle(uiElement);
        }
    }

    public abstract class ScalarGridItem<T> : ScalarGridItem
        where T : UIElement, new()
    {
        protected ScalarGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        protected DataSetView GetDataSetView(T uiElement)
        {
            return uiElement.InternalGetDataSetView();
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
