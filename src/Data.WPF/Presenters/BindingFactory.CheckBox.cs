using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static ScalarBinding<CheckBox> AsSelectionCheckBox(this DataPresenter dataPresenter)
        {
            if (dataPresenter == null)
                throw new ArgumentNullException(nameof(dataPresenter));

            var trigger = new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty).WithExecuteAction(v =>
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
            return new ScalarBinding<CheckBox>(onRefresh: v =>
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

        public static RowBinding<CheckBox> AsSelectionCheckBox(this Model model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var trigger = new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty).WithExecuteAction(v =>
            {
                var binding = (TwoWayBinding)v.GetBinding();
                if (binding.IsRefreshing)
                    return;
                var isChecked = v.IsChecked;
                if (!isChecked.HasValue)
                    return;

                var isSelected = isChecked.GetValueOrDefault();
                var row = v.GetRowPresenter();
                row.IsSelected = isSelected;
            });

            return new RowBinding<CheckBox>(
                onRefresh: (v, p) => v.IsChecked = p.IsSelected,
                onSetup: (v, p) => trigger.Attach(v),
                onCleanup: (v, p) => trigger.Detach(v));
        }
    }
}
