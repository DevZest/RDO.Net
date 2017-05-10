using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest
{
    static partial class DependencyObjectExtensions
    {
        internal static T FindAncestor<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            Debug.Assert(obj != null);
            var parent = VisualTreeHelper.GetParent(obj);
            if (parent == null) return null;

            T parentT = parent as T;
            return parentT ?? parent.FindAncestor<T>();
        }
    }
}
