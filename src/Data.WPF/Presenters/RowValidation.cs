using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class RowValidation
    {
        internal RowValidation(InputManager inputManager)
        {
            _inputManager = inputManager;
            if (Mode == ValidationMode.Progressive)
            {
                _progress = new Dictionary<RowPresenter, IColumns>();
                _valueChanged = new Dictionary<RowPresenter, IColumns>();
            }
            ValidateCurrentRowIfImplicit();
        }

        private readonly InputManager _inputManager;

        private DataPresenter DataPresenter
        {
            get { return _inputManager.DataPresenter; }
        }

        private DataSet DataSet
        {
            get { return _inputManager.DataSet; }
        }

        internal void InvalidateView()
        {
            _validationErrors = _validationErrors.Seal();
            _validationWarnings = _validationWarnings.Seal();
            _inputManager.InvalidateView();
        }

        private Template Template
        {
            get { return _inputManager.Template; }
        }

        public IRowAsyncValidators AsyncValidators
        {
            get { return Template.RowAsyncValidators; }
        }

        private RowPresenter CurrentRow
        {
            get { return _inputManager.CurrentRow; }
        }

        private FlushErrorCollection _flushErrors;
        private FlushErrorCollection InternalFlushErrors
        {
            get
            {
                if (_flushErrors == null)
                    _flushErrors = new FlushErrorCollection(_inputManager);
                return _flushErrors;
            }
        }

        public IReadOnlyList<FlushErrorMessage> FlushErrors
        {
            get
            {
                if (_flushErrors == null)
                    return Array<FlushErrorMessage>.Empty;
                return _flushErrors;
            }
        }

        internal FlushErrorMessage GetFlushError(UIElement element)
        {
            return _flushErrors.GetFlushError(element);
        }

        internal void SetFlushError(UIElement element, FlushErrorMessage value)
        {
            InternalFlushErrors.SetFlushError(element, value);
        }

        private IRowValidationResults _validationErrors = RowValidationResults.Empty;
        public IRowValidationResults _validationWarnings = RowValidationResults.Empty;

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> ValidationErrors
        {
            get { return _validationErrors; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> ValidationWarnings
        {
            get { return _validationWarnings; }
        }

        public IColumnValidationMessages CurrentRowErrors
        {
            get { return _validationErrors.GetValidationMessages(CurrentRow); }
        }

        public IColumnValidationMessages CurrentRowWarnings
        {
            get { return _validationWarnings.GetValidationMessages(CurrentRow); }
        }

        private void ClearRowValidationMessages(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);
            _validationErrors = _validationErrors.Remove(rowPresenter);
            _validationWarnings = _validationWarnings.Remove(rowPresenter);
        }

        private void ValidateCurrentRowIfImplicit()
        {
            if (Mode == ValidationMode.Implicit)
                ValidateCurrentRow();
        }

        internal void ValidateCurrentRow()
        {
            if (CurrentRow != null)
            {
                Validate(CurrentRow, true);
                InvalidateView();
            }
        }

        private Dictionary<RowPresenter, IColumns> _progress;
        private Dictionary<RowPresenter, IColumns> _valueChanged;
        private IColumns _beginEditProgress;
        private IColumns _beginEditValueChanged;

        internal void OnReloaded()
        {
            if (_progress != null)
            {
                _progress.Clear();
                _valueChanged.Clear();
            }

            _validationErrors = _validationWarnings = RowValidationResults.Empty;
            ValidateCurrentRowIfImplicit();
            DataPresenter?.OnRowsLoaded(true);
        }

        internal void OnCurrentRowChanged(RowPresenter oldValue)
        {
            Template.RowAsyncValidators.Each(x => x.OnCurrentRowChanged());
            ValidateCurrentRowIfImplicit();
            DataPresenter?.OnCurrentRowChanged(oldValue);
        }

        internal void EnterEdit()
        {
            if (_progress != null)
            {
                _beginEditProgress = GetProgress(_progress, CurrentRow);
                _beginEditValueChanged = GetProgress(_valueChanged, CurrentRow);
            }
        }

        internal void CancelEdit()
        {
            if (_progress != null)
            {
                Restore(_beginEditProgress, _progress, CurrentRow);
                Restore(_beginEditValueChanged, _valueChanged, CurrentRow);
                ExitEdit();
            }
        }

        private static void Restore(IColumns beginEditProgress, Dictionary<RowPresenter, IColumns> progress, RowPresenter currentRow)
        {
            if (beginEditProgress == Columns.Empty && progress.ContainsKey(currentRow))
                progress.Remove(currentRow);
            else
                progress[currentRow] = beginEditProgress;
        }

        internal void ExitEdit()
        {
            _beginEditProgress = _beginEditValueChanged = null;
        }

        internal void Validate(RowPresenter rowPresenter, bool showAll)
        {
            Debug.Assert(rowPresenter != null);
            if (showAll)
                ShowAll(rowPresenter);
            _validationErrors = _validationErrors.Remove(rowPresenter);
            _validationWarnings = _validationWarnings.Remove(rowPresenter);
            var dataRow = rowPresenter.DataRow;
            var errors = Validate(dataRow, ValidationSeverity.Error);
            var warnings = Validate(dataRow, ValidationSeverity.Warning);
            if (errors != null && errors.Count > 0)
                _validationErrors = _validationErrors.Add(rowPresenter, errors);
            if (warnings != null && warnings.Count > 0)
                _validationWarnings = _validationWarnings.Add(rowPresenter, warnings);
        }

        private IColumnValidationMessages Validate(DataRow dataRow, ValidationSeverity? severity)
        {
            return dataRow == DataSet.AddingRow ? DataSet.ValidateAddingRow(severity) : dataRow.Validate(severity);
        }

        internal void OnFlushed<T>(RowInput<T> rowInput, bool makeProgress, bool valueChanged)
            where T : UIElement, new()
        {
            if (!makeProgress && !valueChanged)
                return;

            if (Mode != ValidationMode.Explicit)
                Validate(CurrentRow, false);
            if (UpdateProgress(rowInput, valueChanged, makeProgress))
                OnProgress(rowInput);
            InvalidateView();
        }

        private void OnProgress<T>(RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (Mode == ValidationMode.Explicit)
                return;

            if (HasError(CurrentRow, rowInput.Target))
                return;

            var asyncValidators = Template.RowAsyncValidators;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                var sourceColumns = asyncValidator.SourceColumns;
                if (sourceColumns.Overlaps(rowInput.Target) && IsVisible(CurrentRow, sourceColumns))
                    asyncValidator.Run();
            }
        }

        private bool HasError(RowPresenter rowPresenter, IColumns columns)
        {
            if (_validationErrors.Count == 0)
                return false;

            IColumnValidationMessages messages;
            if (!_validationErrors.TryGetValue(rowPresenter, out messages))
                return false;

            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(columns))
                    return true;
            }

            return false;
        }

        internal bool UpdateProgress<T>(RowInput<T> rowInput, bool valueChanged, bool makeProgress)
            where T : UIElement, new()
        {
            Debug.Assert(valueChanged || makeProgress);

            if (_progress == null)
                return valueChanged;

            var currentRow = _inputManager.CurrentRow;
            Debug.Assert(currentRow != null);
            var sourceColumns = rowInput.Target;
            if (sourceColumns == null || sourceColumns.Count == 0)
                return false;

            if (makeProgress)
            {
                var columns = GetProgress(_progress, currentRow);
                if (columns == null)
                    return valueChanged;
                if (valueChanged || Exists(_valueChanged, currentRow, sourceColumns))
                {
                    _progress[currentRow] = columns.Union(sourceColumns);
                    return true;
                }
            }
            else
                _valueChanged[currentRow] = GetProgress(_valueChanged, currentRow).Union(sourceColumns);

            return false;
        }

        internal void ShowAll(RowPresenter rowPresenter)
        {
            if (_progress == null)
                return;

            if (_valueChanged.ContainsKey(rowPresenter))
                _valueChanged.Remove(rowPresenter);
            _progress[rowPresenter] = null;
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (_progress == null)
                return;

            if (_progress.ContainsKey(rowPresenter))
                _progress.Remove(rowPresenter);
            if (_valueChanged.ContainsKey(rowPresenter))
                _valueChanged.Remove(rowPresenter);

            if (_validationErrors.ContainsKey(rowPresenter))
                _validationErrors = _validationErrors.Remove(rowPresenter).Seal();

            if (_validationWarnings.ContainsKey(rowPresenter))
                _validationWarnings = _validationWarnings.Remove(rowPresenter).Seal();

            Template.RowAsyncValidators.Each(x => x.OnRowDisposed(rowPresenter));
        }

        public ValidationMode Mode
        {
            get { return Template.RowValidationMode; }
        }

        public bool IsVisible(RowPresenter rowPresenter, IColumns columns)
        {
            Check.NotNull(rowPresenter, nameof(rowPresenter));

            if (columns == null || columns.Count == 0)
                return false;

            if (_progress == null)
                return true;

            return Exists(_progress, rowPresenter, columns);
        }

        private static bool Exists(Dictionary<RowPresenter, IColumns> progressByRow, RowPresenter rowPresenter, IColumns columns)
        {
            Debug.Assert(progressByRow != null);
            Debug.Assert(rowPresenter != null);
            Debug.Assert(columns != null && columns.Count > 0);

            var progress = GetProgress(progressByRow, rowPresenter);
            return progress == null ? true : progress.IsSupersetOf(columns);
        }

        private static IColumns GetProgress(Dictionary<RowPresenter, IColumns> progressByRow, RowPresenter rowPresenter)
        {
            Debug.Assert(progressByRow != null);
            IColumns result;
            if (progressByRow.TryGetValue(rowPresenter, out result))
                return result;
            return Columns.Empty;
        }

        public void FlushCurrentRow()
        {
            if (CurrentRow != null && CurrentRow.View != null)
                CurrentRow.View.Flush();
        }

        public void Validate(int errorLimit = 1, int warningLimit = 0)
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
                foreach (var rowPresenter in _inputManager.Rows)
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
            if (ValidationErrors.ContainsKey(rowPresenter))
                errors++;
            if (ValidationWarnings.ContainsKey(rowPresenter))
                warnings++;
            return errors < errorLimit || warnings < warningLimit;
        }

        internal IColumnValidationMessages GetValidationMessages(RowPresenter rowPresenter, IColumns source, ValidationSeverity severity)
        {
            Debug.Assert(source != null && source.Count > 0);

            if (!IsVisible(rowPresenter, source))
                return ColumnValidationMessages.Empty;

            var validationMessages = severity == ValidationSeverity.Error ? _validationErrors : _validationWarnings;
            IColumnValidationMessages messages;
            if (!validationMessages.TryGetValue(rowPresenter, out messages))
                return ColumnValidationMessages.Empty;

            if (messages == null || messages.Count == 0 || ExistsAnySubsetSourceColumns(messages, source))
                return ColumnValidationMessages.Empty;

            return GetValidationMessages(messages, source);
        }

        private static IColumnValidationMessages GetValidationMessages(IColumnValidationMessages messages, IColumns columns)
        {
            Debug.Assert(messages != null);

            var result = ColumnValidationMessages.Empty;
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(columns))
                    result = result.Add(message);
            }

            return result;
        }

        private static bool ExistsAnySubsetSourceColumns(IColumnValidationMessages messages, IColumns columns)
        {
            if (columns.Count == 1)
                return false;

            for (int i = 0; i < messages.Count; i++)
            {
                var source = messages[i].Source;
                if (columns.IsProperSupersetOf(source))
                    return true;
            }

            return false;
        }

        private IRowValidationResults ToRowValidationResults(IDataRowValidationResults validationResults)
        {
            var result = RowValidationResults.Empty;
            for (int i = 0; i < validationResults.Count; i++)
            {
                var entry = validationResults[i];
                var rowPresenter = _inputManager[entry.DataRow];
                if (rowPresenter != null)
                    result = result.Add(rowPresenter, entry.Messages);
            }
            return result;
        }
    }
}
