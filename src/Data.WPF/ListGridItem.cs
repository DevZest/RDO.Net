using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ListGridItem<T> : GridItem<T>, IListGridItem
        where T : UIElement, new()
    {
        protected ListGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        void IListGridItem.UpdateTarget(DataRowManager dataRowManager, UIElement uiElement)
        {
            UpdateTarget(dataRowManager, (T)uiElement);
        }

        void IListGridItem.UpdateSource(UIElement uiElement, DataRowManager dataRowManager)
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
