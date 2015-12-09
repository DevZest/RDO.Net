using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class DataRowListView : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DataRowView();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var dataRowView = element as DataRowView;
            var dataRowManager = item as DataRowManager;
            if (dataRowView != null && dataRowManager != null)
                dataRowView.Manager = dataRowManager;
            else
                base.PrepareContainerForItemOverride(element, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var dataRowView = element as DataRowView;
            var dataRowManager = item as DataRowManager;
            if (dataRowView != null && dataRowManager != null)
                dataRowView.Manager = null;
            else
                base.ClearContainerForItemOverride(element, item);
        }
    }
}
