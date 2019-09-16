using System;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    static partial class BindingFactory
    {
        /// <summary>
        /// Binds a nullable DateTime column to <see cref="DatePicker"/>.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<DatePicker> BindToDatePicker(this Column<DateTime?> source)
        {
            return new RowBinding<DatePicker>(onRefresh: (v, p) => v.SelectedDate = p.GetValue(source))
                .WithInput(DatePicker.SelectedDateProperty, source, v => v.SelectedDate);
        }

        /// <summary>
        /// Binds a nullable DateTime scalar data to <see cref="DatePicker"/>.
        /// </summary>
        /// <param name="source">The source scalar data.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<DatePicker> BindToDatePicker(this Scalar<DateTime?> source)
        {
            return new ScalarBinding<DatePicker>(onRefresh: (v, p) => v.SelectedDate = source.GetValue())
                .WithInput(DatePicker.SelectedDateProperty, source, v => v.SelectedDate);
        }
    }
}
