using System.Collections.Generic;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System.Diagnostics;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    public abstract class DataPresenter : IService
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
            get { return LayoutManager?.Template; }
        }

        public Orientation? LayoutOrientation
        {
            get { return Template?.Orientation; }
        }

        public DataSet DataSet
        {
            get { return LayoutManager?.DataSet; }
        }

        public bool IsRecursive
        {
            get { return LayoutManager == null ? false : LayoutManager.IsRecursive; }
        }

        private LayoutManager RequireLayoutManager()
        {
            if (LayoutManager == null)
                throw new InvalidOperationException(DiagnosticMessages.DataPresenter_NullDataSet);
            return LayoutManager;
        }

        public Predicate<DataRow> Where
        {
            get { return LayoutManager?.Where; }
            set { Apply(value, OrderBy); }
        }
        
        public IComparer<DataRow> OrderBy
        {
            get { return LayoutManager?.OrderBy; }
            set { Apply(Where, value); }
        }

        public virtual void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            RequireLayoutManager().Apply(where, orderBy);
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager?.Rows; }
        }

        public RowPresenter CurrentRow
        {
            get { return LayoutManager?.CurrentRow; }
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
                throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidRowPresenter, paramName);
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
            get { return LayoutManager?.SelectedRows; }
        }

        public RowPresenter VirtualRow
        {
            get { return LayoutManager?.VirtualRow; }
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
                throw new InvalidOperationException(DiagnosticMessages.DataPresenter_VerifyCanInsert);
            if (row != null)
            {
                if (row.DataPresenter != this || row.Parent != null)
                    throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidRowPresenter, nameof(row));
            }
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

        public void ValidateRows(int errorLimit = 1, int warningLimit = 0)
        {
            if (errorLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(errorLimit));
            if (warningLimit < 0)
                throw new ArgumentOutOfRangeException(nameof(warningLimit));

            if (CurrentRow == null)
                return;

            var errors = 0;
            var warnings = 0;
            var moreToValidate = Validate(CurrentRow, ref errors, errorLimit, ref warnings, warningLimit);
            if (moreToValidate)
            {
                foreach (var rowPresenter in Rows)
                {
                    if (rowPresenter == CurrentRow || rowPresenter.IsVirtual)
                        continue;

                    moreToValidate = Validate(rowPresenter, ref errors, errorLimit, ref warnings, warningLimit);
                    if (!moreToValidate)
                        break;
                }
            }

            InvalidateView();
        }

        private bool Validate(RowPresenter rowPresenter, ref int errors, int errorLimit, ref int warnings, int warningLimit)
        {
            Debug.Assert(rowPresenter != null);
            rowPresenter.Validate(false);
            if (RowErrors.ContainsKey(rowPresenter))
                errors++;
            if (RowWarnings.ContainsKey(rowPresenter))
                warnings++;
            return errors < errorLimit || warnings < warningLimit;
        }

        public IReadOnlyList<FlushErrorMessage> ScalarFlushErrors
        {
            get { return LayoutManager?.ScalarFlushErrors; }
        }

        public IReadOnlyList<FlushErrorMessage> RowFlushErrors
        {
            get { return LayoutManager?.RowFlushErrors; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> RowErrors
        {
            get { return LayoutManager?.RowValidationErrors; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> RowWarnings
        {
            get { return LayoutManager?.RowValidationWarnings; }
        }

        public IReadOnlyList<ScalarValidationMessage> ScalarErrors
        {
            get { return LayoutManager?.ScalarValidationErrors; }
        }

        public IReadOnlyList<ScalarValidationMessage> ScalarWarnings
        {
            get { return LayoutManager?.ScalarValidationWarnings; }
        }

        public IColumnValidationMessages CurrentRowErrors
        {
            get { return LayoutManager?.CurrentRowErrors; }
        }

        public IColumnValidationMessages CurrentRowWarnings
        {
            get { return LayoutManager?.CurrentRowWarnings; }
        }

        public ScalarValidationProgress ScalarValidationProgress
        {
            get { return LayoutManager?.ScalarValidationProgress; }
        }

        public RowValidationProgress RowValidationProgress
        {
            get { return LayoutManager?.RowValidationProgress; }
        }

        public IRowAsyncValidators RowAsyncValidators
        {
            get { return Template?.RowAsyncValidators; }
        }

        public IScalarAsyncValidators ScalarAsyncValidators
        {
            get { return Template?.ScalarAsyncValidators; }
        }

        public IReadOnlyList<ScalarValidationMessage> AssignedScalarValidationResults
        {
            get { return LayoutManager?.AssignedScalarValidationResults; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> AssignedRowValidationResults
        {
            get { return LayoutManager?.AssignedRowValidationResults; }
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

        public virtual T GetService<T>()
            where T : class, IService
        {
            return (this is T) ? (T)((object)this) : ServiceManager.GetService<T>(this);
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

        #region IService

        DataPresenter IService.DataPresenter
        {
            get { return this; }
        }

        void IService.Initialize(DataPresenter dataPresenter)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
