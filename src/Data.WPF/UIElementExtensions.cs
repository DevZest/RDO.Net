using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public static class UIElementExtensions
    {
        internal static DataSetManager GetDataSetManager(this UIElement uiElement)
        {
            var dataSetControl = uiElement.GetParent<DataSetControl>();
            return dataSetControl == null ? null : dataSetControl.Manager;
        }

        internal static DataRowManager GetDataRowManager(this UIElement uiElement)
        {
            var dataRowGrid = uiElement.GetParent<DataRowControl>();
            return dataRowGrid == null ? null : dataRowGrid.View;
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
