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

        internal sealed override void UpdateTarget(DataRowPresenter dataRowPresenter, UIElement uiElement)
        {
            UpdateTarget(dataRowPresenter, (T)uiElement);
        }

        internal sealed override void UpdateSource(UIElement uiElement, DataRowPresenter dataRowPresenter)
        {
            UpdateSource((T)uiElement, dataRowPresenter);
        }

        protected abstract void UpdateTarget(DataRowPresenter dataRowPresenter, T element);

        protected abstract void UpdateSource(T element, DataRowPresenter dataRowPresenter);

        protected TValue GetValue<TValue>(Column<TValue> column, DataRowPresenter dataRowPresenter)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (dataRowPresenter == null)
                throw new ArgumentNullException(nameof(dataRowPresenter));

            return column[dataRowPresenter.DataRow];
        }


        private bool _isSettingValue;
        protected void SetValue<TValue>(DataRowPresenter dataRowPresenter, Column<TValue> column, TValue value)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (dataRowPresenter == null)
                throw new ArgumentNullException(nameof(dataRowPresenter));

            _isSettingValue = true;
            try
            {
                column[dataRowPresenter.DataRow] = value;
            }
            finally
            {
                _isSettingValue = false;
            }
        }
    }
}
