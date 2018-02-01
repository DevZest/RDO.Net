using DevZest.Data.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
                    Check.NotNull(key, nameof(key));
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
                Check.NotNull(key, nameof(key));
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

        protected InputManager(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy, emptyContainerViewList)
        {
        }

        private ScalarValidation _scalarValidation;
        public ScalarValidation ScalarValidation
        {
            get { return _scalarValidation ?? (_scalarValidation = new ScalarValidation(this)); }
        }

        internal ValidationInfo GetValidationInfo(DataView dataView)
        {
            return _scalarValidation == null ? ValidationInfo.Empty : _scalarValidation.GetInfo(dataView);
        }

        internal ValidationInfo GetValidationInfo(Input<ScalarBinding, IScalars> input, int flowIndex)
        {
            return _scalarValidation == null ? ValidationInfo.Empty : _scalarValidation.GetInfo(input, flowIndex);
        }

        internal bool HasValidationError(Input<ScalarBinding, IScalars> input, int flowIndex)
        {
            return _scalarValidation == null ? false : _scalarValidation.HasError(input, flowIndex, true);
        }

        internal virtual IScalarValidationErrors PerformValidateScalars()
        {
            var dataPresenter = DataPresenter;
            return dataPresenter == null ? Presenters.ScalarValidationErrors.Empty : dataPresenter.ValidateScalars();
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

        protected override void Reload()
        {
            base.Reload();
            _rowValidation?.OnReloaded();
            DataPresenter?.OnRowsLoaded(true);
        }

        protected sealed override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            _rowValidation?.OnCurrentRowChanged(oldValue);
            DataPresenter?.OnCurrentRowChanged(oldValue);
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
                RowValidation.CancelEdit();
                base.CancelEdit();
            }
        }

        internal sealed override bool EndEdit()
        {
            var endEdit = DataPresenter == null ? QueryEndEdit() : DataPresenter.QueryEndEdit();
            if (!endEdit)
                return false;

            RowValidation.ExitEdit();
            return base.EndEdit();
        }

        internal bool QueryEndEdit()
        {
            if (_rowValidation == null)
                return true;

            _rowValidation.ValidateCurrentRow();
            var hasError = _rowValidation.HasError(CurrentRow, null, false) || _rowValidation.HasError(CurrentRow, null, true);
            if (hasError)
            {
                FocusToRowInputError();
                return false;
            }

            var isValidationg = RowValidation.AsyncValidators.Any(x => x.Status == AsyncValidatorStatus.Running);
            if (isValidationg)
                return false;
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
                        if (input.Binding[i].Focus())
                            return true;
                    }
                }
            }
            return false;
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
                    if (Focus(input.Binding, CurrentRow))
                        return true;
                }
            }
            return false;
        }

        private bool Focus(RowBinding rowBinding, RowPresenter row)
        {
            if (rowBinding[row] == null)
            {
                if (IsEditing)
                    CurrentRow.EndEdit();
                CurrentRow = row;
            }

            var element = rowBinding[row];
            Debug.Assert(element != null);
            return element.Focus();
        }

        private IReadOnlyList<FlushingError> ScalarFlushingErrors
        {
            get { return _scalarValidation == null ? Array<FlushingError>.Empty : _scalarValidation.FlushingErrors; }
        }

        private IReadOnlyList<FlushingError> RowFlushingErrors
        {
            get { return _rowValidation == null ? Array<FlushingError>.Empty : _rowValidation.FlushingErrors; }
        }

        private IReadOnlyList<ScalarValidationError> ScalarValidationErrors
        {
            get { return _scalarValidation == null ? Array<ScalarValidationError>.Empty : _scalarValidation.Errors; }
        }

        private IReadOnlyDictionary<RowPresenter, IDataValidationErrors> RowValidationErrors
        {
            get { return _rowValidation == null ? EmptyRowValidationResults.Singleton : _rowValidation.Errors; }
        }

        public bool HasVisibleError
        {
            get
            {
                if (ScalarFlushingErrors.Count > 0 || RowFlushingErrors.Count > 0)
                    return true;

                var scalarValidationErrors = ScalarValidationErrors;
                for (int i = 0; i < scalarValidationErrors.Count; i++)
                {
                    var error = scalarValidationErrors[i];
                    if (ScalarValidation.IsVisible(error.Source))
                        return true;
                }

                var rowValidationErrors = RowValidationErrors;
                foreach (var keyValuePair in rowValidationErrors)
                {
                    var rowPresenter = keyValuePair.Key;
                    var messages = keyValuePair.Value;

                    for (int i = 0; i < messages.Count; i++)
                    {
                        var message = messages[i];
                        if (RowValidation.IsVisible(rowPresenter, message.Source))
                            return true;
                    }
                }

                return false;
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
