using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    internal static class UIElementExtensions
    {
        private static ConditionalWeakTable<UIElement, GridItem> s_uiElements = new ConditionalWeakTable<UIElement, GridItem>();

        internal static GridItem GetViewItem(this UIElement uiElement)
        {
            Debug.Assert(uiElement != null);
            GridItem result;
            return s_uiElements.TryGetValue(uiElement, out result) ? result : null;
        }

        internal static T SetViewItem<T>(this T uiElement, GridItem value)
            where T : UIElement
        {
            Debug.Assert(uiElement != null);
            if (value == null)
                s_uiElements.Remove(uiElement);
            else
                s_uiElements.Add(uiElement, value);

            return uiElement;
        }


        internal static T GetParent<T>(this UIElement uiElement)
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
