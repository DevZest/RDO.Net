using DevZest.Data.Windows.Primitives;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public static class UIElementExtensions
    {
        internal static DataSetManager GetDataSetManager(this UIElement uiElement)
        {
            var dataSetView = uiElement.GetParent<DataSetView>();
            return dataSetView == null ? null : dataSetView.Manager;
        }

        internal static DataRowManager GetDataRowManager(this UIElement uiElement)
        {
            var dataRowView = uiElement.GetParent<DataRowView>();
            return dataRowView == null ? null : dataRowView.Manager;
        }

        private static T GetParent<T>(this UIElement uiElement)
            where T : UIElement
        {
            Debug.Assert(uiElement != null);

            for (DependencyObject parent = VisualTreeHelper.GetParent(uiElement); parent != null; parent = VisualTreeHelper.GetParent(parent))
            {
                var result = parent as T;
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
