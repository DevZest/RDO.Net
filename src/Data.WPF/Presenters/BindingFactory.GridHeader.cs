using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Windows.Controls.Primitives;

namespace DevZest.Data.Presenters
{
    static partial class BindingFactory
    {
        /// <summary>
        /// Binds <see cref="DataPresenter"/> to <see cref="GridHeader"/> to select/deselect all rows.
        /// </summary>
        /// <param name="dataPresenter">The DataPresenter.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<GridHeader> BindToGridHeader(this DataPresenter dataPresenter)
        {
            return ToSelectAll<GridHeader>(dataPresenter);
        }

        private static ScalarBinding<T> ToSelectAll<T>(this DataPresenter dataPresenter)
            where T : ToggleButton, new()
        {
            if (dataPresenter == null)
                throw new ArgumentNullException(nameof(dataPresenter));

            var trigger = new PropertyChangedTrigger<T>(ToggleButton.IsCheckedProperty).WithExecuteAction(v =>
            {
                var binding = (TwoWayBinding)v.GetBinding();
                if (binding.IsRefreshing)
                    return;
                var isChecked = v.IsChecked;
                if (!isChecked.HasValue)
                    return;

                var isSelected = isChecked.GetValueOrDefault();
                var rows = dataPresenter.Rows;
                bool anyChange = false;
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var oldValue = row.IsSelected;
                    row.IsSelected = isSelected;
                    if (row.IsSelected != oldValue)
                        anyChange = true;
                }
                if (!anyChange)
                    dataPresenter.InvalidateView();
            });
            return new ScalarBinding<T>(onRefresh: v =>
            {
                var selectedCount = dataPresenter.SelectedRows.Count;
                if (selectedCount == 0)
                    v.IsChecked = false;
                else if (selectedCount == dataPresenter.Rows.Count - (dataPresenter.VirtualRow != null ? 1 : 0))
                    v.IsChecked = true;
                else
                    v.IsChecked = null;
            },
            onSetup: v =>
            {
                trigger.Attach(v);
            },
            onCleanup: v =>
            {
                trigger.Detach(v);
            });
        }
    }
}
