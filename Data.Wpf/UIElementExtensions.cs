using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DevZest.Data.Windows
{
    internal static class UIElementExtensions
    {
        private static ConditionalWeakTable<UIElement, DataSetView> s_dataSetViews = new ConditionalWeakTable<UIElement, DataSetView>();

        public static DataSetView GetDataSetView(this UIElement uiElement)
        {
            DataSetView result;
            return s_dataSetViews.TryGetValue(uiElement, out result) ? result : null;
        }

        internal static void SetDataSetView(this UIElement uiElement, DataSetView value)
        {
            Debug.Assert(uiElement != null);
            if (value == null)
                s_dataSetViews.Remove(uiElement);
            else
                s_dataSetViews.Add(uiElement, value);
        }

        private static ConditionalWeakTable<UIElement, DataRowView> s_dataRowViews = new ConditionalWeakTable<UIElement, DataRowView>();
        public static DataRowView GetDataRowView(this UIElement uiElement)
        {
            DataRowView result;
            return s_dataRowViews.TryGetValue(uiElement, out result) ? result : null;
        }

        internal static void SetDataRowView(this UIElement uiElement, DataRowView value)
        {
            Debug.Assert(uiElement != null);
            if (value == null)
                s_dataRowViews.Remove(uiElement);
            else
                s_dataRowViews.Add(uiElement, value);
        }

        //internal static T GetParent<T>(this UIElement uiElement)
        //    where T : UIElement
        //{
        //    Debug.Assert(uiElement != null);

        //    for (DependencyObject parent = VisualTreeHelper.GetParent(uiElement); parent != null; parent = VisualTreeHelper.GetParent(parent))
        //    {
        //        var result = parent as T;
        //        if (result != null)
        //            return result;
        //    }
        //    return null;
        //}
    }
}
