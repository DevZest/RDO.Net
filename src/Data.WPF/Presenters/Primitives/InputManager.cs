using DevZest.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class InputManager : ElementManager
    {
        private sealed class FlushErrorCollection : KeyedCollection<UIElement, FlushErrorMessage>
        {
            protected override UIElement GetKeyForItem(FlushErrorMessage item)
            {
                return item.Source;
            }
        }

        protected InputManager(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy, emptyContainerViewList)
        {
            Progress = new ValidationProgress(this);
            if (ValidationMode == ValidationMode.Implicit)
                Validate();
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

        private FlushErrorCollection _scalarFlushErrors;
        private FlushErrorCollection InternalScalarFlushErrors
        {
            get
            {
                if (_scalarFlushErrors == null)
                    _scalarFlushErrors = new FlushErrorCollection();
                return _scalarFlushErrors;
            }
        }

        private FlushErrorCollection _scalarValueErrors;
        private FlushErrorCollection InternalScalarValueErrors
        {
            get
            {
                if (_scalarValueErrors == null)
                    _scalarValueErrors = new FlushErrorCollection();
                return _scalarValueErrors;
            }
        }

        internal FlushErrorMessage GetScalarFlushError(UIElement element)
        {
            return GetFlushError(_scalarFlushErrors, element);
        }

        internal FlushErrorMessage GetScalarValueError(UIElement element)
        {
            return GetFlushError(_scalarValueErrors, element);
        }

        private static FlushErrorMessage GetFlushError(FlushErrorCollection flushErrors, UIElement element)
        {
            if (flushErrors == null)
                return null;
            return flushErrors.Contains(element) ? flushErrors[element] : null;
        }

        internal void SetScalarFlushError(UIElement element, FlushErrorMessage inputError)
        {
            SetInputError(InternalScalarFlushErrors, element, inputError);
        }

        internal void SetScalarValueError(UIElement element, FlushErrorMessage inputError)
        {
            SetInputError(InternalScalarValueErrors, element, inputError);
        }

        private void SetInputError(FlushErrorCollection inputErrors, UIElement element, FlushErrorMessage inputError)
        {
            Debug.Assert(inputErrors != null);
            inputErrors.Remove(element);
            if (inputError != null)
                inputErrors.Add(inputError);
            InvalidateView();
        }

        public IReadOnlyList<FlushErrorMessage> ScalarInputErrors
        {
            get
            {
                if (_scalarFlushErrors == null)
                    return Array<FlushErrorMessage>.Empty;
                return _scalarFlushErrors;
            }
        }

        private FlushErrorCollection _rowFlushErrors;
        private FlushErrorCollection InternalRowFlushErrors
        {
            get
            {
                if (_rowFlushErrors == null)
                    _rowFlushErrors = new FlushErrorCollection();
                return _rowFlushErrors;
            }
        }

        internal FlushErrorMessage GetRowFlushError(UIElement element)
        {
            return GetFlushError(_rowFlushErrors, element);
        }

        internal void SetRowFlushError(UIElement element, FlushErrorMessage value)
        {
            SetInputError(InternalRowFlushErrors, element, value);
        }

        public IReadOnlyList<FlushErrorMessage> RowInputErrors
        {
            get
            {
                if (_rowFlushErrors == null)
                    return Array<FlushErrorMessage>.Empty;
                return _rowFlushErrors;
            }
        }

        public ValidationProgress Progress { get; private set; }
        public IValidationDictionary Errors { get; private set; } = ValidationDictionary.Empty;
        public IValidationDictionary Warnings { get; private set; } = ValidationDictionary.Empty;

        public IColumnValidationMessages CurrentRowErrors
        {
            get { return Errors.GetValidationMessages(CurrentRow); }
        }

        public IColumnValidationMessages CurrentRowWarnings
        {
            get { return Warnings.GetValidationMessages(CurrentRow); }
        }

        private void ClearValidationMessages()
        {
            Errors = Warnings = ValidationDictionary.Empty;
        }

        protected override void Reload()
        {
            base.Reload();

            Progress.Reset();
            if (ValidationMode == ValidationMode.Implicit)
                Validate(true);
            else
                ClearValidationMessages();
        }

        private static IColumnValidationMessages GetValidationMessages(IValidationDictionary dictionary, RowPresenter rowPresenter, IColumns columns)
        {
            Debug.Assert(dictionary != null);

            IColumnValidationMessages messages;
            if (!dictionary.TryGetValue(rowPresenter, out messages))
                return ColumnValidationMessages.Empty;

            var result = ColumnValidationMessages.Empty;
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(columns))
                    result = result.Add(message);
            }

            return result;
        }

        internal IColumnValidationMessages GetErrors<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (!Progress.IsVisible(rowPresenter, rowInput.Columns))
                return ColumnValidationMessages.Empty;

            return GetValidationMessages(Errors, rowPresenter, rowInput.Columns);
        }

        internal IColumnValidationMessages GetWarnings<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (!Progress.IsVisible(rowPresenter, rowInput.Columns))
                return ColumnValidationMessages.Empty;

            return GetValidationMessages(Warnings, rowPresenter, rowInput.Columns);
        }

        internal void MakeProgress<T>(RowInput<T> rowInput)
            where T : UIElement, new()
        {
            Progress.MakeProgress(CurrentRow, rowInput);
            if (ValidationMode != ValidationMode.Explicit)
                Validate(_pendingShowAll);
            _pendingShowAll = false;

            OnProgress(rowInput);
            InvalidateView();
        }

        private void OnProgress<T>(RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (ValidationMode == ValidationMode.Explicit)
                return;

            if (HasError(CurrentRow, rowInput.Columns))
                return;

            var asyncValidators = Template.AsyncValidators;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                if (asyncValidator.SourceColumns.Intersect(rowInput.Columns).Count > 0)
                    asyncValidator.Run();
            }
        }

        private bool HasError(RowPresenter rowPresenter, IColumns columns)
        {
            if (Errors.Count == 0)
                return false;

            IColumnValidationMessages messages;
            if (!Errors.TryGetValue(rowPresenter, out messages))
                return false;

            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(columns))
                    return true;
            }

            return false;
        }

        internal ValidationMode ValidationMode
        {
            get { return Template.RowValidationMode; }
        }

        internal RowValidationScope ValidationScope
        {
            get { return Template.RowValidationScope; }
        }

        public void Validate()
        {
            Validate(true);
            InvalidateView();
        }

        private int ErrorMaxEntries
        {
            get { return Template.RowValidationErrorLimit; }
        }

        private int WarningMaxEntries
        {
            get { return Template.RowValidationWarningLimit; }
        }

        private void Validate(bool showAll)
        {
            if (showAll)
                Progress.ShowAll();

            ClearValidationMessages();
            DoValidate();
            Errors.Seal();
            Warnings.Seal();
        }

        private bool MoreErrorsToValidate
        {
            get { return Errors.Count < ErrorMaxEntries; }
        }

        private bool MoreWarningsToValidate
        {
            get { return Warnings.Count < WarningMaxEntries; }
        }

        private bool MoreToValidate
        {
            get { return MoreErrorsToValidate || MoreWarningsToValidate; }
        }

        private void DoValidate()
        {
            if (CurrentRow == null)
                return;

            Validate(CurrentRow);
            if (!MoreToValidate)
                return;

            if (ValidationScope == RowValidationScope.All)
            {
                for (int i = 0; i < Rows.Count; i++)
                {
                    var row = Rows[i];
                    if (row == CurrentRow)
                        continue;
                    Validate(Rows[i]);
                    if (!MoreToValidate)
                        return;
                }
            }
        }

        private void Validate(RowPresenter rowPresenter)
        {
            Debug.Assert(MoreToValidate);

            IColumnValidationMessages errors, warnings;
            Validate(rowPresenter.DataRow, out errors, out warnings);
            if (errors != null && errors.Count > 0)
                Errors = Errors.Add(rowPresenter, errors);
            if (warnings != null && warnings.Count > 0)
                Warnings = Warnings.Add(rowPresenter, warnings);
        }

        private void Validate(DataRow dataRow, out IColumnValidationMessages errors, out IColumnValidationMessages warnings)
        {
            Debug.Assert(MoreToValidate);

            if (MoreErrorsToValidate)
            {
                errors = Validate(dataRow, ValidationSeverity.Error);
                warnings = errors.Count > 0 || MoreWarningsToValidate ? Validate(dataRow, ValidationSeverity.Warning) : ColumnValidationMessages.Empty;
            }
            else
            {
                Debug.Assert(MoreWarningsToValidate);
                warnings = Validate(dataRow, ValidationSeverity.Warning);
                errors = warnings.Count > 0 || MoreErrorsToValidate ? Validate(dataRow, ValidationSeverity.Error) : ColumnValidationMessages.Empty;
            }
        }

        private IColumnValidationMessages Validate(DataRow dataRow, ValidationSeverity? severity)
        {
            return dataRow == DataSet.AddingRow ? DataSet.ValidateAddingRow(severity) : dataRow.Validate(severity);
        }

        protected override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            Progress.OnCurrentRowChanged();
            Template.AsyncValidators.Each(x => x.OnCurrentRowChanged());
        }

        protected override void DisposeRow(RowPresenter rowPresenter)
        {
            base.DisposeRow(rowPresenter);

            Progress.OnRowDisposed(rowPresenter);

            if (Errors.ContainsKey(rowPresenter))
                Errors = Errors.Remove(rowPresenter);

            if (Warnings.ContainsKey(rowPresenter))
                Warnings = Warnings.Remove(rowPresenter);

            Template.AsyncValidators.Each(x => x.OnRowDisposed(rowPresenter));
        }

        public IValidationDictionary ValidationResult { get; private set; } = ValidationDictionary.Empty;

        private bool _pendingShowAll;
        public void Show(IValidationResult validationResult)
        {
            Debug.Assert(validationResult != null);
            ValidationResult = ToValidationDictionary(validationResult);
            Progress.Reset();
            ClearValidationMessages();
            if (ValidationMode == ValidationMode.Implicit)
                _pendingShowAll = true;
            Template.AsyncValidators.Each(x => x.Reset());
            InvalidateView();
        }

        internal IValidationDictionary ToValidationDictionary(IValidationResult validationResult)
        {
            var result = ValidationDictionary.Empty;
            for (int i = 0; i < validationResult.Count; i++)
            {
                var entry = validationResult[i];
                var rowPresenter = this[entry.DataRow];
                if (rowPresenter != null)
                    result = result.Add(rowPresenter, entry.Messages);
            }
            return result;
        }

        private IAsyncValidatorGroup _allRowsAsyncValidators;
        public IAsyncValidatorGroup AllRowsAsyncValidators
        {
            get
            {
                if (_allRowsAsyncValidators == null)
                    _allRowsAsyncValidators = Template.AsyncValidators.Where(x => x.ValidationScope == RowValidationScope.All);
                return _allRowsAsyncValidators;
            }
        }

        private IAsyncValidatorGroup _currentRowAsyncValidators;
        public IAsyncValidatorGroup CurrentRowAsyncValidators
        {
            get
            {
                if (_currentRowAsyncValidators == null)
                    _currentRowAsyncValidators = Template.AsyncValidators.Where(x => x.ValidationScope == RowValidationScope.Current && x.RowInput == null);
                return _currentRowAsyncValidators;
            }
        }

        internal sealed override bool EndEdit()
        {
            if (ValidationScope == RowValidationScope.All)
                return base.EndEdit();

            Debug.Assert(ValidationScope == RowValidationScope.Current);
            Validate(true);
            var hasError = CurrentRowErrors.Count > 0;
            if (hasError)
                return false;

            Progress.Reset();
            return base.EndEdit();
        }
    }
}
