using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class DataRowListView : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DataRowControl();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var dataRowControl = element as DataRowControl;
            var dataRowManager = item as DataRowManager;
            if (dataRowControl != null && dataRowManager != null)
                dataRowControl.DataRowManager = dataRowManager;
            else
                base.PrepareContainerForItemOverride(element, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var dataRowControl = element as DataRowControl;
            var dataRowManager = item as DataRowManager;
            if (dataRowControl != null && dataRowManager != null)
                dataRowControl.DataRowManager = null;
            else
                base.ClearContainerForItemOverride(element, item);
        }
    }
}
