using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DevZest.Windows
{
    static partial class DependencyObjectExtensions
    {
        internal static T FindVisaulAncestor<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            if (obj == null)
                return null;

            // If obj is not Visual nor Visual3D, the next call to VisualTreeHelper.GetParent will throw exception
            // Seems TreeView has this problem too, and not fixed till now (2018):
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/5982cafe-f75b-42b4-99dc-50d3a81b30b0/invalidoperationexception-systemwindowsdocumentshyperlink-is-not-a-visual-or-visual3d?forum=wpf
            if (!(obj is Visual) && !(obj is Visual3D))
                return null;

            var parent = VisualTreeHelper.GetParent(obj);
            if (parent == null) return null;

            T parentT = parent as T;
            return parentT ?? parent.FindVisaulAncestor<T>();
        }
    }
}
