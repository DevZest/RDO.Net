using System.Collections.Generic;
using System.Windows;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Input;
using System;
using System.IO;
using System.Threading;

namespace FileExplorer
{
    public interface IDirectoryList : IDataPresenter, IDisposable
    {
        DirectoryListMode Mode { get; }
    }

    public abstract class DirectoryList<T> : DataPresenter<T>, IDirectoryList, InPlaceEditor.ICommandService, InPlaceEditor.ISwitcher
        where T : DirectoryItem, new()
    {
        protected DirectoryList(DataView directoryListView, DirectoryTree directoryTree)
        {
            _directoryTree = directoryTree;
            _currentDirectory = GetCurrentDirectory();
            Refresh(directoryListView);
            _directoryTree.ViewInvalidated += OnDirectoryTreeViewInvalidated;
        }

        private readonly DirectoryTree _directoryTree;
        protected DirectoryTree DirectoryTree
        {
            get { return _directoryTree; }
        }

        private string _currentDirectory;
        private string CurrentDirectory
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

        private void OnDirectoryTreeViewInvalidated(object sender, EventArgs e)
        {
            CurrentDirectory = GetCurrentDirectory();
        }

        private string GetCurrentDirectory()
        {
            var currentRow = DirectoryTree.CurrentRow;
            return currentRow == null ? null : currentRow.GetValue(DirectoryTree._.Path);
        }

        private void Refresh()
        {
            Refresh(View);
        }

        private void Refresh(DataView directoryListView)
        {
            ShowOrRefreshAsync(directoryListView, (CancellationToken ct) => DirectoryItem.GetDirectoryItemsAsync<T>(CurrentDirectory, ct));
        }

        protected sealed override void BuildTemplate(TemplateBuilder builder)
        {
            builder.WithRowViewBeginEditGestures(new KeyGesture(Key.F2))
                .WithRowViewCancelEditGestures(new KeyGesture(Key.Escape))
                .WithRowViewEndEditGestures(new KeyGesture(Key.Enter));
            OverrideBuildTemplate(builder);
        }

        protected abstract void OverrideBuildTemplate(TemplateBuilder builder);

        public abstract DirectoryListMode Mode { get; }

        protected override bool ConfirmEndEdit()
        {
            var type = CurrentRow.GetValue(_.Type);
            var path = CurrentRow.GetValue(_.Path);
            var displayName = CurrentRow.GetValue(_.DisplayName);
            var caption = type == DirectoryItemType.Directory ? "Rename Directory" : "Rename File";
            var directoryOrFile = type == DirectoryItemType.Directory ? "directory" : "file";
            var message = string.Format("Are you sure you want to rename the {0}?\nWARNING: This will ACTUALLY rename the {0}!!!", directoryOrFile);
            if (MessageBox.Show(message, caption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                return Rename(type, path, displayName);
            return false;
        }

        private bool Rename(DirectoryItemType type, string path, string newName)
        {
            try
            {
                var newPath = Path.Combine(Path.GetDirectoryName(path), newName);
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

        bool InPlaceEditor.ISwitcher.AffectsIsEditing(InPlaceEditor inPlaceEditor, DependencyProperty dp)
        {
            return dp == InPlaceEditor.IsRowEditingProperty;
        }

        bool InPlaceEditor.ISwitcher.GetIsEditing(InPlaceEditor inPlaceEditor)
        {
            return inPlaceEditor.IsRowEditing;
        }

        bool InPlaceEditor.ISwitcher.ShouldFocusToEditorElement(InPlaceEditor inPlaceEditor)
        {
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DirectoryTree.ViewInvalidated -= OnDirectoryTreeViewInvalidated;
                    DetachView();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
