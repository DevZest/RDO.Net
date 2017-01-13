using System.Collections.Generic;
using DevZest.Data.Windows.Primitives;
using System.Windows;
using System;
using System.Windows.Input;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public abstract class DataPresenter
    {
        public abstract DataView View { get; }

        internal abstract LayoutManager LayoutManager { get; }

        public Template Template
        {
            get { return LayoutManager == null ? null : LayoutManager.Template; }
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

        public RowPresenter CurrentRow
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRow; }
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
            get { return IsEditing && LayoutManager.CurrentRow == LayoutManager.Placeholder; }
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

        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager == null ? null : LayoutManager.SelectedRows; }
        }

        public void Validate()
        {
            RequireLayoutManager().Validate();
        }

        public bool HasPreValidatorError
        {
            get { return LayoutManager == null ? false : LayoutManager.HasPreValidatorError; }
        }

        public IReadOnlyDictionary<RowPresenter, IReadOnlyList<ValidationMessage<Column>>> ValidationErros
        {
            get
            {
                if (LayoutManager == null)
                    return null;
                else
                    return LayoutManager.Errors;
            }
        }

        public IReadOnlyDictionary<RowPresenter, IReadOnlyList<ValidationMessage<Column>>> ValidationWarnings
        {
            get
            {
                if (LayoutManager == null)
                    return null;
                else
                    return LayoutManager.Warnings;
            }
        }

        protected virtual IEnumerable<ValidationMessage<Scalar>> GetScalarValidationMessages()
        {
            yield break;
        }

        public void ValidateScalars()
        {
            RequireLayoutManager().ValidateScalars();
        }

        public bool HasScalarPreValidatorError
        {
            get { return LayoutManager == null ? false : LayoutManager.HasScalarPreValidatorError; }
        }

        public IReadOnlyList<ValidationMessage<Scalar>> ScalarValidationErrors
        {
            get
            {
                if (LayoutManager == null)
                    return null;
                else
                    return LayoutManager.ScalarErrors;
            }
        }

        public IReadOnlyList<ValidationMessage<Scalar>> ScalarValidationWarnings
        {
            get
            {
                if (LayoutManager == null)
                    return null;
                else
                    return LayoutManager.ScalarWarnings;
            }
        }

        public void SetCurrentRow(RowPresenter value, SelectionMode? selectionMode, bool ensureVisible = true)
        {
            if (value == CurrentRow)
                return;

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.DataPresenter != this)
                throw new ArgumentException(Strings.DataPresenter_InvalidRowPresenter, nameof(value));

            if (CurrentRow.IsEditing)
                throw new InvalidOperationException(Strings.DataPresenter_CurrentRowIsEditing);

            RequireLayoutManager().SetCurrentRow(value, selectionMode, ensureVisible);
        }

        protected internal virtual IEnumerable<CommandBinding> DataViewCommandBindings
        {
            get { return null; }
        }

        protected internal virtual IEnumerable<InputBinding> DataViewInputBindings
        {
            get { return null; }
        }

        protected internal virtual IEnumerable<CommandBinding> RowViewCommandBindings
        {
            get { return null; }
        }

        protected internal virtual IEnumerable<InputBinding> RowViewInputBindings
        {
            get { return null; }
        }
    }
}
