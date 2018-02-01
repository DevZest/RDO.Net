using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class RowValidation
    {
        internal RowValidation(InputManager inputManager)
        {
            _inputManager = inputManager;
            InitInputs();
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

        private FlushingErrorCollection _flushingErrors;
        private FlushingErrorCollection InternalFlushingErrors
        {
            get
            {
                if (_flushingErrors == null)
                    _flushingErrors = new FlushingErrorCollection(_inputManager);
                return _flushingErrors;
            }
        }

        public IReadOnlyList<FlushingError> FlushingErrors
        {
            get
            {
                if (_flushingErrors == null)
                    return Array<FlushingError>.Empty;
                return _flushingErrors;
            }
        }

        internal FlushingError GetFlushingError(UIElement element)
        {
            return _flushingErrors.GetFlushingError(element);
        }

        internal void SetFlushingError(UIElement element, FlushingError value)
        {
            InternalFlushingErrors.SetFlushError(element, value);
        }

        private Dictionary<RowPresenter, IDataValidationErrors> _errorsByRow;
        private Dictionary<RowPresenter, IDataValidationErrors> ErrorsByRow
        {
            get { return _errorsByRow ?? (_errorsByRow = new Dictionary<RowPresenter, IDataValidationErrors>()); }
        }

        private Dictionary<RowPresenter, IDataValidationErrors> _asyncErrorsByRow;
        private Dictionary<RowPresenter, IDataValidationErrors> AsyncErrorsByRow
        {
            get { return _asyncErrorsByRow ?? (_asyncErrorsByRow = new Dictionary<RowPresenter, IDataValidationErrors>()); }
        }

        public IReadOnlyDictionary<RowPresenter, IDataValidationErrors> Errors
        {
            get { return ErrorsByRow; }
        }

        private static IDataValidationErrors GetErrors(Dictionary<RowPresenter, IDataValidationErrors> dictionary, RowPresenter rowPresenter)
        {
            if (dictionary == null)
                return DataValidationErrors.Empty;

            if (dictionary.TryGetValue(rowPresenter, out var result))
                return result;
            else
                return DataValidationErrors.Empty;
        }

        public IDataValidationErrors CurrentRowErrors
        {
            get
            {
                if (CurrentRow == null)
                    return DataValidationErrors.Empty;

                var result = GetErrors(_errorsByRow, CurrentRow);
                var asyncErrors = GetErrors(_asyncErrorsByRow, CurrentRow);
                return Merge(result, asyncErrors).Seal();
            }
        }

        private void ClearErrors(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);
            if (_errorsByRow != null && _errorsByRow.ContainsKey(rowPresenter))
                _errorsByRow.Remove(rowPresenter);
        }

        private void ClearAsyncErrors(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);
            if (_asyncErrorsByRow != null && _asyncErrorsByRow.ContainsKey(rowPresenter))
                _asyncErrorsByRow.Remove(rowPresenter);
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

            if (_errorsByRow != null)
                _errorsByRow.Clear();
            if (_asyncErrorsByRow != null)
                _asyncErrorsByRow.Clear();
            ValidateCurrentRowIfImplicit();
        }

        internal void OnCurrentRowChanged(RowPresenter oldValue)
        {
            Template.RowAsyncValidators.ForEach(x => x.OnCurrentRowChanged());
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
            ClearErrors(rowPresenter);
            var dataRow = rowPresenter.DataRow;
            var errors = Validate(dataRow);
            if (errors != null && errors.Count > 0)
                ErrorsByRow.Add(rowPresenter, errors);
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

            if (valueChanged)
                UpdateAsyncErrors(rowInput.Target);
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
            if (ErrorsByRow.Count == 0)
                return false;

            IDataValidationErrors errors;
            if (!ErrorsByRow.TryGetValue(rowPresenter, out errors))
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

            ClearErrors(rowPresenter);
            ClearAsyncErrors(rowPresenter);
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
            if (Errors.ContainsKey(rowPresenter))
                errors++;
            return errors < errorLimit;
        }

        internal ValidationInfo GetInfo(RowView rowView)
        {
            Debug.Assert(rowView != null);

            var rowPresenter = rowView.RowPresenter;
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (HasError(rowPresenter, Inputs[i], true) || IsValidating(rowPresenter, Inputs[i], true))
                    return ValidationInfo.Empty;
            }

            var errors = GetErrors(ValidationErrors.Empty, rowPresenter, null, false);
            errors = GetErrors(errors, rowPresenter, null, true);
            if (errors.Count > 0)
                return ValidationInfo.Error(errors.Seal());

            foreach (var asyncValidator in AsyncValidators)
            {
                if (asyncValidator.Status == AsyncValidatorStatus.Running)
                    return ValidationInfo.Validating;
            }

            return ValidationInfo.Empty;
        }

        internal ValidationInfo GetInfo(RowPresenter rowPresenter, Input<RowBinding, IColumns> input)
        {
            Debug.Assert(rowPresenter != null);
            Debug.Assert(input != null);

            if (rowPresenter == CurrentRow)
            {
                var flushingError = GetFlushingError(input.Binding[rowPresenter]);
                if (flushingError != null)
                    return ValidationInfo.Error(flushingError);
            }

            if (AnyBlockingErrorInput(rowPresenter, input, true) || AnyBlockingValidatingInput(rowPresenter, input, true))
                return ValidationInfo.Empty;

            var errors = GetErrors(ValidationErrors.Empty, rowPresenter, input.Target, false);
            errors = GetErrors(errors, rowPresenter, input.Target, true);
            if (errors.Count > 0)
                return ValidationInfo.Error(errors.Seal());

            if (IsValidating(rowPresenter, input, null))
                return ValidationInfo.Validating;

            if (!IsVisible(rowPresenter, input.Target) || AnyBlockingErrorInput(rowPresenter, input, false) || AnyBlockingValidatingInput(rowPresenter, input, false))
                return ValidationInfo.Empty;
            else
                return ValidationInfo.Validated;
        }

        private bool AnyBlockingErrorInput(RowPresenter rowPresenter, Input<RowBinding, IColumns> input, bool isPreceding)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (input.Index == i)
                    continue;
                var canBlock = isPreceding ? Inputs[i].IsPrecedingOf(input) : input.IsPrecedingOf(Inputs[i]);
                if (canBlock && HasError(rowPresenter, Inputs[i], null))
                    return true;
            }
            return false;
        }

        internal bool HasError(RowPresenter rowPresenter, Input<RowBinding, IColumns> input, bool? blockingPrecedence)
        {
            if (rowPresenter == CurrentRow)
            {
                var flushingError = GetFlushingError(input.Binding[rowPresenter]);
                if (flushingError != null)
                    return true;
            }

            if (blockingPrecedence.HasValue)
            {
                if (AnyBlockingErrorInput(rowPresenter, input, blockingPrecedence.Value))
                    return false;
            }

            if (HasError(rowPresenter, input.Target, false))
                return true;
            if (HasError(rowPresenter, input.Target, true))
                return true;
            return false;
        }

        private bool HasError(RowPresenter rowPresenter, IColumns source, bool isAsync)
        {
            IReadOnlyDictionary<RowPresenter, IDataValidationErrors> dictionary = isAsync ? _asyncErrorsByRow : _errorsByRow;
            if (dictionary != null)
            {
                IDataValidationErrors errors;
                if (dictionary.TryGetValue(rowPresenter, out errors))
                {
                    if (errors != null && errors.Count > 0 && HasError(rowPresenter, errors, source, !isAsync))
                        return true;
                }
            }

            if (isAsync)
            {
                foreach (var asyncValidator in AsyncValidators)
                {
                    if (asyncValidator.GetFault(source) != null)
                        return true;
                }
            }
            return false;
        }

        private bool HasError(RowPresenter rowPresenter, IDataValidationErrors messages, IColumns columns, bool ensureVisible)
        {
            Debug.Assert(messages != null);

            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (ensureVisible && !IsVisible(rowPresenter, message.Source))
                    continue;
                if (columns == null || columns.IsSupersetOf(message.Source))
                    return true;
            }

            return false;
        }

        private IValidationErrors GetErrors(IValidationErrors result, RowPresenter rowPresenter, IColumns source, bool isAsync)
        {
            IReadOnlyDictionary<RowPresenter, IDataValidationErrors> dictionary = isAsync ? _asyncErrorsByRow : _errorsByRow;
            if (dictionary != null)
            {
                IDataValidationErrors errors;
                if (dictionary.TryGetValue(rowPresenter, out errors))
                {
                    if (errors != null && errors.Count > 0)
                        result = GetErrors(result, rowPresenter, errors, source, !isAsync);
                }
            }

            if (isAsync)
            {
                foreach (var asyncValidator in AsyncValidators)
                {
                    var fault = asyncValidator.GetFault(source);
                    if (fault != null)
                        result = result.Add(fault);
                }
            }

            return result;
        }

        private IValidationErrors GetErrors(IValidationErrors result, RowPresenter rowPresenter, IDataValidationErrors messages, IColumns columns, bool ensureVisible)
        {
            Debug.Assert(messages != null);

            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (ensureVisible && !IsVisible(rowPresenter, message.Source))
                    continue;
                if (columns == null || columns.IsSupersetOf(message.Source))
                    result = result.Add(message);
            }

            return result;
        }

        public void SetAsyncErrors(IDataValidationResults validationResults)
        {
            if (_asyncErrorsByRow != null)
                _asyncErrorsByRow.Clear();

            for (int i = 0; i < validationResults.Count; i++)
            {
                var entry = validationResults[i];
                var rowPresenter = _inputManager[entry.DataRow];
                if (rowPresenter != null)
                    AsyncErrorsByRow.Add(rowPresenter, entry.Errors);
            }
            InvalidateView();
        }

        private void UpdateAsyncErrors(IColumns changedColumns)
        {
            Debug.Assert(CurrentRow != null);
            var errors = GetErrors(_asyncErrorsByRow, CurrentRow);
            errors = Remove(errors, x => x.Source.Overlaps(changedColumns));
            UpdateAsyncErrors(CurrentRow, errors);
        }

        internal void UpdateAsyncErrors(RowAsyncValidator rowAsyncValidator)
        {
            Debug.Assert(CurrentRow != null);

            var sourceColumns = rowAsyncValidator.SourceColumns;
            var errors = GetErrors(_asyncErrorsByRow, CurrentRow);
            errors = Remove(errors, x => !x.Source.SetEquals(sourceColumns));
            errors = Merge(errors, rowAsyncValidator.Results);
            UpdateAsyncErrors(CurrentRow, errors);
        }

        private void UpdateAsyncErrors(RowPresenter rowPresenter, IDataValidationErrors errors)
        {
            ClearAsyncErrors(rowPresenter);
            if (errors.Count > 0)
                AsyncErrorsByRow.Add(rowPresenter, errors);
        }

        private static IDataValidationErrors Merge(IDataValidationErrors result, IDataValidationErrors errors)
        {
            return Merge(result, errors, errors.Count);
        }

        private static IDataValidationErrors Merge(IDataValidationErrors result, IDataValidationErrors errors, int count)
        {
            for (int i = 0; i < count; i++)
                result = result.Add(errors[i]);
            return result;
        }

        private static IDataValidationErrors Remove(IDataValidationErrors errors, Predicate<DataValidationError> predicate)
        {
            var result = errors;

            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                if (predicate(error))
                {
                    if (result != errors)
                        result = result.Add(error);
                    else
                        result = Merge(DataValidationErrors.Empty, errors, i);
                }
            }
            return result;
        }

        private Input<RowBinding, IColumns>[] _inputs;
        public IReadOnlyList<Input<RowBinding, IColumns>> Inputs
        {
            get { return _inputs; }
        }

        private void InitInputs()
        {
            _inputs = GetInputs(Template.RowBindings).ToArray();
            for (int i = 0; i < Inputs.Count; i++)
                Inputs[i].Index = i;
        }

        private static IEnumerable<Input<RowBinding, IColumns>> GetInputs(IReadOnlyList<RowBinding> rowBindings)
        {
            if (rowBindings == null)
                yield break;
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var rowInput = rowBinding.RowInput;
                if (rowInput != null)
                    yield return rowInput;

                foreach (var childInput in GetInputs(rowBinding.ChildBindings))
                    yield return childInput;
            }
        }

        private bool AnyBlockingValidatingInput(RowPresenter rowPresenter, Input<RowBinding, IColumns> input, bool isPreceding)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (input.Index == i)
                    continue;
                var canBlock = isPreceding ? Inputs[i].IsPrecedingOf(input) : input.IsPrecedingOf(Inputs[i]);
                if (canBlock && IsValidating(rowPresenter, Inputs[i], null))
                    return true;
            }
            return false;
        }

        internal bool IsValidating(RowPresenter rowPresenter, Input<RowBinding, IColumns> input, bool? blockingPrecedence)
        {
            if (blockingPrecedence.HasValue)
            {
                if (AnyBlockingValidatingInput(rowPresenter, input, blockingPrecedence.Value))
                    return false;
            }

            foreach (var asyncValidator in AsyncValidators)
            {
                if (asyncValidator.Status == AsyncValidatorStatus.Running && input.Target.IsSupersetOf(asyncValidator.SourceColumns))
                    return true;
            }
            return false;
        }
    }
}
