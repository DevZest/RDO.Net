using System.Collections.Generic;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System.Diagnostics;
using DevZest.Data.Presenters.Services;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    public abstract class DataPresenter
    {
        public event EventHandler ViewRefreshing = delegate { };
        public event EventHandler ViewRefreshed = delegate { };

        protected internal virtual void OnViewRefreshing()
        {
            ViewRefreshing(this, EventArgs.Empty);
        }

        protected internal virtual void OnViewRefreshed()
        {
            ViewRefreshed(this, EventArgs.Empty);
        }

        public DataView View { get; private set; }

        public virtual void DetachView()
        {
            if (View == null)
                return;
            View.CleanupCommandEntries();
            View.DataPresenter = null;
            View = null;
        }

        internal void AttachView(DataView value)
        {
            if (View != null)
                DetachView();

            Debug.Assert(View == null && value != null);
            View = value;
            View.DataPresenter = this;
        }

        public event EventHandler<EventArgs> ViewChanged = delegate { };

        protected virtual void OnViewChanged()
        {
            CommandManager.InvalidateRequerySuggested();
            ViewChanged(this, EventArgs.Empty);
        }

        internal abstract LayoutManager LayoutManager { get; }

        public Template Template
        {
            get { return LayoutManager == null ? null : LayoutManager.Template; }
        }

        public Orientation? LayoutOrientation
        {
            get { return Template.Orientation; }
        }

        public DataSet DataSet
        {
            get { return LayoutManager == null ? null : LayoutManager.DataSet; }
        }

        public bool IsRecursive
        {
            get { return LayoutManager == null ? false : LayoutManager.IsRecursive; }
        }

        private LayoutManager RequireLayoutManager()
        {
            if (LayoutManager == null)
                throw new InvalidOperationException(Strings.DataPresenter_NullDataSet);
            return LayoutManager;
        }

        public Predicate<DataRow> Where
        {
            get { return LayoutManager == null ? null : LayoutManager.Where; }
            set { Apply(value, OrderBy); }
        }
        
        public IComparer<DataRow> OrderBy
        {
            get { return LayoutManager == null ? null : LayoutManager.OrderBy; }
            set { Apply(Where, value); }
        }

        public virtual void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            RequireLayoutManager().Apply(where, orderBy);
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager == null ? null : LayoutManager.Rows; }
        }

        public RowPresenter CurrentRow
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRow; }
            set
            {
                VerifyRowPresenter(value, nameof(value));
                RequireLayoutManager().CurrentRow = value;
            }
        }

        public ContainerView CurrentContainerView
        {
            get { return RequireLayoutManager().CurrentContainerView; }
        }

        public int FlowRepeatCount
        {
            get { return LayoutManager == null ? 1 : LayoutManager.FlowRepeatCount; }
        }

        private void VerifyRowPresenter(RowPresenter value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);

            if (value.DataPresenter != this)
                throw new ArgumentException(Strings.DataPresenter_InvalidRowPresenter, paramName);
        }

        public void Select(RowPresenter rowPresenter, SelectionMode selectionMode, bool ensureVisible = true)
        {
            VerifyRowPresenter(rowPresenter, nameof(rowPresenter));
            var oldCurrentRow = CurrentRow;
            CurrentRow = rowPresenter;
            RequireLayoutManager().Select(rowPresenter, selectionMode, oldCurrentRow);
            if (ensureVisible && Scrollable != null)
                Scrollable.EnsureCurrentRowVisible();
        }

        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager == null ? null : LayoutManager.SelectedRows; }
        }

        public bool IsEditing
        {
            get { return LayoutManager == null ? false : LayoutManager.IsEditing; }
        }

        public RowPresenter EditingRow
        {
            get { return CurrentRow != null && IsEditing ? CurrentRow : null; }
        }

        public bool IsInserting
        {
            get { return IsEditing && LayoutManager.CurrentRow == LayoutManager.VirtualRow; }
        }

        public RowPresenter InsertingRow
        {
            get { return IsInserting ? CurrentRow : null; }
        }

        public bool CanInsert
        {
            get { return !IsEditing && RequireLayoutManager().DataSet.EditingRow == null; }
        }

        public void BeginInsertBefore(RowPresenter row = null)
        {
            VerifyInsert(row);
            RequireLayoutManager().BeginInsertBefore(null, row);
        }

        public void BeginInsertAfter(RowPresenter row = null)
        {
            VerifyInsert(row);
            RequireLayoutManager().BeginInsertAfter(null, row);
        }

        private void VerifyInsert(RowPresenter row)
        {
            if (!CanInsert)
                throw new InvalidOperationException(Strings.DataPresenter_VerifyCanInsert);
            if (row != null && row.DataPresenter != this)
                throw new ArgumentException(Strings.DataPresenter_InvalidRowPresenter, nameof(row));
        }

        public void InvalidateView()
        {
            RequireLayoutManager().InvalidateView();
        }

        public void FlushScalars()
        {
            RequireLayoutManager().FlushScalars();
        }

        public void FlushCurrentRow()
        {
            RequireLayoutManager().FlushCurrentRow();
        }

        public void ValidateScalars()
        {
            RequireLayoutManager().ValidateScalars();
        }

        public void ValidateRows()
        {
            RequireLayoutManager().ValidateRows();
        }

        public IReadOnlyList<FlushErrorMessage> ScalarFlushErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.ScalarFlushErrors; }
        }

        public IReadOnlyList<FlushErrorMessage> RowFlushErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.RowFlushErrors; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> RowErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.RowValidationErrors; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> RowWarnings
        {
            get { return LayoutManager == null ? null : LayoutManager.RowValidationWarnings; }
        }

        public IReadOnlyList<ScalarValidationMessage> ScalarErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.ScalarValidationErrors; }
        }

        public IReadOnlyList<ScalarValidationMessage> ScalarWarnings
        {
            get { return LayoutManager == null ? null : LayoutManager.ScalarValidationWarnings; }
        }

        public IColumnValidationMessages CurrentRowErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRowErrors; }
        }

        public IColumnValidationMessages CurrentRowWarnings
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRowWarnings; }
        }

        public RowValidationProgress RowValidationProgress
        {
            get { return LayoutManager == null ? null : LayoutManager.RowValidationProgress; }
        }

        public IRowAsyncValidators AllRowsAsyncValidators
        {
            get { return LayoutManager == null ? null : LayoutManager.AllRowsAsyncValidators; }
        }

        public IReadOnlyList<ScalarValidationMessage> AssignedScalarValidationResults
        {
            get { return LayoutManager == null ? null : LayoutManager.AssignedScalarValidationResults; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> AssignedRowValidationResults
        {
            get { return LayoutManager == null ? null : LayoutManager.AssignedRowValidationResults; }
        }

        public void Assign(IScalarValidationMessages validationResults)
        {
            if (validationResults == null)
                throw new ArgumentNullException(nameof(validationResults));
            RequireLayoutManager().Assign(validationResults);
        }

        public void Assign(IDataRowValidationResults validationResults)
        {
            if (validationResults == null)
                throw new ArgumentNullException(nameof(validationResults));
            RequireLayoutManager().Assign(validationResults);
        }

        public void Assign(IRowValidationResults validationResults)
        {
            if (validationResults == null)
                throw new ArgumentNullException(nameof(validationResults));
            RequireLayoutManager().Assign(validationResults);
        }

        public IScrollable Scrollable
        {
            get { return LayoutManager as ScrollableManager; }
        }

        internal protected virtual void ToggleExpandState(RowPresenter rowPresenter)
        {
            rowPresenter.InternalToggleExpandState();
        }

        internal protected virtual bool HasChildren(RowPresenter rowPresenter)
        {
            return rowPresenter.InternalHasChildren;
        }

        private Dictionary<Type, IService> _services;

        public virtual IService this[Type type]
        {
            get
            {
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                if (_services == null)
                    return null;
                IService result;
                return _services.TryGetValue(type, out result) ? result : null;
            }
            set
            {
                if (type == null)
                    throw new ArgumentNullException(nameof(type));
                IService oldValue;
                if (_services == null)
                    oldValue = null;
                else
                    _services.TryGetValue(type, out oldValue);

                if (value == null)
                {
                    if (_services != null && _services.ContainsKey(type))
                        _services.Remove(type);
                }
                else
                {
                    if (_services == null)
                        _services = new Dictionary<Type, IService>();
                    _services[type] = value;
                }

                if (oldValue != null)
                    oldValue.DataPresenter = null;
                if (value != null)
                    value.DataPresenter = this;
            }
        }

        public T GetService<T>(Func<T> createIfNull = null)
            where T : IService
        {
            var type = typeof(T);
            var result = (T)this[type];

            if (createIfNull == null)
                return result;

            if (result == null)
            {
                result = createIfNull();
                this[type] = result;
            }

            return result;
        }

        internal abstract void Reload();

        internal abstract bool CanReload { get; }

        internal abstract void CancelLoading();

        internal abstract bool CanCancelLoading { get; }

        private List<Scalar> _scalars = new List<Scalar>();
        public IReadOnlyList<Scalar> Scalars
        {
            get { return _scalars; }
        }

        protected Scalar<T> NewScalar<T>(T value = default(T))
        {
            var result = new Scalar<T>(value);
            _scalars.Add(result);
            return result;
        }

        internal IScalarValidationMessages PerformValidateScalars()
        {
            var result = ScalarValidationMessages.Empty;
            for (int i = 0; i < Scalars.Count; i++)
                result = Scalars[i].Validate(result);
            return PerformValidateScalars(result);
        }

        protected virtual IScalarValidationMessages PerformValidateScalars(IScalarValidationMessages result)
        {
            return result;
        }

        public bool HasVisibleInputError
        {
            get { return LayoutManager == null ? false : LayoutManager.HasVisibleError; }
        }

        public bool SubmitInput()
        {
            if (ScalarFlushErrors.Count > 0 || RowFlushErrors.Count > 0)
                return false;

            ValidateScalars();
            ValidateRows();
            if (HasVisibleInputError)
                return false;

            if (IsEditing)
                CurrentRow.EndEdit();
            return true;
        }
    }
}
