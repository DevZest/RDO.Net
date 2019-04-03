using DevZest.Data;
using DevZest.Data.Presenters;
using System;

namespace FileExplorer
{
    public abstract class DirectoryPresenter<T> : DataPresenter<T>, IDisposable
        where T : Model, new()
    {
        protected DirectoryPresenter(DirectoryTreePresenter directoryTree)
        {
            _directoryTreePresenter = directoryTree;
            _directoryTreePresenter.ViewInvalidated += OnDirectoryTreeViewInvalidated;
        }

        private readonly DirectoryTreePresenter _directoryTreePresenter;
        protected DirectoryTreePresenter DirectoryTreePresenter
        {
            get { return _directoryTreePresenter; }
        }

        protected abstract string CurrentDirectory { get; set; }

        private void OnDirectoryTreeViewInvalidated(object sender, EventArgs e)
        {
            CurrentDirectory = _directoryTreePresenter.CurrentPath;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DirectoryTreePresenter.ViewInvalidated -= OnDirectoryTreeViewInvalidated;
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
