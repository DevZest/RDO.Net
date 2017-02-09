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
        internal AsyncValidator Create<T>(RowInput<T> rowInput, Func<Task<IValidationMessageGroup>> action)
            where T : UIElement, new()
        {
            return new RowInputAsyncValidator<T>(rowInput, action);
        }

        internal AsyncValidator Create(Template template, IColumnSet sourceColumns, Func<Task<IValidationMessageGroup>> action)
        {
            return new CurrentRowAsyncValidator(template, sourceColumns, action);
        }

        internal AsyncValidator Create(Template template, IColumnSet sourceColumns, Func<Task<IValidationResult>> action)
        {
            return new AllRowAsyncValidator(template, sourceColumns, action);
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
            public RowInputAsyncValidator(RowInput<T> rowInput, Func<Task<IValidationMessageGroup>> action)
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
            protected RowAsyncValidator(Template template, IColumnSet sourceColumns)
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
            public CurrentRowAsyncValidator(Template template, IColumnSet sourceColumns, Func<Task<IValidationMessageGroup>> action)
                : base(template, sourceColumns)
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
            public AllRowAsyncValidator(Template template, IColumnSet sourceColumns, Func<Task<IValidationResult>> action)
                : base(template, sourceColumns)
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
