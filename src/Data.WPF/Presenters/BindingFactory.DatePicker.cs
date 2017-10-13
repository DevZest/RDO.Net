using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    static partial class BindingFactory
    {
        public static RowBinding<DatePicker> AsDatePicker(this _DateTime column)
        {
            return new RowBinding<DatePicker>(onRefresh: (v, p) => v.SelectedDate = p.GetValue(column))
                .WithInput(new PropertyChangedTrigger<DatePicker>(DatePicker.SelectedDateProperty), column, v => v.SelectedDate);
        }
    }
}
