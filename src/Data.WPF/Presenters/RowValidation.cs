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

        private void InvalidateView()
        {
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

        public IReadOnlyList<FlushError> FlushErrors
        {
            get
            {
                if (_flushErrors == null)
                    return Array<FlushError>.Empty;
                return _flushErrors;
            }
        }

        internal FlushError GetFlushError(UIElement element)
        {
            return _flushErrors.GetFlushError(element);
        }

        internal void SetFlushError(UIElement element, FlushError value)
        {
            InternalFlushErrors.SetFlushError(element, value);
        }

        private IRowValidationResults _validationErrors = RowValidationResults.Empty;

        public IReadOnlyDictionary<RowPresenter, IDataValidationErrors> ValidationErrors
        {
            get { return _validationErrors; }
        }

        public IDataValidationErrors CurrentRowErrors
        {
            get { return _validationErrors.GetValidationMessages(CurrentRow); }
        }

        private void ClearRowValidationMessages(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);
            _validationErrors = _validationErrors.Remove(rowPresenter);
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

            _validationErrors = RowValidationResults.Empty;
            ValidateCurrentRowIfImplicit();
        }

        internal void OnCurrentRowChanged(RowPresenter oldValue)
        {
            Template.RowAsyncValidators.Each(x => x.OnCurrentRowChanged());
            ValidateCurrentRowIfImplicit();
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
            var dataRow = rowPresenter.DataRow;
            var errors = Validate(dataRow);
            if (errors != null && errors.Count > 0)
                _validationErrors = _validationErrors.Add(rowPresenter, errors);
        }

        private IDataValidationErrors Validate(DataRow dataRow)
        {
            return dataRow == DataSet.AddingRow ? DataSet.ValidateAddingRow() : dataRow.Validate();
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

            IDataValidationErrors errors;
            if (!_validationErrors.TryGetValue(rowPresenter, out errors))
                return false;

            for (int i = 0; i < errors.Count; i++)
            {
                var message = errors[i];
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
            if (_progress != null)
            {
                if (_progress.ContainsKey(rowPresenter))
                    _progress.Remove(rowPresenter);
                if (_valueChanged.ContainsKey(rowPresenter))
                    _valueChanged.Remove(rowPresenter);
            }

            if (_validationErrors.ContainsKey(rowPresenter))
                _validationErrors = _validationErrors.Remove(rowPresenter).Seal();

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

        public void Validate(int errorLimit = 1)
        {
            if (errorLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(errorLimit));

            if (CurrentRow == null)
                return;

            var errors = 0;
            var moreToValidate = Validate(CurrentRow, ref errors, errorLimit);
            if (moreToValidate)
            {
                foreach (var rowPresenter in _inputManager.Rows)
                {
                    if (rowPresenter == CurrentRow || rowPresenter.IsVirtual)
                        continue;

                    moreToValidate = Validate(rowPresenter, ref errors, errorLimit);
                    if (!moreToValidate)
                        break;
                }
            }

            InvalidateView();
        }

        private bool Validate(RowPresenter rowPresenter, ref int errors, int errorLimit)
        {
            Debug.Assert(rowPresenter != null);
            rowPresenter.Validate(false);
            if (ValidationErrors.ContainsKey(rowPresenter))
                errors++;
            return errors < errorLimit;
        }

        internal IDataValidationErrors GetValidationErrors(RowPresenter rowPresenter, IColumns source)
        {
            Debug.Assert(source != null && source.Count > 0);

            if (!IsVisible(rowPresenter, source))
                return DataValidationErrors.Empty;

            IDataValidationErrors errors;
            if (!_validationErrors.TryGetValue(rowPresenter, out errors))
                return DataValidationErrors.Empty;

            if (errors == null || errors.Count == 0 || ExistsAnySubsetSourceColumns(errors, source))
                return DataValidationErrors.Empty;

            return GetValidationErrors(errors, source);
        }

        private static IDataValidationErrors GetValidationErrors(IDataValidationErrors messages, IColumns columns)
        {
            Debug.Assert(messages != null);

            var result = DataValidationErrors.Empty;
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(columns))
                    result = result.Add(message);
            }

            return result;
        }

        private static bool ExistsAnySubsetSourceColumns(IDataValidationErrors errors, IColumns columns)
        {
            if (columns.Count == 1)
                return false;

            for (int i = 0; i < errors.Count; i++)
            {
                var source = errors[i].Source;
                if (columns.IsProperSupersetOf(source))
                    return true;
            }

            return false;
        }

        private IRowValidationResults ToRowValidationResults(IDataValidationResults validationResults)
        {
            var result = RowValidationResults.Empty;
            for (int i = 0; i < validationResults.Count; i++)
            {
                var entry = validationResults[i];
                var rowPresenter = _inputManager[entry.DataRow];
                if (rowPresenter != null)
                    result = result.Add(rowPresenter, entry.Errors);
            }
            return result;
        }
    }
}
