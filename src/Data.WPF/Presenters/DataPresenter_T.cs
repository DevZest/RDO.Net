using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Linq;

namespace DevZest.Data.Presenters
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
                throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidDataView, nameof(dataView));

            if (_dataLoader != null)
                _dataLoader.Reset();
            AttachView(dataView);
            Mount(dataView, dataSet, where, orderBy, MountMode.Show);
            OnViewChanged();
        }

        private void Mount(DataView dataView, DataSet<T> dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy, MountMode mode)
        {
            var inherit = mode != MountMode.Show;
            var oldLayoutManager = _layoutManager;
            var template = inherit ? Template : new Template();
            Debug.Assert(template != null);

            if (inherit)
            {
                DetachView();
                AttachView(dataView);
            }

            DataSet = dataSet.EnsureInitialized();
            using (var builder = new TemplateBuilder(template, DataSet.Model, inherit))
            {
                BuildTemplate(builder);
                builder.Seal();
            }
            _layoutManager = LayoutManager.Create(inherit ? oldLayoutManager : null, this, template, dataSet, GetMatchColumns(dataSet._), where, orderBy);
            ServiceManager.Reset(this, mode == MountMode.Reload);
            OnMounted(mode);
            dataView.OnDataLoaded();
        }

        public Task ShowAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                return ShowAsync(dataView, getDataSet, null, null);
            else
                return ShowAsync(dataView, getDataSet, _ => Where, _ => OrderBy);
        }

        public Task ShowAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (dataView == null)
                throw new ArgumentNullException(nameof(dataView));
            if (getDataSet == null)
                throw new ArgumentNullException(nameof(getDataSet));

            if (_dataLoader == null)
                _dataLoader = new DataLoader(this);
            return _dataLoader.ShowAsync(dataView, getDataSet, getWhere, getOrderBy);
        }

        public Task ShowAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                return ShowAsync(dataView, getDataSet, null, null);
            else
                return ShowAsync(dataView, getDataSet, _ => Where, _ => OrderBy);
        }

        public Task ShowAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (dataView == null)
                throw new ArgumentNullException(nameof(dataView));
            if (getDataSet == null)
                throw new ArgumentNullException(nameof(getDataSet));

            if (_dataLoader == null)
                _dataLoader = new DataLoader(this);
            return _dataLoader.ShowAsync(dataView, getDataSet, getWhere, getOrderBy);
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
                return ShowOrRefreshAsync(dataView, ct => getDataSet(), false, getWhere, getOrderBy);
            }

            public Task ShowAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
            {
                return ShowOrRefreshAsync(dataView, getDataSet, true, getWhere, getOrderBy);
            }

            public Task RefreshAsync(Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
            {
                return ShowOrRefreshAsync(null, ct => getDataSet(), false, getWhere, getOrderBy);
            }

            public Task RefreshAsync(Func<CancellationToken, Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
            {
                return ShowOrRefreshAsync(null, getDataSet, true, getWhere, getOrderBy);
            }

            public DataLoadState? State
            {
                get { return DataView?.DataLoadState; }
            }

            private Task ShowOrRefreshAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, bool cancellable, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
            {
                bool dataViewChanged = false;
                if (dataView != null)
                {
                    dataViewChanged = dataView != DataView;
                    _dataPresenter.AttachView(dataView);
                }

                _revision++;
                _getDataSet = getDataSet;
                _cancellable = cancellable;
                _getWhere = getWhere;
                _getOrderBy = getOrderBy;

                if (_runningTask == null)
                    _runningTask = Run();

                if (dataViewChanged)
                    _dataPresenter.OnViewChanged();
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

            private LayoutManager LayoutManager
            {
                get { return _dataPresenter.LayoutManager; }
            }

            private static void DoEvents()
            {
                DispatcherFrame f = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                (SendOrPostCallback)delegate (object arg)
                {
                    DispatcherFrame fr = arg as DispatcherFrame;
                    fr.Continue = false;
                }, f);
                Dispatcher.PushFrame(f);
            }

            private async Task Run()
            {
                Debug.Assert(DataView != null && _runningTask == null);

                if (DataView.IsKeyboardFocusWithin && LayoutManager != null)
                    LayoutManager.Template.ResetInitialFocus();

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

                    DoEvents();
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
                            dataSet = DataSet<T>.Create();
                        Predicate<DataRow> where = _getWhere == null ? null : _getWhere(dataSet._);
                        IComparer<DataRow> orderBy = _getOrderBy == null ? null : _getOrderBy(dataSet._);
                        _dataPresenter.Mount(DataView, dataSet, where, orderBy, LayoutManager != null ? MountMode.Refresh : MountMode.Show);
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

        internal sealed override bool CanCancelLoading
        {
            get { return _dataLoader != null && _dataLoader.CanCancel; }
        }

        internal sealed override void CancelLoading()
        {
            Debug.Assert(CanCancelLoading);
            _dataLoader.Cancel();
        }

        public new DataSet<T> DataSet { get; private set; }

        public sealed override void DetachView()
        {
            if (_dataLoader != null)
                _dataLoader.Reset();
            if (_layoutManager != null)
                _layoutManager.ClearElements();
            base.DetachView();  // This must be called after _layoutManager.ClearElements() to ensure DataPresenter.View is not null when doing cleanup
            _layoutManager = null; // This must be called after base.DetachView()
            DataSet = null;
            OnViewChanged();
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

        public void Refresh(DataSet<T> dataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                Refresh(dataSet, null, null);
            else
                Refresh(dataSet, Where, OrderBy);
        }

        public void Refresh(DataSet<T> dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));
            RequireLayoutManager();

            Refresh(dataSet, where, orderBy, false);
        }

        internal override void PerformApply(Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            Refresh(DataSet, where, orderBy, true);
        }

        private void Refresh(DataSet<T> dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool isReload)
        {
            if (View.IsKeyboardFocusWithin)
                Template.ResetInitialFocus();
            Mount(View, dataSet, where, orderBy, isReload ? MountMode.Reload : MountMode.Refresh);
        }

        public Task RefreshAsync(Func<Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                return RefreshAsync(getDataSet, null, null);
            else
                return RefreshAsync(getDataSet, _ => Where, _ => OrderBy);
        }

        public Task RefreshAsync(Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (getDataSet == null)
                throw new ArgumentNullException(nameof(getDataSet));
            RequireLayoutManager();

            if (_dataLoader == null)
                _dataLoader = new DataLoader(this);
            return _dataLoader.RefreshAsync(getDataSet, getWhere, getOrderBy);
        }

        public Task RefreshAsync(Func<CancellationToken, Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                return RefreshAsync(getDataSet, null, null);
            else
                return RefreshAsync(getDataSet, _ => Where, _ => OrderBy);
        }

        public Task RefreshAsync(Func<CancellationToken, Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (getDataSet == null)
                throw new ArgumentNullException(nameof(getDataSet));
            RequireLayoutManager();

            if (_dataLoader == null)
                _dataLoader = new DataLoader(this);
            return _dataLoader.RefreshAsync(getDataSet, getWhere, getOrderBy);
        }

        public void ShowOrRefresh(DataView dataView, DataSet<T> dataSet, bool resetCriteria = false)
        {
            if (LayoutManager == null)
                Show(dataView, dataSet, resetCriteria);
            else
                Refresh(dataSet, resetCriteria);
        }

        public void ShowOrRefresh(DataView dataView, DataSet<T> dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            if (LayoutManager == null)
                Show(dataView, dataSet, where, orderBy);
            else
                Refresh(dataSet, where, orderBy);
        }

        public Task ShowOrRefreshAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (LayoutManager == null)
                return ShowAsync(dataView, getDataSet, resetCriteria);
            else
                return RefreshAsync(getDataSet, resetCriteria);
        }

        public Task ShowOrRefreshAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (LayoutManager == null)
                return ShowAsync(dataView, getDataSet, getWhere, getOrderBy);
            else
                return RefreshAsync(getDataSet, getWhere, getOrderBy);
        }

        public Task ShowOrRefreshAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (LayoutManager == null)
                return ShowAsync(dataView, getDataSet, resetCriteria);
            else
                return RefreshAsync(getDataSet, resetCriteria);
        }

        public Task ShowOrRefreshAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (LayoutManager == null)
                return ShowAsync(dataView, getDataSet, getWhere, getOrderBy);
            else
                return RefreshAsync(getDataSet, getWhere, getOrderBy);
        }

        protected virtual IReadOnlyList<Column> GetMatchColumns(T _)
        {
            return _.PrimaryKey;
        }
    }
}
