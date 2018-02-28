using DevZest.Data;
using DevZest.Data.Presenters;
using System;

namespace FileExplorer
{
    public abstract class DirectoryPresenter<T> : DataPresenter<T>, IDisposable
        where T : Model, new()
    {
        protected DirectoryPresenter(DirectoryTree directoryTree)
        {
            _directoryTree = directoryTree;
            _directoryTree.ViewInvalidated += OnDirectoryTreeViewInvalidated;
        }

        private readonly DirectoryTree _directoryTree;
        protected DirectoryTree DirectoryTree
        {
            get { return _directoryTree; }
        }

        protected abstract string CurrentDirectory { get; set; }

        private void OnDirectoryTreeViewInvalidated(object sender, EventArgs e)
        {
            CurrentDirectory = _directoryTree.CurrentPath;
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
