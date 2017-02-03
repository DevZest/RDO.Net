using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowInput<T> : Input<T>
        where T : UIElement, new()
    {
        internal static RowInput<T> Create<TData>(Trigger<T> flushTrigger, Column<TData> column, Func<T, TData> getValue)
        {
            return new RowInput<T>(flushTrigger).Bind(column, getValue);
        }
            

        private RowInput(Trigger<T> flushTrigger)
            : base(flushTrigger)
        {
        }

        public RowBinding<T> RowBinding { get; private set; }

        public sealed override TwoWayBinding Binding
        {
            get { return RowBinding; }
        }

        internal void Seal(RowBinding<T> rowBinding)
        {
            Debug.Assert(rowBinding != null);
            VerifyNotSealed();
            RowBinding = rowBinding;
        }

        internal IColumnSet SourceColumns { get; private set; } = ColumnSet.Empty;
        private List<Func<RowPresenter, T, bool>> _flushFuncs = new List<Func<RowPresenter, T, bool>>();

        private void MakeProgress()
        {
            var currentRow = CurrentRow;
            Debug.Assert(currentRow != null);
            ValidationManager.MakeProgress(currentRow, this);
        }

        private RowPresenter CurrentRow
        {
            get { return ValidationManager == null ? null : ValidationManager.CurrentRow; }
        }

        public RowInput<T> WithInputValidator(Func<T, InputError> inputValidaitor, Trigger<T> inputValidationTrigger)
        {
            SetInputValidator(inputValidaitor, inputValidationTrigger);
            return this;
        }

        public RowInput<T> Bind<TData>(Column<TData> column, Func<T, TData> getValue)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            VerifyNotSealed();
            SourceColumns = SourceColumns.Union(column);
            _flushFuncs.Add((Func<RowPresenter, T, bool>)((rowPresenter, element) =>
            {
                var value = getValue(element);
                if (column.AreEqual(rowPresenter.GetValue((Column<TData>)column), value))
                    return false;
                rowPresenter.EditValue((Column<TData>)column, getValue(element));
                return true;
            }));
            return this;
        }

        internal override void FlushCore(T element)
        {
            Debug.Assert(CurrentRow != null && CurrentRow == element.GetRowPresenter());
            var currentRow = CurrentRow;
            var flushed = DoFlush(currentRow, element);
            if (flushed)
                MakeProgress();
        }

        private bool DoFlush(RowPresenter rowPresenter, T element)
        {
            bool result = false;
            foreach (var flush in _flushFuncs)
            {
                if (flush == null)
                    return true;

                var flushed = flush(rowPresenter, element);
                if (flushed)
                    result = true;
            }
            return result;
        }

        private Func<RowPresenter, Task<ValidationMessage>> _asyncValidator;
        private HashSet<RowPresenter> _pendingAsyncValidators;
        private Dictionary<RowPresenter, AsyncValidationState> _asyncValidationStates;
        private Dictionary<RowPresenter, ValidationMessage> _asyncValidationMessages;

        public RowInput<T> WithAsyncValidator(Func<RowPresenter, Task<ValidationMessage>> asyncValidator)
        {
            if (asyncValidator == null)
                throw new ArgumentNullException(nameof(asyncValidator));

            VerifyNotSealed();
            _asyncValidator = asyncValidator;
            _pendingAsyncValidators = new HashSet<RowPresenter>();
            _asyncValidationStates = new Dictionary<RowPresenter, AsyncValidationState>();
            _asyncValidationMessages = new Dictionary<RowPresenter, ValidationMessage>();
            return this;
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (_asyncValidator != null)
            {
                _asyncValidationStates.Remove(rowPresenter);
                _asyncValidationMessages.Remove(rowPresenter);
                _pendingAsyncValidators.Remove(rowPresenter);
            }
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
                    state = message == null ? AsyncValidationState.Valid : AsyncValidationState.Invalid;
                }
                catch (Exception ex)
                {
                    message = new ValidationMessage(null, ValidationSeverity.Error, ex.Message, this.SourceColumns);
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

        internal ValidationMessage GetAsyncValidationMessage(RowPresenter rowPresenter)
        {
            Debug.Assert(_asyncValidationMessages != null);

            ValidationMessage result;
            if (_asyncValidationMessages.TryGetValue(rowPresenter, out result))
                return result;
            return null;
        }

        private void SetAsyncValidationMessage(RowPresenter rowPresenter, ValidationMessage message)
        {
            Debug.Assert(_asyncValidationMessages != null);

            if (message == null)
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

        internal bool HasAsyncValidator
        {
            get { return _asyncValidator != null; }
        }

        internal void RunAsyncValidator(RowPresenter rowPresenter)
        {
            Debug.Assert(HasAsyncValidator);
            if (ShouldRunAsyncValidator(rowPresenter))
                AsyncValidate(rowPresenter);
        }

        private bool ShouldRunAsyncValidator(RowPresenter rowPresenter)
        {
            return HasAsyncValidator && !HasInputError && ValidationManager.ShouldRunAsyncValidator(rowPresenter, SourceColumns);
        }

        private IReadOnlyList<IValidationMessage> GetErrors(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);

            List<IValidationMessage> result = null;

            if (rowPresenter == CurrentRow && HasInputError)
                result = result.AddItem(InputError);

            result = result.AddItems(ValidationManager.GetErrors(rowPresenter, this));

            var asyncMessage = GetAsyncValidationMessage(rowPresenter);
            if (asyncMessage != null && asyncMessage.Severity == ValidationSeverity.Error)
                result = result.AddItem(asyncMessage);

            return result.ToReadOnlyList();
        }

        private IReadOnlyList<IValidationMessage> GetWarnings(RowPresenter rowPresenter)
        {
            List<IValidationMessage> result = null;
            result = result.AddItems(ValidationManager.GetWarnings(rowPresenter, this));

            var asyncMessage = GetAsyncValidationMessage(rowPresenter);
            if (asyncMessage != null && asyncMessage.Severity == ValidationSeverity.Warning)
                result = result.AddItem(GetAsyncValidationMessage(rowPresenter));

            return result.ToReadOnlyList();
        }

        internal void RefreshValidation(T element, RowPresenter rowPresenter)
        {
            element.RefreshValidation(GetErrors(rowPresenter), GetWarnings(rowPresenter));
        }
    }
}
