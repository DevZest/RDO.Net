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
            ScalarValidationProgress = new ScalarValidationProgress(this);
            RowValidationProgress = new RowValidationProgress(this);
            if (ScalarValidationMode == ValidationMode.Implicit)
                ValidateScalars();
            if (RowValidationMode == ValidationMode.Implicit)
                ValidateRows();
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

        internal FlushErrorMessage GetScalarFlushError(UIElement element)
        {
            return GetFlushError(_scalarFlushErrors, element);
        }

        private static FlushErrorMessage GetFlushError(FlushErrorCollection flushErrors, UIElement element)
        {
            if (flushErrors == null)
                return null;
            return flushErrors.Contains(element) ? flushErrors[element] : null;
        }

        internal void SetScalarFlushError(UIElement element, FlushErrorMessage inputError)
        {
            SetFlushError(InternalScalarFlushErrors, element, inputError);
        }

        private void SetFlushError(FlushErrorCollection inputErrors, UIElement element, FlushErrorMessage inputError)
        {
            Debug.Assert(inputErrors != null);
            inputErrors.Remove(element);
            if (inputError != null)
                inputErrors.Add(inputError);
            InvalidateView();
        }

        public IReadOnlyList<FlushErrorMessage> ScalarFlushErrors
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
            SetFlushError(InternalRowFlushErrors, element, value);
        }

        public IReadOnlyList<FlushErrorMessage> RowFlushErrors
        {
            get
            {
                if (_rowFlushErrors == null)
                    return Array<FlushErrorMessage>.Empty;
                return _rowFlushErrors;
            }
        }

        public ScalarValidationProgress ScalarValidationProgress { get; private set; }
        public IScalarValidationMessages ScalarValidationErrors { get; private set; } = ScalarValidationMessages.Empty;
        public IScalarValidationMessages ScalarValidationWarnings { get; private set; } = ScalarValidationMessages.Empty;

        public RowValidationProgress RowValidationProgress { get; private set; }
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

        private void ClearRowValidationMessages()
        {
            RowValidationErrors = RowValidationWarnings = RowValidationResults.Empty;
        }

        protected override void Reload()
        {
            base.Reload();

            RowValidationProgress.Reset();
            AssignedRowValidationResults = RowValidationResults.Empty;
            if (RowValidationMode == ValidationMode.Implicit)
                ValidateRows(true);
            else
                ClearRowValidationMessages();
        }

        private static IColumnValidationMessages GetValidationMessages(IRowValidationResults dictionary, RowPresenter rowPresenter, IColumns columns)
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
            if (!RowValidationProgress.IsVisible(rowPresenter, rowInput.Target))
                return ColumnValidationMessages.Empty;

            return GetValidationMessages(RowValidationErrors, rowPresenter, rowInput.Target);
        }

        internal IColumnValidationMessages GetWarnings<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (!RowValidationProgress.IsVisible(rowPresenter, rowInput.Target))
                return ColumnValidationMessages.Empty;

            return GetValidationMessages(RowValidationWarnings, rowPresenter, rowInput.Target);
        }

        internal void MakeProgress<T>(ScalarInput<T> scalarInput)
            where T : UIElement, new()
        {
            ScalarValidationProgress.MakeProgress(scalarInput);
            if (ScalarValidationMode != ValidationMode.Explicit)
                ValidateScalars(_scalarPendingShowAll);
            _scalarPendingShowAll = false;

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

        internal void MakeProgress<T>(RowInput<T> rowInput)
            where T : UIElement, new()
        {
            RowValidationProgress.MakeProgress(CurrentRow, rowInput);
            if (RowValidationMode != ValidationMode.Explicit)
                ValidateRows(_rowPendingShowAll);
            _rowPendingShowAll = false;

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
                if (asyncValidator.SourceColumns.Intersect(rowInput.Target).Count > 0)
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
                ScalarValidationProgress.ShowAll();

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
            return DataPresenter == null ? ScalarValidationMessages.Empty : DataPresenter.PerformValidateScalars();
        }

        private void ClearScalarValidationMessages()
        {
            ScalarValidationErrors = ScalarValidationWarnings = ScalarValidationMessages.Empty;
        }

        internal ValidationMode RowValidationMode
        {
            get { return Template.RowValidationMode; }
        }

        internal RowValidationScope RowValidationScope
        {
            get { return Template.RowValidationScope; }
        }

        public void ValidateRows()
        {
            ValidateRows(true);
            InvalidateView();
        }

        private int RowValidationErrorLimit
        {
            get { return Template.RowValidationErrorLimit; }
        }

        private int RowValidationWarningLimit
        {
            get { return Template.RowValidationWarningLimit; }
        }

        private void ValidateRows(bool showAll)
        {
            if (showAll)
                RowValidationProgress.ShowAll();

            ClearRowValidationMessages();
            DoValidateRows();
            RowValidationErrors.Seal();
            RowValidationWarnings.Seal();
        }

        private bool MoreErrorsToValidate
        {
            get { return RowValidationErrors.Count < RowValidationErrorLimit; }
        }

        private bool MoreWarningsToValidate
        {
            get { return RowValidationWarnings.Count < RowValidationWarningLimit; }
        }

        private bool MoreToValidate
        {
            get { return MoreErrorsToValidate || MoreWarningsToValidate; }
        }

        private void DoValidateRows()
        {
            if (CurrentRow == null)
                return;

            Validate(CurrentRow);
            if (!MoreToValidate)
                return;

            if (RowValidationScope == RowValidationScope.All)
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
                RowValidationErrors = RowValidationErrors.Add(rowPresenter, errors);
            if (warnings != null && warnings.Count > 0)
                RowValidationWarnings = RowValidationWarnings.Add(rowPresenter, warnings);
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
            RowValidationProgress.OnCurrentRowChanged();
            Template.RowAsyncValidators.Each(x => x.OnCurrentRowChanged());
        }

        protected override void DisposeRow(RowPresenter rowPresenter)
        {
            base.DisposeRow(rowPresenter);

            RowValidationProgress.OnRowDisposed(rowPresenter);

            if (RowValidationErrors.ContainsKey(rowPresenter))
                RowValidationErrors = RowValidationErrors.Remove(rowPresenter).Seal();

            if (RowValidationWarnings.ContainsKey(rowPresenter))
                RowValidationWarnings = RowValidationWarnings.Remove(rowPresenter).Seal();

            if (AssignedRowValidationResults.ContainsKey(rowPresenter))
                AssignedRowValidationResults = AssignedRowValidationResults.Remove(rowPresenter).Seal();

            Template.RowAsyncValidators.Each(x => x.OnRowDisposed(rowPresenter));
        }

        public IScalarValidationMessages AssignedScalarValidationResults { get; private set; } = ScalarValidationMessages.Empty;

        private bool _scalarPendingShowAll;
        public void Assign(IScalarValidationMessages validationResults)
        {
            Debug.Assert(validationResults != null);
            AssignedScalarValidationResults = validationResults;
            ScalarValidationProgress.Reset();
            ClearScalarValidationMessages();
            if (RowValidationMode == ValidationMode.Implicit)
                _scalarPendingShowAll = true;
            Template.ScalarAsyncValidators.Each(x => x.Reset());
            InvalidateView();
        }

        public IRowValidationResults AssignedRowValidationResults { get; private set; } = RowValidationResults.Empty;

        private bool _rowPendingShowAll;
        public void Assign(IDataRowValidationResults validationResults)
        {
            Debug.Assert(validationResults != null);
            Assign(ToRowValidationResults(validationResults));
        }

        public void Assign(IRowValidationResults validationResults)
        {
            Debug.Assert(validationResults != null);
            AssignedRowValidationResults = validationResults;
            RowValidationProgress.Reset();
            ClearRowValidationMessages();
            if (RowValidationMode == ValidationMode.Implicit)
                _rowPendingShowAll = true;
            Template.RowAsyncValidators.Each(x => x.Reset());
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

        private IRowAsyncValidators _allRowsAsyncValidators;
        public IRowAsyncValidators AllRowsAsyncValidators
        {
            get
            {
                if (_allRowsAsyncValidators == null)
                    _allRowsAsyncValidators = Template.RowAsyncValidators.Where(x => x.ValidationScope == RowValidationScope.All);
                return _allRowsAsyncValidators;
            }
        }

        private IRowAsyncValidators _currentRowAsyncValidators;
        public IRowAsyncValidators CurrentRowAsyncValidators
        {
            get
            {
                if (_currentRowAsyncValidators == null)
                    _currentRowAsyncValidators = Template.RowAsyncValidators.Where(x => x.ValidationScope == RowValidationScope.Current && x.RowInput == null);
                return _currentRowAsyncValidators;
            }
        }

        internal sealed override bool EndEdit()
        {
            if (RowValidationScope == RowValidationScope.All)
                return base.EndEdit();

            Debug.Assert(RowValidationScope == RowValidationScope.Current);
            ValidateRows();
            var hasError = CurrentRowErrors.Count > 0;
            if (hasError)
                return false;

            RowValidationProgress.Reset();
            return base.EndEdit();
        }

        public bool HasVisibleError
        {
            get
            {
                if (ScalarFlushErrors.Count > 0 || RowFlushErrors.Count > 0)
                    return true;

                for (int i = 0; i < ScalarValidationErrors.Count; i++)
                {
                    var error = ScalarValidationErrors[i];
                    if (ScalarValidationProgress.IsVisible(error.Source))
                        return true;
                }

                foreach (var keyValuePair in RowValidationErrors)
                {
                    var rowPresenter = keyValuePair.Key;
                    var messages = keyValuePair.Value;
                    for (int i = 0; i < messages.Count; i++)
                    {
                        var message = messages[i];
                        if (RowValidationProgress.IsVisible(rowPresenter, message.Source))
                            return true;
                    }
                }

                return false;
            }
        }

        internal sealed override void RollbackEdit()
        {
            base.RollbackEdit();
            if (RowValidationScope == RowValidationScope.Current)
                RowValidationProgress.Reset();
        }
    }
}
