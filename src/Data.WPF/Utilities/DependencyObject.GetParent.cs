using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest
{
    static partial class Extension
    {
        public static T GetParent<T>(this DependencyObject reference)
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
