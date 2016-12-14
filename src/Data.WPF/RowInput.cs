using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowInput<T> : Input<T>
        where T : UIElement, new()
    {
        internal static RowInput<T> Create<TData>(Trigger<T> flushTrigger, Func<Column<TData>> getColumn, Func<T, TData> getValue)
        {
            return new RowInput<T>(flushTrigger).Bind(getColumn, getValue);
        }
            

        private RowInput(Trigger<T> flushTrigger)
            : base(flushTrigger)
        {
        }

        private IColumnSet _columns = ColumnSet.Empty;
        private List<Func<Column>> _getColumnFuncs = new List<Func<Column>>();
        private List<Func<RowPresenter, T, bool>> _flushFuncs = new List<Func<RowPresenter, T, bool>>();
        private HashSet<RowPresenter> _progress;
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _validationErrors = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _validationWarnings = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();
        private Func<RowPresenter, Task<ValidationMessage>> _asyncValidator;
        private HashSet<RowPresenter> _pendingAsyncValidators;
        private Dictionary<RowPresenter, AsyncValidationState> _asyncValidationStates;
        private Dictionary<RowPresenter, ValidationMessage> _asyncValidationMessages;

        private ValidationMode ValidationMode
        {
            get { return Template.ValidationMode; }
        }

        private ValidationScope ValidationScope
        {
            get { return Template.ValidationScope; }
        }

        internal override void Seal(Binding binding)
        {
            base.Seal(binding);
            if (ValidationMode == ValidationMode.Progressive)
                _progress = new HashSet<RowPresenter>();
            if (_asyncValidator != null)
            {
                _pendingAsyncValidators = new HashSet<RowPresenter>();
                _asyncValidationStates = new Dictionary<RowPresenter, AsyncValidationState>();
                _asyncValidationMessages = new Dictionary<RowPresenter, ValidationMessage>();
            }
        }

        internal void Reset(bool dataReloaded)
        {
            if (dataReloaded)
            {
                _columns = ColumnSet.Empty;
                foreach (var getColumn in _getColumnFuncs)
                    _columns = _columns.Merge(getColumn());
            }

            _validationErrors.Clear();
            _validationWarnings.Clear();

            if (ValidationMode == ValidationMode.Progressive && _progress == null)
                _progress = new HashSet<RowPresenter>();
            else if (_progress != null)
                _progress.Clear();

            if (_asyncValidator != null)
            {
                _asyncValidationMessages.Clear();
                _asyncValidationStates.Clear();
                _pendingAsyncValidators.Clear();
            }
        }

        private void MakeProgress()
        {
            var currentRow = CurrentRow;
            Debug.Assert(currentRow != null);
            if (_progress != null && !_progress.Contains(currentRow))
                _progress.Add(currentRow);
        }

        internal void BypassProgressiveMode()
        {
            _progress = null;
        }

        internal void OnCurrentRowChanged()
        {
            if (_progress != null)
            {
                if (ValidationScope == ValidationScope.CurrentRow)
                    _progress.Clear();
            }
            else if (ValidationMode == ValidationMode.Progressive)
                _progress = new HashSet<RowPresenter>();
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (_progress != null && _progress.Contains(rowPresenter))
                _progress.Remove(rowPresenter);

            if (_asyncValidator != null)
            {
                _asyncValidationStates.Remove(rowPresenter);
                _asyncValidationMessages.Remove(rowPresenter);
                _pendingAsyncValidators.Remove(rowPresenter);
            }
        }

        private RowPresenter CurrentRow
        {
            get { return ValidationManager == null ? null : ValidationManager.CurrentRow; }
        }

        internal void SetValidationResult(RowValidationResult errors, RowValidationResult warnings)
        {
            if (_progress != null && _progress.Count == 0)
                return;

            bool errorsChanged = SetValidationResult(_validationErrors, errors);
            bool warningsChanged = SetValidationResult(_validationWarnings, warnings);
            if (errorsChanged || warningsChanged)
                ValidationManager.InvalidateElements();
        }

        private bool SetValidationResult(Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> messages, RowValidationResult validationResult)
        {
            bool oldIsEmpty = messages.Count > 0;
            messages.Clear();
            foreach (var entry in validationResult.Entries)
            {
                if (_progress != null && !_progress.Contains(entry.Row))
                    continue;
                var filteredMessages = Filter(entry.Messages);
                if (filteredMessages.Count > 0)
                    messages.Add(entry.Row, filteredMessages);
            }

            return !oldIsEmpty || messages.Count > 0;
        }

        private IReadOnlyList<ValidationMessage> Filter(IReadOnlyList<ValidationMessage<IColumnSet>> messages)
        {
            List<ValidationMessage> result = null;
            foreach (var message in messages)
            {
                throw new NotImplementedException();
                //if (message.Source.IntersectsWith(Columns))
                //    result = result.AddItem(new ValidationMessage(message.Id, message.Description, message.Severity));
            }

            return result.ToReadOnlyList();
        }

        internal IReadOnlyList<ValidationMessage> GetErrors(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);

            List<ValidationMessage> result = null;

            if (rowPresenter == CurrentRow && HasPreValidatorError)
                result = result.AddItem(PreValidatorError);
            result = result.AddItems(_validationErrors.GetValues(rowPresenter))
                .AddValidationMessage(GetAsyncValidationMessage(rowPresenter), ValidationSeverity.Error);

            return result.ToReadOnlyList();
        }

        internal IReadOnlyList<ValidationMessage> GetWarnings(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);

            List<ValidationMessage> result = null;
            result = result.AddItems(_validationWarnings.GetValues(rowPresenter))
                .AddValidationMessage(GetAsyncValidationMessage(rowPresenter), ValidationSeverity.Warning);

            return result.ToReadOnlyList();
        }

        public RowInput<T> WithPreValidator(Func<T, string> preValidator, Trigger<T> preValidatorTrigger)
        {
            SetPreValidator(preValidator, preValidatorTrigger);
            return this;
        }

        public RowInput<T> WithAsyncValidator(Func<RowPresenter, Task<ValidationMessage>> asyncValidator)
        {
            VerifyNotSealed();
            _asyncValidator = asyncValidator;
            return this;
        }

        public RowInput<T> Bind<TData>(Func<Column<TData>> getColumn, Func<T, TData> getValue)
        {
            if (getColumn == null)
                throw new ArgumentNullException(nameof(getColumn));

            VerifyNotSealed();
            _getColumnFuncs.Add(getColumn);
            _flushFuncs.Add((rowPresenter, element) =>
            {
                if (getValue == null)
                    return false;
                var column = getColumn();
                var value = getValue(element);
                if (column.AreEqual(rowPresenter.GetValue(column), value))
                    return false;
                rowPresenter.EditValue(column, getValue(element));
                return true;
            });
            return this;
        }

        internal IColumnSet Columns
        {
            get { return _columns; }
        }

        internal override void FlushCore(T element)
        {
            Debug.Assert(CurrentRow != null && CurrentRow == element.GetRowPresenter());
            var currentRow = CurrentRow;
            var flushed = DoFlush(currentRow, element);
            if (flushed)
            {
                MakeProgress();
                if (ValidationMode != ValidationMode.Explicit)
                    ValidationManager.Validate(false);
                if (_asyncValidator != null)
                    AsyncValidate(currentRow);
                ValidationManager.InvalidateElements();
            }
        }

        private bool DoFlush(RowPresenter rowPresenter, T element)
        {
            bool result = false;
            foreach (var flush in _flushFuncs)
            {
                var flushed = flush(rowPresenter, element);
                if (flushed)
                    result = true;
            }
            return result;
        }

        private async void AsyncValidate(RowPresenter rowPresenter)
        {
            Debug.Assert(_asyncValidator != null);

            var state = GetAsyncValidationState(rowPresenter);
            if (state == AsyncValidationState.Running)
            {
                AddPendingAsyncValidator(rowPresenter);
                return;
            }

            ValidationMessage message;
            do
            {
                var task = _asyncValidator(rowPresenter);
                SetAsyncValidationState(rowPresenter, AsyncValidationState.Running);
                try
                {
                    message = await task;
                    state = message.IsEmpty ? AsyncValidationState.Valid : AsyncValidationState.Invalid;
                }
                catch (Exception ex)
                {
                    message = ValidationMessage.Error(ex.Message);
                    state = AsyncValidationState.Failed;
                }
            }
            while (RemovePendingAsyncValidator(rowPresenter));

            if (!_asyncValidationStates.ContainsKey(rowPresenter))
                return;

            SetAsyncValidationState(rowPresenter, state);
            SetAsyncValidationMessage(rowPresenter, message);
            ValidationManager.InvalidateElements();
        }

        internal AsyncValidationState GetAsyncValidationState(RowPresenter rowPresenter)
        {
            if (_asyncValidationStates == null)
                return AsyncValidationState.NotRunning;

            AsyncValidationState result;
            if (_asyncValidationStates.TryGetValue(rowPresenter, out result))
                return result;

            return AsyncValidationState.NotRunning;
        }

        private void SetAsyncValidationState(RowPresenter rowPresenter, AsyncValidationState state)
        {
            Debug.Assert(_asyncValidationStates != null);

            if (state == AsyncValidationState.NotRunning)
            {
                if (_asyncValidationStates.ContainsKey(rowPresenter))
                    _asyncValidationStates.Remove(rowPresenter);
            }
            else
                _asyncValidationStates[rowPresenter] = state;
        }

        private ValidationMessage GetAsyncValidationMessage(RowPresenter rowPresenter)
        {
            Debug.Assert(_asyncValidationMessages != null);

            ValidationMessage result;
            if (_asyncValidationMessages.TryGetValue(rowPresenter, out result))
                return result;
            return ValidationMessage.Empty;
        }

        private void SetAsyncValidationMessage(RowPresenter rowPresenter, ValidationMessage message)
        {
            Debug.Assert(_asyncValidationMessages != null);

            if (message.IsEmpty)
            {
                if (_asyncValidationMessages.ContainsKey(rowPresenter))
                    _asyncValidationMessages.Remove(rowPresenter);
            }
            else
                _asyncValidationMessages[rowPresenter] = message;
        }

        private void AddPendingAsyncValidator(RowPresenter rowPresenter)
        {
            Debug.Assert(_pendingAsyncValidators != null);
            _pendingAsyncValidators.Add(rowPresenter);
        }

        private bool RemovePendingAsyncValidator(RowPresenter rowPresenter)
        {
            Debug.Assert(_pendingAsyncValidators != null);
            return _pendingAsyncValidators.Remove(rowPresenter);
        }

        internal bool CanRetryAsyncValidation
        {
            get { return _asyncValidator != null; }
        }

        internal void RetryAsyncValidation(RowPresenter rowPresenter)
        {
            Debug.Assert(CanRetryAsyncValidation);
            AsyncValidate(rowPresenter);
        }

        internal void Refresh(T element, RowPresenter rowPresenter)
        {
            element.SetDataErrorInfo(GetErrors(rowPresenter), GetWarnings(rowPresenter));
        }
    }
}
