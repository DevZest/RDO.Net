using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class ChildGridItem<T> : ChildGridItem
        where T : DataSetView, new()
    {
        public ChildGridItem(GridTemplate template)
            : base(template)
        {
        }

        private Action<T> _initializer;
        public Action<T> Initializer
        {
            get { return _initializer; }
            set
            {
                VerifyNotSealed();
                _initializer = value;
            }
        }

        internal sealed override UIElement Create()
        {
            return new T();
        }

        internal sealed override void OnMounted(UIElement uiElement)
        {
            var dataSetView = (T)uiElement;
            var parentDataRowPresenter = dataSetView.GetDataRowPresenter();
            dataSetView.Presenter = parentDataRowPresenter.Children[Template.ChildOrdinal];
            Debug.Assert(dataSetView.Presenter.Template == Template);
            if (Initializer != null)
                Initializer(dataSetView);
            OnMounted(dataSetView);
        }

        protected virtual void OnMounted(T uiElement)
        {
        }

        internal sealed override void OnUnmounting(UIElement uiElement)
        {
            var dataSetView = (T)uiElement;
            dataSetView.Presenter = null;
            OnUnmounting(dataSetView);
        }

        protected virtual void OnUnmounting(T uiElement)
        {
        }
    }
}
