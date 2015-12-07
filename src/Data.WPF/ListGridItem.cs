using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ListGridItem<T> : GridItem<T>
        where T : UIElement, new()
    {
        protected ListGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        protected DataRowView GetDataRowView(T element)
        {
            return element.GetDataRowView();
        }

        protected TValue GetValue<TValue>(Column<TValue> column, DataRowView dataRowView)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (dataRowView == null)
                throw new ArgumentNullException(nameof(dataRowView));

            return column[dataRowView.DataRow];
        }


        private bool _isSettingValue;
        protected void SetValue<TValue>(DataRowView dataRowView, Column<TValue> column, TValue value)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (dataRowView == null)
                throw new ArgumentNullException(nameof(dataRowView));

            _isSettingValue = true;
            try
            {
                column[dataRowView.DataRow] = value;
            }
            finally
            {
                _isSettingValue = false;
            }
        }

        internal override void UpdateTarget(UIElement element)
        {
            if (!_isSettingValue)
                base.UpdateTarget(element);
        }
    }
}
