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

        private InputManager _inputManager;
        private Dictionary<RowPresenter, IColumns> _progress;
        private Dictionary<RowPresenter, IColumns> _valueChanged;
        private IColumns _beginEditProgress;
        private IColumns _beginEditValueChanged;

        internal void Reset()
        {
            if (_progress != null)
            {
                _progress.Clear();
                _valueChanged.Clear();
            }
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
        }

        public ValidationMode Mode
        {
            get { return _inputManager.RowValidationMode; }
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

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> Errors
        {
            get { return _inputManager.RowValidationErrors; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> Warnings
        {
            get { return _inputManager.RowValidationWarnings; }
        }

        public IColumnValidationMessages CurrentRowErrors
        {
            get { return _inputManager.CurrentRowErrors; }
        }

        public IColumnValidationMessages CurrentRowWarnings
        {
            get { return _inputManager.CurrentRowWarnings; }
        }

        private Template Template
        {
            get { return _inputManager.Template; }
        }

        public IRowAsyncValidators AsyncValidators
        {
            get { return Template.RowAsyncValidators; }
        }

        public void Assign(IDataRowValidationResults validationResults)
        {
            if (validationResults == null)
                throw new ArgumentNullException(nameof(validationResults));
            _inputManager.Assign(validationResults);
        }

        public void Assign(IRowValidationResults validationResults)
        {
            if (validationResults == null)
                throw new ArgumentNullException(nameof(validationResults));
            _inputManager.Assign(validationResults);
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> AssignedResults
        {
            get { return _inputManager.AssignedRowValidationResults; }
        }

        private RowPresenter CurrentRow
        {
            get { return _inputManager.CurrentRow; }
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

            _inputManager.InvalidateView();
        }

        private bool Validate(RowPresenter rowPresenter, ref int errors, int errorLimit, ref int warnings, int warningLimit)
        {
            Debug.Assert(rowPresenter != null);
            rowPresenter.Validate(false);
            if (Errors.ContainsKey(rowPresenter))
                errors++;
            if (Warnings.ContainsKey(rowPresenter))
                warnings++;
            return errors < errorLimit || warnings < warningLimit;
        }

        internal IColumnValidationMessages GetValidationMessages(RowPresenter rowPresenter, IColumns source, ValidationSeverity severity)
        {
            Debug.Assert(source != null && source.Count > 0);

            if (!IsVisible(rowPresenter, source))
                return ColumnValidationMessages.Empty;

            var validationMessages = severity == ValidationSeverity.Error ? _inputManager.RowValidationErrors : _inputManager.RowValidationWarnings;
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
    }
}
