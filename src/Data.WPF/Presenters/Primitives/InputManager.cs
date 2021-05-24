using DevZest.Data.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class InputManager : ElementManager
    {
        private sealed class EmptyRowValidationResults : IReadOnlyDictionary<RowPresenter, IDataValidationErrors>
        {
            public static readonly EmptyRowValidationResults Singleton = new EmptyRowValidationResults();
            private EmptyRowValidationResults()
            {
            }

            public IDataValidationErrors this[RowPresenter key]
            {
                get
                {
                    key.VerifyNotNull(nameof(key));
                    throw new ArgumentOutOfRangeException(nameof(key));
                }
            }

            public IEnumerable<RowPresenter> Keys
            {
                get { yield break; }
            }

            public IEnumerable<IDataValidationErrors> Values
            {
                get { yield break; }
            }

            public int Count
            {
                get { return 0; }
            }

            public bool ContainsKey(RowPresenter key)
            {
                key.VerifyNotNull(nameof(key));
                return false;
            }

            public IEnumerator<KeyValuePair<RowPresenter, IDataValidationErrors>> GetEnumerator()
            {
                yield break;
            }

            public bool TryGetValue(RowPresenter key, out IDataValidationErrors value)
            {
                value = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        protected InputManager(InputManager inherit, Template template, DataSet dataSet, IReadOnlyList<Column> rowMatchColumns, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool emptyContainerViewList)
            : base(inherit, template, dataSet, rowMatchColumns, where, orderBy, emptyContainerViewList)
        {
        }

        private ScalarValidation _scalarValidation;
        public ScalarValidation ScalarValidation
        {
            get { return _scalarValidation ?? (_scalarValidation = new ScalarValidation(this)); }
        }

        internal ValidationInfo GetScalarValidationInfo()
        {
            return _scalarValidation == null ? ValidationInfo.Empty : _scalarValidation.GetInfo();
        }

        internal ValidationInfo GetValidationInfo(Input<ScalarBinding, IScalars> input, int flowIndex)
        {
            return _scalarValidation == null ? ValidationInfo.Empty : _scalarValidation.GetInfo(input, flowIndex);
        }

        internal virtual IScalarValidationErrors PerformValidateScalars()
        {
            var presenter = Presenter;
            return presenter == null ? Presenters.ScalarValidationErrors.Empty : presenter.ValidateScalars();
        }

        private RowValidation _rowValidation;
        public RowValidation RowValidation
        {
            get { return _rowValidation ?? (_rowValidation = new RowValidation(this)); }
        }

        internal ValidationInfo GetValidationInfo(RowView rowView)
        {
            return _rowValidation == null ? ValidationInfo.Empty : _rowValidation.GetInfo(rowView);
        }

        internal ValidationInfo GetValidationInfo(RowPresenter rowPresenter, Input<RowBinding, IColumns> input)
        {
            return _rowValidation == null ? ValidationInfo.Empty : _rowValidation.GetInfo(rowPresenter, input);
        }

        internal bool HasValidationError(RowPresenter rowPresenter, Input<RowBinding, IColumns> input)
        {
            return _rowValidation == null ? false : _rowValidation.HasError(rowPresenter, input, true);
        }

        internal bool IsValidating(RowPresenter rowPresenter, Input<RowBinding, IColumns> input)
        {
            return _rowValidation == null ? false : _rowValidation.IsValidatingStatus(rowPresenter, input, true);
        }

        protected sealed override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            _rowValidation?.OnCurrentRowChanged(oldValue);
            DataPresenter?.OnCurrentRowChanged(oldValue);
            InvalidateView();
        }

        protected override void DisposeRow(RowPresenter rowPresenter)
        {
            base.DisposeRow(rowPresenter);
            _rowValidation?.OnRowDisposed(rowPresenter);
        }

        internal override void BeginEdit()
        {
            base.BeginEdit();
            RowValidation.EnterEdit();
        }

        internal sealed override void CancelEdit()
        {
            var cancelEdit = DataPresenter == null ? true : DataPresenter.QueryCancelEdit();
            if (cancelEdit)
            {
                var rowAfterEditing = CurrentRow.IsInserting ? null : CurrentRow;
                base.CancelEdit();
                RowValidation.CancelEdit(rowAfterEditing);
            }
        }

        /// <seealso cref="ElementManager.OnFocused(RowView)"/>.
        internal sealed override RowPresenter EndEdit(bool staysOnInserting)
        {
            var endEdit = DataPresenter == null ? QueryEndEdit() : DataPresenter.QueryEndEdit();
            if (!endEdit)
                return null;
            
            // CurrentRow does not always be the currently editing row after editing, see comments on seealso.
            var rowAfterEditing = base.EndEdit(staysOnInserting);
            RowValidation.ExitEdit(rowAfterEditing);
            return rowAfterEditing;
        }

        internal bool QueryEndEdit()
        {
            if (_rowValidation == null || Template.RowValidationMode == ValidationMode.Explicit)
                return true;

            _rowValidation.ValidateCurrentRow();
            var hasVisibleError = _rowValidation.HasVisibleError(CurrentRow);
            if (hasVisibleError)
            {
                FocusToRowInputError();
                return false;
            }

            var isValidating = _rowValidation.IsValidating;
            if (isValidating)
                return false;

            if (DataPresenter != null)
                return DataPresenter.ConfirmEndEdit();
            return true;
        }

        internal bool QueryEndEditScalars()
        {
            if (_scalarValidation == null || Template.ScalarValidationMode == ValidationMode.Explicit)
                return true;

            _scalarValidation.Validate();
            var hasVisibleError = _scalarValidation.HasVisibleError;
            if (hasVisibleError)
            {
                FocusToScalarInputError();
                return false;
            }

            var isValidating = _scalarValidation.IsValidating;
            if (isValidating)
                return false;

            if (Presenter != null)
                return Presenter.ConfirmEndEditScalars();
            return true;
        }

        public bool FocusToInputError()
        {
            if (FocusToScalarInputError())
                return true;
            else if (CurrentRow != null)
                return FocusToRowInputError();
            else
                return false;
        }

        internal bool FocusToScalarInputError()
        {
            if (_scalarValidation == null)
                return false;

            foreach (var input in _scalarValidation.Inputs)
            {
                for (int i = 0; i < FlowRepeatCount; i++)
                {
                    if (_scalarValidation.HasError(input, i, true))
                    {
                        if (TryFocus(input.Binding, i))
                            return true;
                    }
                }
            }
            return false;
        }

        private bool TryFocus(ScalarBinding scalarBinding, int flowIndex)
        {
            var element = scalarBinding[flowIndex];
            if (element.IsKeyboardFocusWithin)
                return true;
            return element.Focus();
        }

        private bool FocusToRowInputError()
        {
            Debug.Assert(CurrentRow != null);
            if (_rowValidation == null)
                return false;

            foreach (var input in _rowValidation.Inputs)
            {
                if (_rowValidation.HasError(CurrentRow, input, true))
                {
                    if (TryFocus(input.Binding, CurrentRow))
                        return true;
                }
            }
            return false;
        }

        private bool TryFocus(RowBinding rowBinding, RowPresenter row)
        {
            if (rowBinding[row] == null)
            {
                if (IsEditing)
                    CurrentRow.EndEdit();
                CurrentRow = row;
            }

            var element = rowBinding[row];
            Debug.Assert(element != null);
            if (element.IsKeyboardFocusWithin)
                return true;
            return element.Focus();
        }

        public bool CanSubmitInput
        {
            get
            {
                if (_scalarValidation != null)
                {
                    if (_scalarValidation.HasVisibleError || _scalarValidation.IsValidating)
                        return false;
                }

                if (_rowValidation != null)
                {
                    if (_rowValidation.IsValidating)
                        return false;

                    if (CurrentRow != null)
                    {
                        if (_rowValidation.HasVisibleError(CurrentRow))
                            return false;
                    }

                    foreach (var row in Rows)
                    {
                        if (row != CurrentRow && _rowValidation.HasVisibleError(row))
                            return false;
                    }
                }

                return true;
            }
        }

        public void FlushScalars()
        {
            var scalarBindings = Template.ScalarBindings;
            foreach (var scalarBinding in scalarBindings)
            {
                for (int i = 0; i < scalarBinding.FlowRepeatCount; i++)
                {
                    var element = scalarBinding[i];
                    scalarBinding.FlushInput(element);
                }
            }
        }

        public void FlushCurrentRow()
        {
            if (CurrentRow != null && CurrentRow.View != null)
                CurrentRow.View.Flush();
        }

        internal IValidationErrors GetVisibleValidationErrors(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);

            if (_rowValidation == null)
                return ValidationErrors.Empty;

            var result = ValidationErrors.Empty;
            foreach (var input in _rowValidation.Inputs)
            {
                var errors = GetValidationInfo(rowPresenter, input).Errors;
                for (int i = 0; i < errors.Count; i++)
                    result = result.Add(errors[i]);
            }

            {
                var errors = GetValidationInfo(rowPresenter.View).Errors;
                for (int i = 0; i < errors.Count; i++)
                    result = result.Add(errors[i]);
            }
            return result.Seal();
        }

        internal bool HasVisibleValidationError(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);

            return _rowValidation == null ? false : _rowValidation.HasVisibleError(rowPresenter);
        }
    }
}
