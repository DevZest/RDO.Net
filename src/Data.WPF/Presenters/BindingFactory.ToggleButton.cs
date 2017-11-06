using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DevZest.Data.Presenters
{
    static partial class BindingFactory
    {
        public static ScalarBinding<ToggleButton> AsToggleButtonToSelectAll(this DataPresenter dataPresenter)
        {
            return ToSelectAll<ToggleButton>(dataPresenter);
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
                for (int i = 0; i < rows.Count; i++)
                    rows[i].IsSelected = isSelected;
            });
            return new ScalarBinding<T>(onRefresh: v =>
            {
                var selectedCount = dataPresenter.SelectedRows.Count;
                if (selectedCount == 0)
                    v.IsChecked = false;
                else if (selectedCount == dataPresenter.Rows.Count)
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
