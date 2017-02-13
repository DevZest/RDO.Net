using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public static class BindingFactory
    {
        public static RowBinding<ValidationView> ValidationView<T>(this RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (rowInput == null)
                throw new ArgumentNullException(nameof(rowInput));

            var result = new RowBinding<ValidationView>((e, r) =>
            {
                e.Errors = rowInput.GetErrors(null, r);
                e.Warnings = rowInput.GetWarnings(r);

            });
            return result;
        }
    }
}
