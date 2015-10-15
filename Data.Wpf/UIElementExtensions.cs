using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Wpf
{
    internal static class UIElementExtensions
    {
        private static ConditionalWeakTable<UIElement, ViewManager> s_uiElements = new ConditionalWeakTable<UIElement, ViewManager>();

        internal static ViewManager GetViewManager(this UIElement uiElement)
        {
            Debug.Assert(uiElement != null);
            ViewManager result;
            return s_uiElements.TryGetValue(uiElement, out result) ? result : null;
        }

        internal static T SetViewManager<T>(this T uiElement, ViewManager value)
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
