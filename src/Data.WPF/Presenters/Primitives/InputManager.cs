using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class InputManager : ElementManager
    {
        protected InputManager(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy, emptyContainerViewList)
        {
            ScalarValidation = new ScalarValidation(this);
            RowValidation = new RowValidation(this);
            if (ScalarValidationMode == ValidationMode.Implicit)
                ValidateScalars();
            ValidateCurrentRowIfImplicit();
        }

        public void FlushCurrentRow()
        {
            if (CurrentRow != null && CurrentRow.View != null)
                CurrentRow.View.Flush();
        }

        public ScalarValidation ScalarValidation { get; private set; }
        public IScalarValidationMessages ScalarValidationErrors { get; private set; } = ScalarValidationMessages.Empty;
        public IScalarValidationMessages ScalarValidationWarnings { get; private set; } = ScalarValidationMessages.Empty;

        public RowValidation RowValidation { get; private set; }
        public IRowValidationResults RowValidationErrors { get; private set; } = RowValidationResults.Empty;
        public IRowValidationResults RowValidationWarnings { get; private set; } = RowValidationResults.Empty;

        public IColumnValidationMessages CurrentRowErrors
        {
            get { return RowValidationErrors.GetValidationMessages(CurrentRow); }
        }

        public IColumnValidationMessages CurrentRowWarnings
        {
            get { return RowValidationWarnings.GetValidationMessages(CurrentRow); }
        }

        private void ClearRowValidationMessages(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);
            RowValidationErrors = RowValidationErrors.Remove(rowPresenter);
            RowValidationWarnings = RowValidationWarnings.Remove(rowPresenter);
        }

        protected override void Reload()
        {
            base.Reload();

            RowValidation.Reset();
            RowValidationErrors = RowValidationWarnings = RowValidationResults.Empty;
            AssignedRowValidationResults = RowValidationResults.Empty;
            ValidateCurrentRowIfImplicit();

            DataPresenter?.OnRowsLoaded(true);
        }

        internal void OnFlushed<T>(ScalarInput<T> scalarInput, bool makeProgress, bool valueChanged)
            where T : UIElement, new()
        {
            if (!makeProgress && !valueChanged)
                return;

            if (ScalarValidationMode != ValidationMode.Explicit)
                ValidateScalars(false);
            if (ScalarValidation.UpdateProgress(scalarInput, valueChanged, makeProgress))
                OnProgress(scalarInput);
            InvalidateView();
        }

        private void OnProgress<T>(ScalarInput<T> scalarInput)
            where T : UIElement, new()
        {
            if (RowValidationMode == ValidationMode.Explicit)
                return;

            if (HasError(scalarInput.Target))
                return;

            var asyncValidators = Template.ScalarAsyncValidators;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                if (asyncValidator.SourceScalars.Intersect(scalarInput.Target).Count > 0)
                    asyncValidator.Run();
            }
        }

        private bool HasError(IScalars scalars)
        {
            if (ScalarValidationErrors.Count == 0)
                return false;

            for (int i = 0; i < ScalarValidationErrors.Count; i++)
            {
                var message = ScalarValidationErrors[i];
                if (message.Source.SetEquals(scalars))
                    return true;
            }

            return false;
        }

        internal void OnFlushed<T>(RowInput<T> rowInput, bool makeProgress, bool valueChanged)
            where T : UIElement, new()
        {
            if (!makeProgress && !valueChanged)
                return;

            if (RowValidationMode != ValidationMode.Explicit)
                Validate(CurrentRow, false);
            if (RowValidation.UpdateProgress(rowInput, valueChanged, makeProgress))
                OnProgress(rowInput);
            InvalidateView();
        }

        private void OnProgress<T>(RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (RowValidationMode == ValidationMode.Explicit)
                return;

            if (HasError(CurrentRow, rowInput.Target))
                return;

            var asyncValidators = Template.RowAsyncValidators;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                var sourceColumns = asyncValidator.SourceColumns;
                if (sourceColumns.Overlaps(rowInput.Target) && RowValidation.IsVisible(CurrentRow, sourceColumns))
                    asyncValidator.Run();
            }
        }

        private bool HasError(RowPresenter rowPresenter, IColumns columns)
        {
            if (RowValidationErrors.Count == 0)
                return false;

            IColumnValidationMessages messages;
            if (!RowValidationErrors.TryGetValue(rowPresenter, out messages))
                return false;

            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(columns))
                    return true;
            }

            return false;
        }

        internal ValidationMode ScalarValidationMode
        {
            get { return Template.ScalarValidationMode; }
        }

        public void ValidateScalars()
        {
            ValidateScalars(true);
            InvalidateView();
        }

        private void ValidateScalars(bool showAll)
        {
            if (showAll)
                ScalarValidation.ShowAll();

            ClearScalarValidationMessages();
            var messages = PerformValidateScalars();
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Severity == ValidationSeverity.Error)
                    ScalarValidationErrors = ScalarValidationErrors.Add(message);
                else
                    ScalarValidationWarnings = ScalarValidationWarnings.Add(message);
            }
            ScalarValidationErrors = ScalarValidationErrors.Seal();
            ScalarValidationWarnings = ScalarValidationWarnings.Seal();
        }

        protected virtual IScalarValidationMessages PerformValidateScalars()
        {
            return DataPresenter == null ? ScalarValidationMessages.Empty : DataPresenter.ValidateScalars();
        }

        private void ClearScalarValidationMessages()
        {
            ScalarValidationErrors = ScalarValidationWarnings = ScalarValidationMessages.Empty;
        }

        internal ValidationMode RowValidationMode
        {
            get { return Template.RowValidationMode; }
        }

        private void ValidateCurrentRowIfImplicit()
        {
            if (RowValidationMode == ValidationMode.Implicit)
                ValidateCurrentRow();
        }

        private void ValidateCurrentRow()
        {
            if (CurrentRow != null)
            {
                Validate(CurrentRow, true);
                InvalidateView();
            }
        }

        public void Validate(RowPresenter rowPresenter, bool showAll)
        {
            Debug.Assert(rowPresenter != null);
            if (showAll)
                RowValidation.ShowAll(rowPresenter);
            RowValidationErrors = RowValidationErrors.Remove(rowPresenter);
            RowValidationWarnings = RowValidationWarnings.Remove(rowPresenter);
            var dataRow = rowPresenter.DataRow;
            var errors = Validate(dataRow, ValidationSeverity.Error);
            var warnings = Validate(dataRow, ValidationSeverity.Warning);
            if (errors != null && errors.Count > 0)
                RowValidationErrors = RowValidationErrors.Add(rowPresenter, errors);
            if (warnings != null && warnings.Count > 0)
                RowValidationWarnings = RowValidationWarnings.Add(rowPresenter, warnings);
        }

        public override void InvalidateView()
        {
            RowValidationErrors = RowValidationErrors.Seal();
            RowValidationWarnings = RowValidationWarnings.Seal();
            base.InvalidateView();
        }

        private IColumnValidationMessages Validate(DataRow dataRow, ValidationSeverity? severity)
        {
            return dataRow == DataSet.AddingRow ? DataSet.ValidateAddingRow(severity) : dataRow.Validate(severity);
        }

        protected sealed override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            Template.RowAsyncValidators.Each(x => x.OnCurrentRowChanged());
            ValidateCurrentRowIfImplicit();
            DataPresenter?.OnCurrentRowChanged(oldValue);
        }

        protected override void DisposeRow(RowPresenter rowPresenter)
        {
            base.DisposeRow(rowPresenter);

            RowValidation.OnRowDisposed(rowPresenter);

            if (RowValidationErrors.ContainsKey(rowPresenter))
                RowValidationErrors = RowValidationErrors.Remove(rowPresenter).Seal();

            if (RowValidationWarnings.ContainsKey(rowPresenter))
                RowValidationWarnings = RowValidationWarnings.Remove(rowPresenter).Seal();

            if (AssignedRowValidationResults.ContainsKey(rowPresenter))
                AssignedRowValidationResults = AssignedRowValidationResults.Remove(rowPresenter).Seal();

            Template.RowAsyncValidators.Each(x => x.OnRowDisposed(rowPresenter));
        }

        public IScalarValidationMessages AssignedScalarValidationResults { get; private set; } = ScalarValidationMessages.Empty;

        public void Assign(IScalarValidationMessages validationResults)
        {
            Debug.Assert(validationResults != null);
            AssignedScalarValidationResults = validationResults;
            InvalidateView();
        }

        public IRowValidationResults AssignedRowValidationResults { get; private set; } = RowValidationResults.Empty;

        public void Assign(IDataRowValidationResults validationResults)
        {
            Debug.Assert(validationResults != null);
            Assign(ToRowValidationResults(validationResults));
        }

        public void Assign(IRowValidationResults validationResults)
        {
            Debug.Assert(validationResults != null);
            AssignedRowValidationResults = validationResults;
            InvalidateView();
        }

        internal IRowValidationResults ToRowValidationResults(IDataRowValidationResults validationResults)
        {
            var result = RowValidationResults.Empty;
            for (int i = 0; i < validationResults.Count; i++)
            {
                var entry = validationResults[i];
                var rowPresenter = this[entry.DataRow];
                if (rowPresenter != null)
                    result = result.Add(rowPresenter, entry.Messages);
            }
            return result;
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
            ValidateCurrentRow();
            var hasError = CurrentRowErrors.Count > 0;
            if (hasError)
            {
                FocusToInputError(CurrentRowErrors);
                return false;
            }
            return true;
        }

        private void FocusToInputError(IColumnValidationMessages errors)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                if (FocusToInputError(error, CurrentRow))
                    return;
            }
        }

        internal bool FocusToInputError(ColumnValidationMessage error, RowPresenter row)
        {
            foreach (var rowBinding in Template.RowBindings)
            {
                var rowInput = rowBinding.RowInput;
                if (rowInput == null)
                    continue;

                if (error.Source.IsSupersetOf(rowInput.Target))
                {
                    if (Focus(rowBinding, row))
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

        public bool HasVisibleError
        {
            get
            {
                if (ScalarValidation.FlushErrors.Count > 0 || RowValidation.FlushErrors.Count > 0)
                    return true;

                for (int i = 0; i < ScalarValidationErrors.Count; i++)
                {
                    var error = ScalarValidationErrors[i];
                    if (ScalarValidation.IsVisible(error.Source))
                        return true;
                }

                foreach (var keyValuePair in RowValidationErrors)
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

        public IScalarValidationMessages GetValidationErrors(IScalars scalars)
        {
            var result = ScalarValidationMessages.Empty;
            if (ScalarValidation.IsVisible(scalars))
                result = AddValidationMessages(result, ScalarValidationErrors, scalars);
            result = AddAsyncValidationMessages(result, ValidationSeverity.Error, scalars);
            result = AddValidationMessages(result, AssignedScalarValidationResults.Where(ValidationSeverity.Error), scalars);
            return result;
        }

        public IScalarValidationMessages GetValidationWarnings(IScalars scalars)
        {
            var result = ScalarValidationMessages.Empty;
            if (ScalarValidation.IsVisible(scalars))
                result = AddValidationMessages(result, ScalarValidationWarnings, scalars);
            result = AddAsyncValidationMessages(result, ValidationSeverity.Warning, scalars);
            result = AddValidationMessages(result, AssignedScalarValidationResults.Where(ValidationSeverity.Warning), scalars);
            return result;
        }

        private static IScalarValidationMessages AddValidationMessages(IScalarValidationMessages result, IScalarValidationMessages messages, IScalars scalars)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(scalars))
                    result = result.Add(message);
            }
            return result;
        }

        private IScalarValidationMessages AddAsyncValidationMessages(IScalarValidationMessages result, ValidationSeverity severity, IScalars scalars)
        {
            var asyncValidators = Template.ScalarAsyncValidators;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                var messages = severity == ValidationSeverity.Error ? asyncValidator.Errors : asyncValidator.Warnings;
                result = AddValidationMessages(result, messages, scalars);
            }

            return result;
        }
    }
}
