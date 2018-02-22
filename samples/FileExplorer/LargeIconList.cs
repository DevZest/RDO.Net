using DevZest.Data.Presenters;
using System.Windows.Controls;

namespace FileExplorer
{
    public class LargeIconList : FolderContentPresenter<LargeIconListItem>
    {
        protected override void OverrideBuildTemplate(TemplateBuilder builder)
        {
            builder
                .GridColumns("85", "5")
                .GridRows("Auto", "5")
                .Layout(Orientation.Vertical, 0)
                .WithSelectionMode(SelectionMode.Extended)
                .AddBinding(0, 0, _.BindToLargeIconView());
        }
    }
}
