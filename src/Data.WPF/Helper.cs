using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public static class Helper
    {
        internal static DataPresenter GetDataPresenter(this DependencyObject reference)
        {
            var dataForm = reference.GetParent<DataForm>();
            return dataForm == null ? null : dataForm.Presenter;
        }

        private static T GetParent<T>(this DependencyObject reference)
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
