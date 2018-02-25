using DevZest.Data.Presenters;
using System.Windows.Controls;

namespace FileExplorer
{
    public class DirectoryTree : DataPresenter<DirectoryTreeItem>
    {
        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .MakeRecursive(_.SubFolders)
                .GridColumns("Auto")
                .GridRows("Auto")
                .Layout(Orientation.Vertical)
                .WithSelectionMode(SelectionMode.Single)
                .AddBinding(0, 0, _.BindToDirectoryTreeItemView());
        }

        protected override void ToggleExpandState(RowPresenter rowPresenter)
        {
            var dataRow = rowPresenter.DataRow;
            DirectoryTreeItem.Expand(dataRow);
            base.ToggleExpandState(rowPresenter);
        }
    }
}
