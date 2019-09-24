using System.Collections.Generic;
using System.Windows;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Input;
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace FileExplorer
{
    public interface IDirectoryListPresenter : IDataPresenter, IDisposable
    {
        DirectoryListMode Mode { get; }
    }

    public static class DirectoryListCommands
    {
        public static RoutedUICommand Refresh { get { return NavigationCommands.Refresh; } }
        public static RoutedUICommand NewFolder { get { return ApplicationCommands.New; } }
        public static readonly RoutedUICommand Start = new RoutedUICommand();
    }

    public abstract class DirectoryListPresenter<T> : DirectoryPresenter<T>, IDirectoryListPresenter,
        InPlaceEditor.ICommandService,
        RowSelector.ICommandService,
        DataView.ICommandService
        where T : DirectoryItem, new()
    {
        protected DirectoryListPresenter(DataView directoryListView, DirectoryTreePresenter directoryTree)
            : base(directoryTree)
        {
            _currentDirectory = DirectoryTreePresenter.CurrentPath;
            Refresh(directoryListView);
        }

        private string _currentDirectory;
        protected sealed override string CurrentDirectory
        {
            get { return _currentDirectory; }
            set
            {
                if (_currentDirectory == value)
                    return;

                _currentDirectory = value;
                Refresh();
            }
        }

        private Task Refresh()
        {
            return Refresh(View);
        }

        private Task Refresh(DataView directoryListView)
        {
            return ShowOrRefreshAsync(directoryListView, (CancellationToken ct) => DirectoryItem.GetDirectoryItemsAsync<T>(CurrentDirectory, ct));
        }

        public abstract DirectoryListMode Mode { get; }

        protected override bool ConfirmEndEdit()
        {
            var type = CurrentRow.GetValue(_.Type);
            var caption = type == DirectoryItemType.Directory ? "Rename Directory" : "Rename File";
            var directoryOrFile = type == DirectoryItemType.Directory ? "directory" : "file";
            var message = string.Format("Are you sure you want to rename the {0}?\nWARNING: This will ACTUALLY rename the {0}!!!", directoryOrFile);
            if (MessageBox.Show(message, caption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                return RenameCurrent();
            return false;
        }

        private bool RenameCurrent()
        {
            var type = CurrentRow.GetValue(_.Type);
            var path = CurrentRow.GetValue(_.Path);
            var displayName = CurrentRow.GetValue(_.DisplayName);

            var newPath = Path.Combine(Path.GetDirectoryName(path), displayName);
            if (!PerformRename(type, path, newPath))
                return false;

            CurrentRow.EditValue(_.Path, newPath);
            if (type == DirectoryItemType.Directory)
                DirectoryTreePresenter.OnSubDirectoryRenamed(path, newPath);
            return true;
        }

        private bool PerformRename(DirectoryItemType type, string path, string newPath)
        {
            try
            {
                if (type == DirectoryItemType.Directory)
                    Directory.Move(path, newPath);
                else
                    File.Move(path, newPath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        IEnumerable<CommandEntry> InPlaceEditor.ICommandService.GetCommandEntries(InPlaceEditor inPlaceEditor)
        {
            yield return RowView.Commands.BeginEdit.Bind(BeginEdit, CanBeginEdit, new MouseGesture(MouseAction.LeftClick));
        }

        private void CanBeginEdit(object sender, CanExecuteRoutedEventArgs e)
        {
            var rowView = RowView.GetCurrent((InPlaceEditor)sender);
            var rowPresenter = rowView.RowPresenter;
            e.CanExecute = rowPresenter.IsCurrent && !rowPresenter.IsEditing;
            if (!e.CanExecute)
                e.ContinueRouting = true;
        }

        private void BeginEdit(object sender, ExecutedRoutedEventArgs e)
        {
            var rowView = RowView.GetCurrent((InPlaceEditor)sender);
            var rowPresenter = rowView.RowPresenter;
            rowPresenter.BeginEdit();
        }

        IEnumerable<CommandEntry> DataView.ICommandService.GetCommandEntries(DataView dataView)
        {
            var baseService = this.GetRegisteredService<DataView.ICommandService>();
            foreach (var entry in baseService.GetCommandEntries(dataView))
                yield return entry;
            yield return DirectoryListCommands.Refresh.Bind(ExecRefresh);
            yield return DirectoryListCommands.NewFolder.Bind(ExecNewFolder, CanExecNewFolder);
        }

        private async void ExecRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            var currentPath = CurrentPath;
            await Refresh();
            SelectPath(currentPath);
        }

        private void CanExecNewFolder(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsEditing && !string.IsNullOrEmpty(CurrentDirectory);
        }

        private void ExecNewFolder(object sender, ExecutedRoutedEventArgs e)
        {
            var fullPath = GetNewFolder();
            Directory.CreateDirectory(fullPath);
            var dataRow = DataSet.AddRow((_, x) => _.Initialize(x, new DirectoryInfo(fullPath)));
            var rowPresenter = this[dataRow];
            CurrentRow = rowPresenter;
            CurrentRow.View.Focus();
            CurrentRow.BeginEdit();
            e.Handled = true;
        }

        private string GetNewFolder()
        {
            var baseFolderName = "New Folder";
            var suffix = 0;
            string folderName, fullPath;
            folderName = baseFolderName;
            do
            {
                fullPath = Path.Combine(CurrentDirectory, folderName);
                if (!Directory.Exists(fullPath))
                    return fullPath;
                suffix++;
                folderName = baseFolderName + suffix.ToString();
            }
            while (true);
        }

        private string CurrentPath
        {
            get { return CurrentRow?.GetValue(_.Path); }
        }

        private void SelectPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            foreach (var row in Rows)
            {
                if (row.GetValue(_.Path) == path)
                {
                    Select(row, SelectionMode.Single);
                    return;
                }
            }
        }

        IEnumerable<CommandEntry> RowSelector.ICommandService.GetCommandEntries(RowSelector rowSelector)
        {
            var baseService = this.GetRegisteredService<RowSelector.ICommandService>();
            foreach (var entry in baseService.GetCommandEntries(rowSelector))
                yield return entry;
            yield return DirectoryListCommands.Start.Bind(ExecStart, CanExecStart, new MouseGesture(MouseAction.LeftDoubleClick), new KeyGesture(Key.Enter));
        }

        private void CanExecStart(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !CurrentRow.IsEditing;
            if (!e.CanExecute)
                e.ContinueRouting = true;
        }

        private void ExecStart(object sender, ExecutedRoutedEventArgs e)
        {
            StartCurrent();
        }

        private void StartCurrent()
        {
            var type = CurrentRow.GetValue(_.Type);
            var path = CurrentRow.GetValue(_.Path);
            if (type == DirectoryItemType.Directory)
                DirectoryTreePresenter.SelectSubDirectory(path);
            else
                ProcessStart(path);
        }

        private void ProcessStart(string fileName)
        {
            try
            {
                Process.Start(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
