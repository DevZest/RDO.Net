using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public sealed class DataRowClient : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DataRowControl();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ((DataRowControl)element).View = (DataRowManager)item;
        }
    }
}
