using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public static class Helper
    {
        internal static DataSetPresenter GetDataSetPresenter(this DependencyObject reference)
        {
            var dataSetView = reference.GetParent<DataSetView>();
            return dataSetView == null ? null : dataSetView.Presenter;
        }

        internal static DataRowPresenter GetDataRowPresenter(this DependencyObject reference)
        {
            var dataRowView = reference.GetParent<DataRowView>();
            return dataRowView == null ? null : dataRowView.Presenter;
        }

        private static T GetParent<T>(this DependencyObject reference)
            where T : UIElement
        {
            Debug.Assert(reference != null);

            for (DependencyObject parent = VisualTreeHelper.GetParent(reference); parent != null; parent = VisualTreeHelper.GetParent(parent))
            {
                var result = parent as T;
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
