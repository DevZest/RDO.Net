using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows.Utilities
{
    static partial class Extension
    {
        public static bool Contains(this DependencyObject depObj, DependencyObject element)
        {
            for (; element != null; element = VisualTreeHelper.GetParent(element))
            {
                if (element == depObj)
                    return true;
            }
            return false;
        }
    }
}
