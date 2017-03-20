using System.Collections.Generic;
using DevZest.Windows.Data.Primitives;
using System;
using System.Windows.Controls;
using DevZest.Data;
using DevZest.Windows.Controls;
using DevZest.Windows.Controls.Primitives;

namespace DevZest.Windows.Data
{
    public abstract partial class DataPresenter
    {
        public abstract DataView View { get; }

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

        public virtual _Boolean Where
        {
            get { return LayoutManager == null ? null : LayoutManager.Where; }
        }

        public virtual IReadOnlyList<ColumnSort> OrderBy
        {
            get { return LayoutManager == null ? null : LayoutManager.OrderBy; }
        }

        private LayoutManager RequireLayoutManager()
        {
            if (LayoutManager == null)
                throw new InvalidOperationException(Strings.DataPresenter_NullDataSet);
            return LayoutManager;
        }

        public virtual IDataCriteria Criteria
        {
            get { return LayoutManager; }
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager == null ? null : LayoutManager.Rows; }
        }

        public bool CanChangeCurrentRow
        {
            get { return LayoutManager == null ? false : LayoutManager.CanChangeCurrentRow; }
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

        public int FlowCount
        {
            get { return LayoutManager == null ? 1 : LayoutManager.FlowCount; }
        }

        private void VerifyRowPresenter(RowPresenter value,  string paramName)
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

        public void Validate()
        {
            RequireLayoutManager().Validate();
        }

        public IReadOnlyList<ViewInputError> ScalarInputErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.ScalarInputErrors; }
        }

        public IReadOnlyList<ViewInputError> RowInputErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.RowInputErrors; }
        }

        public IValidationDictionary ValidationErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.Errors; }
        }

        public IValidationDictionary ValidationWarnings
        {
            get { return LayoutManager == null ? null : LayoutManager.Warnings; }
        }

        public IValidationMessageGroup CurrentRowErrors
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRowErrors; }
        }

        public IValidationMessageGroup CurrentRowWarnings
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRowWarnings; }
        }

        public ValidationProgress ValidationProgress
        {
            get { return LayoutManager == null ? null : LayoutManager.Progress; }
        }

        public IAsyncValidatorGroup AsyncValidators
        {
            get { return LayoutManager == null ? null : LayoutManager.AllRowsAsyncValidators; }
        }

        public IValidationDictionary ValidationResult
        {
            get { return LayoutManager == null ? null : LayoutManager.ValidationResult; }
        }

        public void Show(IValidationResult validationResult)
        {
            if (validationResult == null)
                throw new ArgumentNullException(nameof(validationResult));
            RequireLayoutManager().Show(validationResult);
        }

        public IScrollable Scrollable
        {
            get { return LayoutManager as ScrollableManager; }
        }

        protected internal virtual IEnumerable<CommandEntry> DataViewCommandEntries
        {
            get { yield break; }
        }

        protected internal virtual void OnRowUpdated(RowPresenter rowPresenter)
        {
        }
    }
}
