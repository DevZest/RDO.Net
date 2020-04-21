using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Base class to contain presentation logic for scalar data and strongly typed DataSet.
    /// </summary>
    public abstract class DataPresenter<T> : DataPresenter
        where T : Model, new()
    {
        /// <summary>
        /// Shows DataSet to DataView.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="dataSet">The DataSet.</param>
        /// <param name="resetCriteria">Indicates whether filtering and sorting criteria should be reseted.</param>
        public void Show(DataView dataView, DataSet<T> dataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                Show(dataView, dataSet, null, null);
            else
                Show(dataView, dataSet, Where, OrderBy);
        }

        /// <summary>
        /// Shows DataSet to DataView, with specified filter condition and sorting comparer.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="dataSet">The DataSet.</param>
        /// <param name="where">The filtering condition.</param>
        /// <param name="orderBy">The sorting comparer.</param>
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

        /// <summary>
        /// Shows data to DataView asynchronously.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="getDataSet">The delegate to load data asynchronouly.</param>
        /// <param name="resetCriteria">Indicates whether filtering and sorting criteria should be reseted.</param>
        /// <returns>The async task.</returns>
        public Task ShowAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                return ShowAsync(dataView, getDataSet, null, null);
            else
                return ShowAsync(dataView, getDataSet, _ => Where, _ => OrderBy);
        }

        /// <summary>
        /// Shows data to DataView asynchronously, with specified filtering condition and sorting comparer.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="getDataSet">The delegate to load data asynchronouly.</param>
        /// <param name="getWhere">The delegate to return filtering condition.</param>
        /// <param name="getOrderBy">The delegate to return sorting comparer.</param>
        /// <returns>The async task.</returns>
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

        /// <summary>
        /// Shows data to DataView asynchronously.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="getDataSet">The delegate to load data asynchronouly.</param>
        /// <param name="resetCriteria">Indicates whether filtering and sorting criteria should be reseted.</param>
        /// <returns>The async task.</returns>
        public Task ShowAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                return ShowAsync(dataView, getDataSet, null, null);
            else
                return ShowAsync(dataView, getDataSet, _ => Where, _ => OrderBy);
        }

        /// <summary>
        /// Shows data to DataView asynchronously, with specified filter condition and sorting comparer.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="getDataSet">The delegate to load data asynchronouly.</param>
        /// <param name="getWhere">The delegate to return filtering condition.</param>
        /// <param name="getOrderBy">The delegate to return sorting comparer.</param>
        /// <returns>The async task.</returns>
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

        /// <summary>
        /// Gets the underlying DataSet.
        /// </summary>
        public new DataSet<T> DataSet { get; private set; }

        /// <inheritdoc/>
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

        /// <summary>
        /// Builds the template of this data presenter.
        /// </summary>
        /// <param name="builder">The template builder.</param>
        protected abstract void BuildTemplate(TemplateBuilder builder);

        /// <summary>
        /// Gets the entity of the DataSet.
        /// </summary>
        /// <remarks>In VB.Net, use <see cref="Model"/> property instead because <see cref="_"/> is a VB.Net reserved keyword and is not supported.</remarks>
        public T _
        {
            get { return DataSet == null ? null : DataSet._; }
        }

        /// <summary>
        /// Refresh by reloading DataSet.
        /// </summary>
        /// <param name="dataSet">The DataSet to reload.</param>
        /// <param name="resetCriteria">Indicates whether filtering and sorting criteria should be reseted.</param>
        public void Refresh(DataSet<T> dataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                Refresh(dataSet, null, null);
            else
                Refresh(dataSet, Where, OrderBy);
        }

        /// <summary>
        /// Refresh by reloading DataSet, with specified filtering condition and sorting comparer.
        /// </summary>
        /// <param name="dataSet">The DataSet to reload.</param>
        /// <param name="where">The filtering condition.</param>
        /// <param name="orderBy">The sorting comparer.</param>
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

        /// <summary>
        /// Refresh by reloading DataSet asynchronously.
        /// </summary>
        /// <param name="getDataSet">The delegate to reload DataSet asynchronously.</param>
        /// <param name="resetCriteria">Indicates whether filtering and sorting criteria should be reseted.</param>
        /// <returns>The async task.</returns>
        public Task RefreshAsync(Func<Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                return RefreshAsync(getDataSet, null, null);
            else
                return RefreshAsync(getDataSet, _ => Where, _ => OrderBy);
        }

        /// <summary>
        /// Refresh by reloading DataSet asynchronously, with specified filtering condition and sorting comparer.
        /// </summary>
        /// <param name="getDataSet">The delegate to reload DataSet asynchronously.</param>
        /// <param name="getWhere">The delegate to return filtering condition.</param>
        /// <param name="getOrderBy">The delegate to return sorting comparer.</param>
        /// <returns>The async task.</returns>
        public Task RefreshAsync(Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (getDataSet == null)
                throw new ArgumentNullException(nameof(getDataSet));
            RequireLayoutManager();

            if (_dataLoader == null)
                _dataLoader = new DataLoader(this);
            return _dataLoader.RefreshAsync(getDataSet, getWhere, getOrderBy);
        }

        /// <summary>
        /// Refresh by reloading DataSet asynchronously.
        /// </summary>
        /// <param name="getDataSet">The delegate to reload DataSet asynchronously.</param>
        /// <param name="resetCriteria">Indicates whether filtering and sorting criteria should be reseted.</param>
        /// <returns>The async task.</returns>
        public Task RefreshAsync(Func<CancellationToken, Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                return RefreshAsync(getDataSet, null, null);
            else
                return RefreshAsync(getDataSet, _ => Where, _ => OrderBy);
        }

        /// <summary>
        /// Refresh by reloading DataSet asynchronously, with specified filtering condition and sorting comparer.
        /// </summary>
        /// <param name="getDataSet">The delegate to reload DataSet asynchronously.</param>
        /// <param name="getWhere">The delegate to return filtering condition.</param>
        /// <param name="getOrderBy">The delegate to return sorting comparer.</param>
        /// <returns>The async task.</returns>
        public Task RefreshAsync(Func<CancellationToken, Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (getDataSet == null)
                throw new ArgumentNullException(nameof(getDataSet));
            RequireLayoutManager();

            if (_dataLoader == null)
                _dataLoader = new DataLoader(this);
            return _dataLoader.RefreshAsync(getDataSet, getWhere, getOrderBy);
        }

        /// <summary>
        /// Show or refresh the DataSet to DataView.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="dataSet">The DataSet.</param>
        /// <param name="resetCriteria">Indicates whether filtering and sorting criteria should be reseted.</param>
        public void ShowOrRefresh(DataView dataView, DataSet<T> dataSet, bool resetCriteria = false)
        {
            if (LayoutManager == null)
                Show(dataView, dataSet, resetCriteria);
            else
                Refresh(dataSet, resetCriteria);
        }

        /// <summary>
        /// Show or refresh the DataSet to DataView, with specified filtering contition and sorting comparer.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="dataSet">The DataSet.</param>
        /// <param name="where">The filtering condition.</param>
        /// <param name="orderBy">The sorting comparer.</param>
        public void ShowOrRefresh(DataView dataView, DataSet<T> dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            if (LayoutManager == null)
                Show(dataView, dataSet, where, orderBy);
            else
                Refresh(dataSet, where, orderBy);
        }

        /// <summary>
        /// Show or refresh the DataSet to DataView asynchronously.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="getDataSet">The delegate to load DataSet asynchronously.</param>
        /// <param name="resetCriteria">Indicates whether filtering and sorting criteria should be reseted.</param>
        /// <remarks>The async task.</remarks>
        public Task ShowOrRefreshAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (LayoutManager == null)
                return ShowAsync(dataView, getDataSet, resetCriteria);
            else
                return RefreshAsync(getDataSet, resetCriteria);
        }

        /// <summary>
        /// Show or refresh the DataSet to DataView asynchronously, with specified filtering condition and sorting comparer.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="getDataSet">The delegate to load DataSet asynchronously.</param>
        /// <param name="getWhere">The delegate to return filtering condition.</param>
        /// <param name="getOrderBy">The delegate to return sorting comparer.</param>
        /// <returns>The async task.</returns>
        public Task ShowOrRefreshAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (LayoutManager == null)
                return ShowAsync(dataView, getDataSet, getWhere, getOrderBy);
            else
                return RefreshAsync(getDataSet, getWhere, getOrderBy);
        }

        /// <summary>
        /// Show or refresh the DataSet to DataView asynchronously.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="getDataSet">The delegate to load DataSet asynchronously.</param>
        /// <param name="resetCriteria">Indicates whether filtering and sorting criteria should be reseted.</param>
        /// <remarks>The async task.</remarks>
        public Task ShowOrRefreshAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (LayoutManager == null)
                return ShowAsync(dataView, getDataSet, resetCriteria);
            else
                return RefreshAsync(getDataSet, resetCriteria);
        }

        /// <summary>
        /// Show or refresh the DataSet to DataView asynchronously, with specified filtering condition and sorting comparer.
        /// </summary>
        /// <param name="dataView">The DataView which renders the data.</param>
        /// <param name="getDataSet">The delegate to load DataSet asynchronously.</param>
        /// <param name="getWhere">The delegate to return filtering condition.</param>
        /// <param name="getOrderBy">The delegate to return sorting comparer.</param>
        /// <returns>The async task.</returns>
        public Task ShowOrRefreshAsync(DataView dataView, Func<CancellationToken, Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (LayoutManager == null)
                return ShowAsync(dataView, getDataSet, getWhere, getOrderBy);
            else
                return RefreshAsync(getDataSet, getWhere, getOrderBy);
        }

        /// <summary>
        /// Gets columns which contains values that can match row uniquely.
        /// </summary>
        /// <param name="_">The entity.</param>
        /// <returns>The columns which contains values that can match row uniquely.</returns>
        /// <remarks>The default implementation returns the primary key.</remarks>
        protected virtual IReadOnlyList<Column> GetMatchColumns(T _)
        {
            return _.PrimaryKey;
        }

        /// <summary>
        /// Gets the model of DataPresenter.
        /// </summary>
        /// <remarks>This property is provided for VB.Net because <see cref="_"/> is a VB.Net reserved keyword and is not supported.</remarks>
        public T Model
        {
            get { return _; }
        }
    }
}
