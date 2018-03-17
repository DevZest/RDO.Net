using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using System.IO;

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
            var currentPath = CurrentPath;
            var dataSet = DirectoryTreeItem.GetLogicalDrives();
            Refresh(dataSet);
            Select(currentPath);
        }

        public void Select(string path)
        {
            Select(ParsePath(path));
        }

        private static IReadOnlyList<string> ParsePath(string path)
        {
            var result = new List<string>();
            for (; path != null; path = Directory.GetParent(path)?.FullName)
                result.Insert(0, path);

            return result;
        }

        private void Select(IReadOnlyList<string> paths)
        {
            var currentRow = FindRow(Rows, paths, 0);
            if (currentRow != null)
                Select(currentRow, SelectionMode.Single);
        }

        private RowPresenter FindRow(IReadOnlyList<RowPresenter> rows, IReadOnlyList<string> paths, int level)
        {
            if (paths == null || level >= paths.Count)
                return null;

            foreach (var row in rows)
            {
                if (row.GetValue(_.Path).ToLower() == paths[level].ToLower())
                {
                    EnsureSubDirectoriesLoaded(row);
                    if (!row.IsExpanded)
                        row.ToggleExpandState();
                    return FindRow(row.Children, paths, level + 1) ?? row;
                }
            }

            return null;
        }

        public string CurrentPath
        {
            get { return CurrentRow == null ? null : CurrentRow.GetValue(_.Path); }
        }
    }
}
