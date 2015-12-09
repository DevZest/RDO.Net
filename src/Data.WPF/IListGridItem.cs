using System.Windows;

namespace DevZest.Data.Windows
{
    internal interface IListGridItem
    {
        void UpdateTarget(DataRowManager dataRowManager, UIElement uiElement);

        void UpdateSource(UIElement uiElement, DataRowManager dataRowManager);
    }
}
