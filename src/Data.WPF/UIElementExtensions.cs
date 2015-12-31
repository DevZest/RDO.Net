using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public static class UIElementExtensions
    {
        private static readonly DependencyProperty GridEntryProperty = DependencyProperty.RegisterAttached(nameof(GridEntry),
            typeof(GridEntry), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static GridEntry GetGridEntry(this UIElement element)
        {
            return (GridEntry)element.GetValue(GridEntryProperty);
        }

        internal static void SetGridEntry(this UIElement element, GridEntry value)
        {
            Debug.Assert(value != null && element.GetGridEntry() == null);
            element.SetValue(GridEntryProperty, value);
        }

        private static readonly DependencyProperty DataRowPresenterProperty = DependencyProperty.RegisterAttached(nameof(DataRowPresenter),
            typeof(DataRowPresenter), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static DataRowPresenter GetDataRowPresenter(this UIElement element)
        {
            return (DataRowPresenter)element.GetValue(DataRowPresenterProperty);
        }

        internal static void SetDataRowPresenter(this UIElement element, DataRowPresenter value)
        {
            if (value == null)
            {
                Debug.Assert(element.GetDataRowPresenter() != null);
                element.ClearValue(DataRowPresenterProperty);
            }
            else
            {
                Debug.Assert(element.GetDataRowPresenter() == null);
                element.SetValue(DataRowPresenterProperty, value);
            }
        }

        public static T GetColumnValue<T>(this UIElement element, Column<T> column)
        {
            var dataRowPresenter = element.GetDataRowPresenter();
            if (dataRowPresenter == null || dataRowPresenter.DataRow == null)
                return default(T);
            return column[dataRowPresenter.DataRow];
        }

        public static void SetColumnValue<T>(this UIElement element, Column<T> column, T value)
        {
            var dataRowPresenter = element.GetDataRowPresenter();
            if (dataRowPresenter == null || dataRowPresenter.DataRow == null)
                return;
            column[dataRowPresenter.DataRow] = value;
        }
    }
}
