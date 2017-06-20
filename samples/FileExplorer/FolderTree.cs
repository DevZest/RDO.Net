using DevZest.Data.Presenters;
using System.Windows.Controls;

namespace FileExplorer
{
    public class FolderTree : DataPresenter<Folder>
    {
        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .MakeRecursive(_.SubFolders)
                .GridColumns("Auto")
                .GridRows("Auto")
                .Layout(Orientation.Vertical)
                .WithSelectionMode(SelectionMode.Single)
                .AddBinding(0, 0, _.AsFolderView());
        }

        protected override void ToggleExpandState(RowPresenter rowPresenter)
        {
            var dataRow = rowPresenter.DataRow;
            Folder.Expand(dataRow);
            base.ToggleExpandState(rowPresenter);
        }
    }
}
