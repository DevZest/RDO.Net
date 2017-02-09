using DevZest.Data.Windows.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public abstract class AsyncValidator : IAsyncValidatorGroup
    {
        private sealed class RowInputAsyncValidator<T> : AsyncValidator
            where T : UIElement, new()
        {
            public RowInputAsyncValidator(RowInput<T> rowInput, Func<Task<IValidationMessageGroup>> action)
            {
                Debug.Assert(rowInput != null);
                Debug.Assert(action != null);
                _rowInput = rowInput;
                _action = action;
            }

            private readonly RowInput<T> _rowInput;
            private readonly Func<Task<IValidationMessageGroup>> _action;

            internal override InputManager InputManager
            {
                get { return _rowInput.InputManager; }
            }

            private RowPresenter CurrentRow
            {
                get { return InputManager.CurrentRow; }
            }

            public override IColumnSet SourceColumns
            {
                get { return _rowInput.Columns; }
            }

            public override ValidationScope Scope
            {
                get { return ValidationScope.CurrentRow; }
            }

            protected override async Task<IValidationDictionary> Validate()
            {
                var messages = await _action();
                if (messages == null || messages.Count == 0 || CurrentRow == null)
                    return ValidationDictionary.Empty;
                return ValidationDictionary.Empty.Add(CurrentRow, messages);
            }
        }

        private AsyncValidator()
        {
        }

        internal abstract InputManager InputManager { get; }

        public abstract IColumnSet SourceColumns { get; }

        public IValidationDictionary Errors { get; private set; } = ValidationDictionary.Empty;

        public IValidationDictionary Warnings { get; private set; } = ValidationDictionary.Empty;

        public abstract ValidationScope Scope { get; }

        protected abstract Task<IValidationDictionary> Validate();

        public AsyncValidatorState State { get; private set; } = AsyncValidatorState.Idle;

        public Exception Exception { get; private set; }

        private bool _pendingValidationRequest;
        public async void Run()
        {
            if (State == AsyncValidatorState.Running)
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
                State = AsyncValidatorState.Running;
                try
                {
                    result = await task;
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    State = AsyncValidatorState.Failed;
                }
            }
            while (_pendingValidationRequest);

            if (State != AsyncValidatorState.Failed)
            {
                Exception = null;
                State = AsyncValidatorState.Completed;
                Errors = result.Where(ValidationSeverity.Error);
                Warnings = result.Where(ValidationSeverity.Warning);
            }
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
