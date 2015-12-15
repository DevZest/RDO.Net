using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class ChildGridItem<T> : ChildGridItem
        where T : DataSetControl, new()
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
            var dataSetControl = (T)uiElement;
            var parentDataRowManager = dataSetControl.GetDataRowManager();
            dataSetControl.DataSetManager = parentDataRowManager.Children[Template.ChildOrdinal];
            Debug.Assert(dataSetControl.DataSetManager.Template == Template);
            if (Initializer != null)
                Initializer(dataSetControl);
            OnMounted(dataSetControl);
        }

        protected virtual void OnMounted(T uiElement)
        {
        }

        internal sealed override void OnUnmounting(UIElement uiElement)
        {
            var dataSetControl = (T)uiElement;
            dataSetControl.DataSetManager = null;
            OnUnmounting(dataSetControl);
        }

        protected virtual void OnUnmounting(T uiElement)
        {
        }
    }
}
