using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class DataRowListView : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DataRowView();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ((DataRowView)element).Manager = (DataRowManager)item;
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            ((DataRowView)element).Manager = null;
        }
    }
}
