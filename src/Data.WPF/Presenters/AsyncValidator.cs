using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DevZest.Data;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data.Presenters
{
    public abstract class AsyncValidator : IAsyncValidatorGroup, INotifyPropertyChanged
    {
        internal static AsyncValidator Create<T>(RowInput<T> rowInput, Func<Task<IColumnValidationMessages>> action, Action postAction)
            where T : UIElement, new()
        {
            return new RowInputAsyncValidator<T>(rowInput, action, postAction);
        }

        internal static AsyncValidator Create(Template template, IColumns sourceColumns, Func<Task<IColumnValidationMessages>> action, Action postAction)
        {
            return new CurrentRowAsyncValidator(template, sourceColumns, action, postAction);
        }

        internal static AsyncValidator Create(Template template, IColumns sourceColumns, Func<Task<IValidationResult>> action, Action postAction)
        {
            return new AllRowAsyncValidator(template, sourceColumns, action, postAction);
        }

        private static async Task<IValidationDictionary> Validate(Func<Task<IColumnValidationMessages>> action, RowPresenter currentRow)
        {
            var messages = await action();
            return messages == null || messages.Count == 0 || currentRow == null
                ? ValidationDictionary.Empty : ValidationDictionary.Empty.Add(currentRow, messages);
        }

        private sealed class RowInputAsyncValidator<T> : AsyncValidator
            where T : UIElement, new()
        {
            public RowInputAsyncValidator(RowInput<T> rowInput, Func<Task<IColumnValidationMessages>> action, Action postAction)
                : base(postAction)
            {
                Debug.Assert(rowInput != null);
                Debug.Assert(action != null);
                _rowInput = rowInput;
                _action = action;
            }

            private readonly RowInput<T> _rowInput;
            private readonly Func<Task<IColumnValidationMessages>> _action;

            public override IColumns SourceColumns
            {
                get { return _rowInput.Columns; }
            }

            internal override InputManager InputManager
            {
                get { return _rowInput.InputManager; }
            }

            private RowPresenter CurrentRow
            {
                get { return InputManager.CurrentRow; }
            }

            internal override IRowInput RowInput
            {
                get { return _rowInput; }
            }

            public override RowValidationScope ValidationScope
            {
                get { return RowValidationScope.Current; }
            }

            protected override async Task<IValidationDictionary> ValidateCoreAsync()
            {
                return await Validate(_action, CurrentRow);
            }
        }

        private abstract class RowAsyncValidator : AsyncValidator
        {
            protected RowAsyncValidator(Template template, IColumns sourceColumns, Action postAction)
                : base(postAction)
            {
                Debug.Assert(template != null);
                _template = template;
                _sourceColumns = sourceColumns;
            }

            private readonly Template _template;
            private readonly IColumns _sourceColumns;

            internal override InputManager InputManager
            {
                get { return _template.InputManager; }
            }

            public override IColumns SourceColumns
            {
                get { return _sourceColumns; }
            }
        }

        private sealed class CurrentRowAsyncValidator : RowAsyncValidator
        {
            public CurrentRowAsyncValidator(Template template, IColumns sourceColumns, Func<Task<IColumnValidationMessages>> action, Action postAction)
                : base(template, sourceColumns, postAction)
            {
                Debug.Assert(action != null);
                _action = action;
            }

            private readonly Func<Task<IColumnValidationMessages>> _action;

            private RowPresenter CurrentRow
            {
                get { return InputManager.CurrentRow; }
            }

            internal override IRowInput RowInput
            {
                get { return null; }
            }

            public override RowValidationScope ValidationScope
            {
                get { return RowValidationScope.Current; }
            }

            protected override async Task<IValidationDictionary> ValidateCoreAsync()
            {
                return await Validate(_action, CurrentRow);
            }
        }

        private sealed class AllRowAsyncValidator : RowAsyncValidator
        {
            public AllRowAsyncValidator(Template template, IColumns sourceColumns, Func<Task<IValidationResult>> action, Action postAction)
                : base(template, sourceColumns, postAction)
            {
                Debug.Assert(action != null);
                _action = action;
            }

            private readonly Func<Task<IValidationResult>> _action;

            internal override IRowInput RowInput
            {
                get { return null; }
            }

            public override RowValidationScope ValidationScope
            {
                get { return RowValidationScope.All; }
            }

            protected override async Task<IValidationDictionary> ValidateCoreAsync()
            {
                var result = await _action();
                return result == null ? ValidationDictionary.Empty : InputManager.ToValidationDictionary(result);
            }
        }

#if DEBUG
        public AsyncValidator()
        {
        }
#endif

        private AsyncValidator(Action postAction)
        {
            _postAction = postAction;
        }

        internal abstract InputManager InputManager { get; }

        public abstract IColumns SourceColumns { get; }

        private IValidationDictionary _errors = ValidationDictionary.Empty;
        public IValidationDictionary Errors
        {
            get { return _errors; }
            private set
            {
                Debug.Assert(value != null && value.IsSealed);
                if (_errors == value)
                    return;
                _errors = value;
                OnPropertyChanged(nameof(Errors));
            }
        }

        private IValidationDictionary _warnings = ValidationDictionary.Empty;
        public IValidationDictionary Warnings
        {
            get { return _warnings; }
            private set
            {
                Debug.Assert(value != null && value.IsSealed);
                if (_warnings == value)
                    return;
                _warnings = value;
                OnPropertyChanged(nameof(Warnings));
            }
        }

        internal abstract IRowInput RowInput { get; }

        public abstract RowValidationScope ValidationScope { get; }

        protected abstract Task<IValidationDictionary> ValidateCoreAsync();

        private AsyncValidatorStatus _status = AsyncValidatorStatus.Created;
        public AsyncValidatorStatus Status
        {
            get { return _status; }
            internal set
            {
                if (_status == value)
                    return;
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private Exception _exception;
        public Exception Exception
        {
            get { return _exception; }
            private set
            {
                if (_exception == value)
                    return;
                _exception = value;
                OnPropertyChanged(nameof(Exception));
            }
        }

        private bool _pendingValidationRequest;
        private readonly Action _postAction;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

#if DEBUG
        internal Task LastRunningTask { get; private set; }
#endif
        private Task<IValidationDictionary> _awaitingTask;

        public async void Run()
        {
#if DEBUG
            var runningTask = LastRunningTask = ValidateAsync();
#else
            var runningTask = ValidateAsync();
#endif
            await runningTask;
        }

        private async Task ValidateAsync()
        {
            if (Status == AsyncValidatorStatus.Running)
            {
                _pendingValidationRequest = true;
                return;
            }

            Errors = Warnings = ValidationDictionary.Empty;
            Exception = null;
            Status = AsyncValidatorStatus.Running;
            InputManager.InvalidateView();

            IValidationDictionary result;
            AsyncValidatorStatus status;
            Exception exception;
            do
            {
                _pendingValidationRequest = false;
                var task = _awaitingTask = ValidateCoreAsync();
                try
                {
                    result = await task;
                    if (task != _awaitingTask)   // cancelled
                        return;
                    status = AsyncValidatorStatus.Completed;
                    exception = null;
                }
                catch (Exception ex)
                {
                    if (task != _awaitingTask)  // cancelled
                        return;
                    result = ValidationDictionary.Empty;
                    status = AsyncValidatorStatus.Faulted;
                    exception = ex;
                }
            }
            while (_pendingValidationRequest);

            _awaitingTask = null;
            Exception = exception;
            Status = status;
            Errors = result.Where(ValidationSeverity.Error);
            Warnings = result.Where(ValidationSeverity.Warning);

            if (_postAction != null)
                _postAction();

            InputManager.InvalidateView();
        }

        public void CancelRunning()
        {
            if (Status == AsyncValidatorStatus.Running)
            {
                Status = AsyncValidatorStatus.Created;
                InputManager.InvalidateView();
            }
        }

        internal void Reset()
        {
            if (Status == AsyncValidatorStatus.Running)
                _awaitingTask = null;

            _pendingValidationRequest = false;
            Status = AsyncValidatorStatus.Created;
            Exception = null;
            Errors = Warnings = ValidationDictionary.Empty;
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (Errors.ContainsKey(rowPresenter))
                Errors = Errors.Remove(rowPresenter);
            if (Warnings.ContainsKey(rowPresenter))
                Warnings = Warnings.Remove(rowPresenter);
        }

        internal void OnCurrentRowChanged()
        {
            if (ValidationScope == RowValidationScope.Current)
                Reset();
        }

        #region IAsyncValidatorGroup

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IAsyncValidatorGroup.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<AsyncValidator>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        AsyncValidator IReadOnlyList<AsyncValidator>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IAsyncValidatorGroup IAsyncValidatorGroup.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IAsyncValidatorGroup IAsyncValidatorGroup.Add(AsyncValidator value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return AsyncValidatorGroup.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<AsyncValidator> IEnumerable<AsyncValidator>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

#endregion
    }
}
