using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Wpf
{
    internal static class UIElementExtensions
    {
        private static ConditionalWeakTable<UIElement, ViewItem> s_uiElements = new ConditionalWeakTable<UIElement, ViewItem>();

        internal static ViewItem GetViewItem(this UIElement uiElement)
        {
            Debug.Assert(uiElement != null);
            ViewItem result;
            return s_uiElements.TryGetValue(uiElement, out result) ? result : null;
        }

        internal static T SetViewItem<T>(this T uiElement, ViewItem value)
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
