using DevZest.Data.Windows.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace DevZest.Data.Windows
{
    public abstract class AsyncValidator : IAsyncValidatorGroup, INotifyPropertyChanged
    {
        internal static AsyncValidator Create<T>(RowInput<T> rowInput, Func<Task<IValidationMessageGroup>> action, Action postAction)
            where T : UIElement, new()
        {
            return new RowInputAsyncValidator<T>(rowInput, action, postAction);
        }

        internal static AsyncValidator Create(Template template, IColumnSet sourceColumns, Func<Task<IValidationMessageGroup>> action, Action postAction)
        {
            return new CurrentRowAsyncValidator(template, sourceColumns, action, postAction);
        }

        internal static AsyncValidator Create(Template template, IColumnSet sourceColumns, Func<Task<IValidationResult>> action, Action postAction)
        {
            return new AllRowAsyncValidator(template, sourceColumns, action, postAction);
        }

        private static async Task<IValidationDictionary> Validate(Func<Task<IValidationMessageGroup>> action, RowPresenter currentRow)
        {
            var messages = await action();
            return messages == null || messages.Count == 0 || currentRow == null
                ? ValidationDictionary.Empty : ValidationDictionary.Empty.Add(currentRow, messages);
        }

        private sealed class RowInputAsyncValidator<T> : AsyncValidator
            where T : UIElement, new()
        {
            public RowInputAsyncValidator(RowInput<T> rowInput, Func<Task<IValidationMessageGroup>> action, Action postAction)
                : base(postAction)
            {
                Debug.Assert(rowInput != null);
                Debug.Assert(action != null);
                _rowInput = rowInput;
                _action = action;
            }

            private readonly RowInput<T> _rowInput;
            private readonly Func<Task<IValidationMessageGroup>> _action;

            public override IColumnSet SourceColumns
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

            public override ValidationScope Scope
            {
                get { return ValidationScope.CurrentRow; }
            }

            protected override async Task<IValidationDictionary> Validate()
            {
                return await Validate(_action, CurrentRow);
            }
        }

        private abstract class RowAsyncValidator : AsyncValidator
        {
            protected RowAsyncValidator(Template template, IColumnSet sourceColumns, Action postAction)
                : base(postAction)
            {
                Debug.Assert(template != null);
                _template = template;
                _sourceColumns = sourceColumns;
            }

            private readonly Template _template;
            private readonly IColumnSet _sourceColumns;

            internal override InputManager InputManager
            {
                get { return _template.InputManager; }
            }

            public override IColumnSet SourceColumns
            {
                get { return _sourceColumns; }
            }
        }

        private sealed class CurrentRowAsyncValidator : RowAsyncValidator
        {
            public CurrentRowAsyncValidator(Template template, IColumnSet sourceColumns, Func<Task<IValidationMessageGroup>> action, Action postAction)
                : base(template, sourceColumns, postAction)
            {
                Debug.Assert(action != null);
                _action = action;
            }

            private readonly Func<Task<IValidationMessageGroup>> _action;

            private RowPresenter CurrentRow
            {
                get { return InputManager.CurrentRow; }
            }

            public override ValidationScope Scope
            {
                get { return ValidationScope.CurrentRow; }
            }

            protected override async Task<IValidationDictionary> Validate()
            {
                return await Validate(_action, CurrentRow);
            }
        }

        private sealed class AllRowAsyncValidator : RowAsyncValidator
        {
            public AllRowAsyncValidator(Template template, IColumnSet sourceColumns, Func<Task<IValidationResult>> action, Action postAction)
                : base(template, sourceColumns, postAction)
            {
                Debug.Assert(action != null);
                _action = action;
            }

            private readonly Func<Task<IValidationResult>> _action;

            public override ValidationScope Scope
            {
                get { return ValidationScope.AllRows; }
            }

            protected override async Task<IValidationDictionary> Validate()
            {
                var result = await _action();
                return result == null ? ValidationDictionary.Empty : InputManager.ToValidationDictionary(result);
            }
        }

        private AsyncValidator(Action postAction)
        {
            _postAction = postAction;
        }

        internal abstract InputManager InputManager { get; }

        public abstract IColumnSet SourceColumns { get; }

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

        public abstract ValidationScope Scope { get; }

        protected abstract Task<IValidationDictionary> Validate();

        private AsyncValidatorStatus _status = AsyncValidatorStatus.Created;
        public AsyncValidatorStatus Status
        {
            get { return _status; }
            private set
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

        public async Task Run()
        {
            if (Status == AsyncValidatorStatus.Running)
            {
                _pendingValidationRequest = true;
                return;
            }

            Errors = Warnings = ValidationDictionary.Empty;
            IValidationDictionary result = ValidationDictionary.Empty;
            do
            {
                _pendingValidationRequest = false;
                var task = Validate();
                Status = AsyncValidatorStatus.Running;
                try
                {
                    result = await task;
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    Status = AsyncValidatorStatus.Faulted;
                }
            }
            while (_pendingValidationRequest);

            if (Status != AsyncValidatorStatus.Faulted)
            {
                Exception = null;
                Status = AsyncValidatorStatus.Completed;
                Errors = result.Where(ValidationSeverity.Error);
                Warnings = result.Where(ValidationSeverity.Warning);
            }

            if (_postAction != null)
                _postAction();
        }

        #region IAsyncValidatorGroup
        bool IAsyncValidatorGroup.IsSealed
        {
            get { return true; }
        }

        int IReadOnlyCollection<AsyncValidator>.Count
        {
            get { return 1; }
        }

        AsyncValidator IReadOnlyList<AsyncValidator>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }


        IAsyncValidatorGroup IAsyncValidatorGroup.Seal()
        {
            return this;
        }

        IAsyncValidatorGroup IAsyncValidatorGroup.Add(AsyncValidator value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return AsyncValidatorGroup.New(this, value);
        }

        IEnumerator<AsyncValidator> IEnumerable<AsyncValidator>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion
    }
}
