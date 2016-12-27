using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarInput<T> : Input<T>
        where T : UIElement, new()
    {
        internal static ScalarInput<T> Create<TData>(Trigger<T> flushTrigger, Scalar<TData> scalar, Func<T, TData> getValue)
        {
            return new ScalarInput<T>(flushTrigger).Bind(scalar, getValue);
        }

        private ScalarInput(Trigger<T> flushTrigger)
            : base(flushTrigger)
        {
        }

        public ScalarBinding<T> ScalarBinding { get; private set; }

        public sealed override Binding Binding
        {
            get { return ScalarBinding; }
        }

        internal void Seal(ScalarBinding<T> scalarBinding)
        {
            Debug.Assert(scalarBinding != null);
            VerifyNotSealed();
            ScalarBinding = scalarBinding;
        }

        internal IValidationSource<Scalar> SourceScalars { get; private set; } = ValidationSource<Scalar>.Empty;
        private List<Func<T, bool>> _flushFuncs = new List<Func<T, bool>>();

        public ScalarInput<T> WithPreValidator(Func<T, string> preValidator, Trigger<T> preValidatorTrigger = null)
        {
            SetPreValidator(preValidator, preValidatorTrigger);
            return this;
        }

        public ScalarInput<T> Bind<TData>(Scalar<TData> scalar, Func<T, TData> getValue)
        {
            if (scalar == null)
                throw new ArgumentNullException(nameof(scalar));
            if (getValue == null)
                throw new ArgumentNullException(nameof(getValue));

            SourceScalars = SourceScalars.Union(scalar);
            _flushFuncs.Add((element) => scalar.SetValue(getValue(element)));
            return this;
        }

        private ScalarValidationManager ScalarValidationManager
        {
            get { return Template.ScalarValidationManager; }
        }

        internal override void FlushCore(T element)
        {
            var flushed = DoFlush(element);
            if (flushed)
                ScalarValidationManager.MakeProgress(this);
        }

        private bool DoFlush(T element)
        {
            bool result = false;
            foreach (var flush in _flushFuncs)
            {
                if (flush == null)
                    return true;

                var flushed = flush(element);
                if (flushed)
                    result = true;
            }
            return result;
        }

        private Func<Task<ValidationMessage>> _asyncValidator;
        private bool _hasPendingAsyncValidator;
        private AsyncValidationState _asyncValidationState;
        private ValidationMessage _asyncValidationMessage;

        public ScalarInput<T> WithAsyncValidator(Func<Task<ValidationMessage>> asyncValidator)
        {
            if (asyncValidator == null)
                throw new ArgumentNullException(nameof(asyncValidator));

            VerifyNotSealed();
            _asyncValidator = asyncValidator;
            return this;
        }

        private async void AsyncValidate()
        {
            Debug.Assert(_asyncValidator != null);

            if (_asyncValidationState == AsyncValidationState.Running)
            {
                _hasPendingAsyncValidator = true;
                return;
            }

            var state = _asyncValidationState;
            ValidationMessage message;
            do
            {
                var task = _asyncValidator();
                _asyncValidationState = AsyncValidationState.Running;
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
            while (RemovePendingAsyncValidator());

            _asyncValidationState = state;
            _asyncValidationMessage = message;
            ScalarValidationManager.InvalidateElements();
        }

        private bool RemovePendingAsyncValidator()
        {
            if (_hasPendingAsyncValidator)
            {
                _hasPendingAsyncValidator = false;
                return true;
            }
            return false;
        }

        internal bool HasAsyncValidator
        {
            get { return _asyncValidator != null; }
        }

        internal void RunAsyncValidator()
        {
            Debug.Assert(HasAsyncValidator);
            AsyncValidate();
        }
    }
}
