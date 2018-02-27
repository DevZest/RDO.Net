using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System;

namespace FileExplorer
{
    public static class DirectoryTreeCommands
    {
        public static RoutedUICommand Refresh
        {
            get { return NavigationCommands.Refresh; }
        }
    }

    public class DirectoryTree : DataPresenter<DirectoryTreeItem>, DataView.ICommandService
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

        public void SelectSubDirectory(string path)
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

        IEnumerable<CommandEntry> DataView.ICommandService.GetCommandEntries(DataView dataView)
        {
            var baseService = ServiceManager.GetService<DataView.ICommandService>(this);
            foreach (var entry in baseService.GetCommandEntries(dataView))
                yield return entry;
            yield return DirectoryTreeCommands.Refresh.Bind(ExecRefresh);
        }

        private void ExecRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            var currentPath = GetCurrentPath();
            var dataSet = DirectoryTreeItem.GetLogicalDrives();
            Refresh(dataSet);
            SelectCurrentPath(currentPath);
        }

        private IReadOnlyList<string> GetCurrentPath()
        {
            if (CurrentRow == null)
                return null;

            var result = new List<string>();
            for (var row = CurrentRow; row != null; row = row.Parent)
                result.Insert(0, row.GetValue(_.Path));

            return result;
        }

        private void SelectCurrentPath(IReadOnlyList<string> currentPath)
        {
            var currentRow = FindRow(Rows, currentPath, 0);
            if (currentRow != null)
                Select(currentRow, SelectionMode.Single);
        }

        private RowPresenter FindRow(IReadOnlyList<RowPresenter> rows, IReadOnlyList<string> currentPath, int level)
        {
            if (currentPath == null)
                return null;

            foreach (var row in rows)
            {
                if (row.GetValue(_.Path) == currentPath[level])
                {
                    if (level == currentPath.Count - 1)
                        return row;
                    EnsureSubDirectoriesLoaded(row);
                    row.ToggleExpandState();
                    return FindRow(row.Children, currentPath, level + 1);
                }
            }

            return null;
        }
    }
}
