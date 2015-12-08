using System.Windows;

namespace DevZest.Data.Windows
{
    internal interface IListGridItem
    {
        void UpdateTarget(DataRowView dataRowView, UIElement uiElement);

        void UpdateSource(UIElement uiElement, DataRowView dataRowView);
    }
}
