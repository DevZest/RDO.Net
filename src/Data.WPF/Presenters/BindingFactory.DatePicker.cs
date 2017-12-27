using System;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    static partial class BindingFactory
    {
        public static RowBinding<DatePicker> BindToDatePicker(this Column<DateTime?> column)
        {
            return new RowBinding<DatePicker>(onRefresh: (v, p) => v.SelectedDate = p.GetValue(column))
                .WithInput(DatePicker.SelectedDateProperty, column, v => v.SelectedDate);
        }
    }
}
