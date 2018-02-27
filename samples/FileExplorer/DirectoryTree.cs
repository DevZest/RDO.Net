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
            EnsureSubDirectoriesLoaded(rowPresenter);
            base.ToggleExpandState(rowPresenter);
        }

        private static void EnsureSubDirectoriesLoaded(RowPresenter rowPresenter)
        {
            var dataRow = rowPresenter.DataRow;
            DirectoryTreeItem.Expand(dataRow);
        }

        public void OnSubDirectoryRenamed(string oldPath, string newPath)
        {
            var children = CurrentRow.Children;
            foreach (var child in children)
            {
                if (child.GetValue(_.Path) == oldPath)
                {
                    var dataRow = child.DataRow;
                    var _ = (DirectoryTreeItem)dataRow.Model;
                    _.Path[child.DataRow] = newPath;
                    return;
                }
            }
        }

        public void OnSubDirectorySelected(string path)
        {
            EnsureSubDirectoriesLoaded(CurrentRow);
            if (!CurrentRow.IsExpanded)
                CurrentRow.ToggleExpandState();
            var children = CurrentRow.Children;
            foreach (var child in children)
            {
                if (child.GetValue(_.Path) == path)
                {
                    Select(child, SelectionMode.Single);
                    return;
                }
            }
        }
    }
}
