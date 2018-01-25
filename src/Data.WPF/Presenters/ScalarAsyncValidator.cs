using DevZest.Data.Presenters.Primitives;
using System.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public abstract class ScalarAsyncValidator : AsyncValidator<IScalarValidationErrors>, IScalarAsyncValidators
    {
        internal static ScalarAsyncValidator Create(string displayName, IScalars sourceScalars, Func<Task<string>> validator)
        {
            return new SingleErrorValidator(displayName, sourceScalars, validator);
        }

        internal static ScalarAsyncValidator Create(string displayName, IScalars sourceScalars, Func<Task<IEnumerable<string>>> validator)
        {
            return new MultipleErrorsValidator(displayName, sourceScalars, validator);
        }

        private sealed class SingleErrorValidator : ScalarAsyncValidator
        {
            public SingleErrorValidator(string displayName, IScalars scalars, Func<Task<string>> validator)
                : base(displayName, scalars)
            {
                _validator = validator;
            }

            private readonly Func<Task<string>> _validator;

            internal override async Task<IScalarValidationErrors> ValidateAsync()
            {
                var message = await _validator();
                return string.IsNullOrEmpty(message) ? ScalarValidationErrors.Empty : new ScalarValidationError(message, SourceScalars);
            }
        }

        private sealed class MultipleErrorsValidator : ScalarAsyncValidator
        {
            public MultipleErrorsValidator(string displayName, IScalars scalars, Func<Task<IEnumerable<string>>> validator)
                : base(displayName, scalars)
            {
                _validator = validator;
            }

            private readonly Func<Task<IEnumerable<string>>> _validator;

            internal override async Task<IScalarValidationErrors> ValidateAsync()
            {
                var messages = await _validator();
                var result = ScalarValidationErrors.Empty;
                if (messages == null)
                    return result;
                foreach (var message in messages)
                {
                    if (!string.IsNullOrEmpty(message))
                        result = result.Add(new ScalarValidationError(message, SourceScalars));
                }
                return result.Seal();
            }
        }

        private ScalarAsyncValidator(string displayName, IScalars sourceScalars)
            : base(displayName)
        {
            Debug.Assert(sourceScalars != null && sourceScalars.Count > 0);
            _sourceScalars = sourceScalars.Seal();
        }

        private readonly IScalars _sourceScalars;
        public IScalars SourceScalars
        {
            get { return _sourceScalars; }
        }

        internal sealed override IScalarValidationErrors EmptyResult
        {
            get { return ScalarValidationErrors.Empty; }
        }

        internal sealed override void OnStatusChanged()
        {
            InputManager.ScalarValidation.UpdateAsyncErrors(this);
            InvalidateView();
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
