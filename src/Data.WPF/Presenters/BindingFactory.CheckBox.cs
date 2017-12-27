using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static ScalarBinding<CheckBox> BindToCheckBoxToSelectAll(this DataPresenter dataPresenter)
        {
            return ToSelectAll<CheckBox>(dataPresenter);
        }

        public static RowBinding<CheckBox> BindToCheckBoxToSelectRow(this Model model)
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

        public static RowBinding<CheckBox> BindToCheckBox(this _Boolean column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            return new RowBinding<CheckBox>(onRefresh: (v, p) => v.IsChecked = p.GetValue(column))
                .WithInput(CheckBox.IsCheckedProperty, column, v => v.IsChecked);
        }
    }
}
