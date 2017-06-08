using DevZest.Windows;
using System.Windows.Controls;

namespace FileExplorer
{
    public class LargeIconList : DataPresenter<LargeIconListItem>
    {
        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .GridColumns("85", "5")
                .GridRows("Auto", "5")
                .Layout(Orientation.Vertical, 0)
                .WithSelectionMode(SelectionMode.Single)
                .AddBinding(0, 0, _.AsLargeIconView());
        }
    }
}
