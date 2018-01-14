using DevZest.Data.Presenters.Primitives;
using System.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections;
using System.Windows;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public abstract class ScalarAsyncValidator : AsyncValidator<IScalarValidationErrors>, IScalarAsyncValidators
    {
        internal static ScalarAsyncValidator Create<T>(ScalarInput<T> scalarInput, Func<Task<IScalarValidationErrors>> action, Action postAction)
            where T : UIElement, new()
        {
            return new ScalarInputAsyncValidator<T>(scalarInput, action, postAction);
        }

        internal static ScalarAsyncValidator Create(Template template, IScalars sourceScalars, Func<Task<IScalarValidationErrors>> action, Action postAction)
        {
            return new SourceScalarsAsyncValidator(template, sourceScalars, action, postAction);
        }

        private sealed class ScalarInputAsyncValidator<T> : ScalarAsyncValidator
            where T : UIElement, new()
        {
            public ScalarInputAsyncValidator(ScalarInput<T> scalarInput, Func<Task<IScalarValidationErrors>> action, Action postAction)
                : base(action, postAction)
            {
                Debug.Assert(scalarInput != null);
                Debug.Assert(action != null);
                _scalarInput = scalarInput;
            }

            private readonly ScalarInput<T> _scalarInput;

            public override IScalars SourceScalars
            {
                get { return _scalarInput.Target; }
            }

            internal override InputManager InputManager
            {
                get { return _scalarInput.InputManager; }
            }
        }

        private sealed class SourceScalarsAsyncValidator : ScalarAsyncValidator
        {
            public SourceScalarsAsyncValidator(Template template, IScalars sourceScalars, Func<Task<IScalarValidationErrors>> action, Action postAction)
                : base(action, postAction)
            {
                Debug.Assert(template != null);
                _template = template;
                _sourceScalars = sourceScalars;
            }

            private readonly Template _template;
            private readonly IScalars _sourceScalars;

            internal override InputManager InputManager
            {
                get { return _template.InputManager; }
            }

            public override IScalars SourceScalars
            {
                get { return _sourceScalars; }
            }
        }

#if DEBUG
        public ScalarAsyncValidator(Func<Task<IScalarValidationErrors>> action)
            : this(action, null)
        {
        }
#endif

        private ScalarAsyncValidator(Func<Task<IScalarValidationErrors>> action, Action postAction)
            : base(postAction)
        {
            Debug.Assert(action != null);
            _action = action;
        }

        private readonly Func<Task<IScalarValidationErrors>> _action;

        protected sealed override async Task<IScalarValidationErrors> ValidateAsync()
        {
            return await _action();
        }

        public abstract IScalars SourceScalars { get; }

        private IScalarValidationErrors _errors = ScalarValidationErrors.Empty;
        public IScalarValidationErrors Errors
        {
            get { return _errors; }
            private set
            {
                Debug.Assert(value != null && value.IsSealed);
                if (_errors == value)
                    return;
                _errors = value;
                OnPropertyChanged(nameof(Errors));
                RefreshHasError();
            }
        }

        private bool _hasError;
        public sealed override bool HasError
        {
            get { return _hasError; }
        }
        private void RefreshHasError()
        {
            var value = Errors.Count > 0;
            if (value == _hasError)
                return;
            _hasError = value;
            OnPropertyChanged(nameof(HasError));
        }

        protected sealed override void ClearValidationMessages()
        {
            Errors = ScalarValidationErrors.Empty;
        }

        protected sealed override IScalarValidationErrors EmptyValidationResult
        {
            get { return ScalarValidationErrors.Empty; }
        }

        protected sealed override void RefreshValidationErrors(IScalarValidationErrors result)
        {
            Errors = result;
        }

        #region IScalarAsyncValidators

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IScalarAsyncValidators.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<ScalarAsyncValidator>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        ScalarAsyncValidator IReadOnlyList<ScalarAsyncValidator>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IScalarAsyncValidators IScalarAsyncValidators.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IScalarAsyncValidators IScalarAsyncValidators.Add(ScalarAsyncValidator value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return ScalarAsyncValidators.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        ScalarAsyncValidator IScalarAsyncValidators.this[IScalars sourceScalars]
        {
            get { return SourceScalars.SetEquals(sourceScalars) ? this : null; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<ScalarAsyncValidator> IEnumerable<ScalarAsyncValidator>.GetEnumerator()
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
