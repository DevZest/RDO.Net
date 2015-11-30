using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public static class UIElementExtensions
    {
        internal static DataSetView GetDataSetView(this UIElement uiElement)
        {
            var dataSetGrid = uiElement.GetParent<DataSetGrid>();
            return dataSetGrid == null ? null : dataSetGrid.View;
        }

        internal static DataRowView GetDataRowView(this UIElement uiElement)
        {
            var dataRowGrid = uiElement.GetParent<DataRowGrid>();
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
