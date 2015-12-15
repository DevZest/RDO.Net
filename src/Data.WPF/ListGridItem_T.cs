using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ListGridItem<T> : ListGridItem
        where T : UIElement, new()
    {
        protected ListGridItem(Model parentModel)
            : base(parentModel)
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
            var element = (T)uiElement;
            if (Initializer != null)
                Initializer(element);
            OnMounted(element);
        }

        protected virtual void OnMounted(T uiElement)
        {
        }

        internal sealed override void OnUnmounting(UIElement uiElement)
        {
            OnUnmounting((T)uiElement);
        }

        protected virtual void OnUnmounting(T uiElement)
        {
        }

        internal sealed override void UpdateTarget(DataRowManager dataRowManager, UIElement uiElement)
        {
            UpdateTarget(dataRowManager, (T)uiElement);
        }

        internal sealed override void UpdateSource(UIElement uiElement, DataRowManager dataRowManager)
        {
            UpdateSource((T)uiElement, dataRowManager);
        }

        protected abstract void UpdateTarget(DataRowManager dataRowManager, T element);

        protected abstract void UpdateSource(T element, DataRowManager dataRowManager);

        protected TValue GetValue<TValue>(Column<TValue> column, DataRowManager dataRowManager)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (dataRowManager == null)
                throw new ArgumentNullException(nameof(dataRowManager));

            return column[dataRowManager.DataRow];
        }


        private bool _isSettingValue;
        protected void SetValue<TValue>(DataRowManager dataRowManager, Column<TValue> column, TValue value)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (dataRowManager == null)
                throw new ArgumentNullException(nameof(dataRowManager));

            _isSettingValue = true;
            try
            {
                column[dataRowManager.DataRow] = value;
            }
            finally
            {
                _isSettingValue = false;
            }
        }
    }
}
