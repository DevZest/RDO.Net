using DevZest.Data;
using DevZest.Windows.Controls;
using DevZest.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Windows
{
    public abstract class DataPresenter<T> : DataPresenter
        where T : Model, new()
    {
        public void Show(DataView dataView, DataSet<T> dataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                Show(dataView, dataSet, null, null);
            else
                Show(dataView, dataSet, Where, OrderBy);
        }

        public void Show(DataView dataView, DataSet<T> dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            if (dataView == null)
                throw new ArgumentNullException(nameof(dataView));
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            if (dataView.DataPresenter != null && dataView.DataPresenter != this)
                throw new ArgumentException(Strings.DataPresenter_InvalidDataView, nameof(dataView));

            if (_dataLoader != null)
                _dataLoader.Reset();
            AttachView(dataView);
            Mount(dataView, dataSet, where, orderBy);
        }

        private void Mount(DataView dataView, DataSet<T> dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            dataSet._.EnsureInitialized();
            DataSet = dataSet;
            var template = new Template();
            using (var builder = new TemplateBuilder(template, DataSet.Model))
            {
                BuildTemplate(builder);
            }
            _layoutManager = LayoutManager.Create(this, template, dataSet, where, orderBy);
            dataView.OnDataLoaded();
        }

        public void ShowAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                ShowAsync(dataView, getDataSet, null, null);
            else
                ShowAsync(dataView, getDataSet, _ => Where, _ => OrderBy);
        }

        public void ShowAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (dataView == null)
                throw new ArgumentNullException(nameof(dataView));
            if (getDataSet == null)
                throw new ArgumentNullException(nameof(getDataSet));

            if (_dataLoader == null)
                _dataLoader = new DataLoader(this);
            _dataLoader.ShowAsync(dataView, getDataSet, getWhere, getOrderBy);
        }

        public void ShowAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                ShowAsync(dataView, getDataSet, null, null);
            else
                ShowAsync(dataView, getDataSet, _ => Where, _ => OrderBy);
        }

        public void ShowAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (dataView == null)
                throw new ArgumentNullException(nameof(dataView));
            if (getDataSet == null)
                throw new ArgumentNullException(nameof(getDataSet));

            if (_dataLoader == null)
                _dataLoader = new DataLoader(this);
            _dataLoader.ShowAsync(dataView, getDataSet, getWhere, getOrderBy);
        }

        private sealed class DataLoader
        {
            public DataLoader(DataPresenter<T> dataPresenter)
            {
                Debug.Assert(dataPresenter != null);
                _dataPresenter = dataPresenter;
            }

            private readonly DataPresenter<T> _dataPresenter;
            private int _revision;
            private Func<CancellationToken, Task<DataSet<T>>> _getDataSet;
            private bool _cancellable;
            private Func<T, Predicate<DataRow>> _getWhere;
            private Func<T, IComparer<DataRow>> _getOrderBy;
            private Task _runningTask;
            private CancellationTokenSource _cts;

            public DataView DataView
            {
                get { return _dataPresenter.View; }
            }

            public void Reset()
            {
                if (_cts != null)
                    _cts.Cancel();

                _getDataSet = null;
                _getWhere = null;
                _getOrderBy = null;

                DataView.ResetDataLoadState();
                if (_runningTask == null)
                    Dispose();
            }

            private void Dispose()
            {
                _dataPresenter._dataLoader = null;
            }

            private bool IsDisposed
            {
                get { return _dataPresenter._dataLoader != this; }
            }

            public Task ShowAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
            {
                return ShowAsync(dataView, ct => getDataSet(), false, getWhere, getOrderBy);
            }

            public Task ShowAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
            {
                return ShowAsync(dataView, getDataSet, true, getWhere, getOrderBy);
            }

            public DataLoadState? State
            {
                get { return DataView?.DataLoadState; }
            }

            private Task ShowAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, bool cancellable, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
            {
                Debug.Assert(dataView != null);

                if (dataView != DataView)
                    _dataPresenter.AttachView(dataView);

                _revision++;
                _getDataSet = getDataSet;
                _cancellable = cancellable;
                _getWhere = getWhere;
                _getOrderBy = getOrderBy;

                if (_runningTask == null)
                    _runningTask = Run();
                else if (_cts != null)
                    _cts.Cancel();
                return _runningTask;
            }

            public bool CanRetry
            {
                get { return State == DataLoadState.Failed || State == DataLoadState.Cancelled; }
            }

            public Task Retry()
            {
                Debug.Assert(State == DataLoadState.Cancelled || State == DataLoadState.Failed);
                Debug.Assert(_runningTask == null);
                _runningTask = Run();
                return _runningTask;
            }

            public bool CanCancel
            {
                get { return _cts != null && State == DataLoadState.Loading; }
            }

            public void Cancel()
            {
                DataView.OnDataLoadCancelling();
                _cts.Cancel();
            }

            private async Task Run()
            {
                Debug.Assert(DataView != null && _runningTask == null);

                DataSet<T> dataSet = null;
                int revision;
                DataLoadState state;
                string errorMessage = null;
                do
                {
                    revision = _revision;
                    try
                    {
                        if (_cancellable)
                        {
                            using (_cts = new CancellationTokenSource())
                            {
                                DataView.OnDataLoading(true);
                                dataSet = await (_getDataSet(_cts.Token));
                            }
                        }
                        else
                        {
                            DataView.OnDataLoading(false);
                            dataSet = await _getDataSet(CancellationToken.None);
                        }
                        state = DataLoadState.Succeeded;
                    }
                    catch (OperationCanceledException)
                    {
                        state = DataLoadState.Cancelled;
                    }
                    catch (Exception ex)
                    {
                        state = DataLoadState.Failed;
                        errorMessage = ex.ToString();
                    }
                    finally { _cts = null; }
                }
                while (revision != _revision && _getDataSet != null);

                _runningTask = null;

                if (_getDataSet == null)
                {
                    Dispose();
                    return;
                }

                if (state == DataLoadState.Succeeded)
                {
                    try
                    {
                        if (dataSet == null)
                            dataSet = DataSet<T>.New();
                        Predicate<DataRow> where = _getWhere == null ? null : _getWhere(dataSet._);
                        IComparer<DataRow> orderBy = _getOrderBy == null ? null : _getOrderBy(dataSet._);
                        _dataPresenter.Mount(DataView, dataSet, where, orderBy);
                        Dispose();
                        return;
                    }
                    catch (Exception ex)
                    {
                        state = DataLoadState.Failed;
                        errorMessage = ex.ToString();
                    }
                }

                if (state == DataLoadState.Cancelled)
                    DataView.OnDataLoadCancelled();
                else
                    DataView.OnDataLoadFailed(errorMessage);
            }
        }

        private DataLoader _dataLoader;

        internal sealed override bool CanReload
        {
            get { return _dataLoader != null && _dataLoader.CanRetry; }
        }

        internal sealed override void Reload()
        {
            Debug.Assert(CanReload);
            _dataLoader.Retry();
        }

        internal sealed override bool CanCancelLoad
        {
            get { return _dataLoader != null && _dataLoader.CanCancel; }
        }

        internal sealed override void CancelLoad()
        {
            Debug.Assert(CanCancelLoad);
            _dataLoader.Cancel();
        }

        public new DataSet<T> DataSet { get; private set; }

        public sealed override void DetachView()
        {
            if (_dataLoader != null)
                _dataLoader.Reset();
            base.DetachView();
            if (_layoutManager != null)
            {
                _layoutManager.ClearElements();
                _layoutManager = null;
            }
        }

        private LayoutManager _layoutManager;
        internal sealed override LayoutManager LayoutManager
        {
            get { return _layoutManager; }
        }

        protected abstract void BuildTemplate(TemplateBuilder builder);

        public T _
        {
            get { return DataSet == null ? null : DataSet._; }
        }
    }
}
