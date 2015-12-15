using DevZest.Data.Windows.Primitives;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public static class Helper
    {
        internal static DataSetManager GetDataSetManager(this DependencyObject reference)
        {
            var dataSetControl = reference.GetParent<DataSetControl>();
            return dataSetControl == null ? null : dataSetControl.DataSetManager;
        }

        internal static DataRowManager GetDataRowManager(this DependencyObject reference)
        {
            var dataRowControl = reference.GetParent<DataRowControl>();
            return dataRowControl == null ? null : dataRowControl.DataRowManager;
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
