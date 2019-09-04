using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
    /// <summary>
    /// Base class for data validation error.
    /// </summary>
    public abstract class ValidationError : IValidationErrors
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        protected ValidationError(string message)
        {
            _message = message.VerifyNotEmpty(nameof(message));
        }

        private readonly string _message;
        /// <summary>
        /// Gets the validation error message
        /// </summary>
        public string Message
        {
            get { return _message; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Message;
        }

        #region IValidationErrors

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<ValidationError>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        ValidationError IReadOnlyList<ValidationError>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IValidationErrors IValidationErrors.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IValidationErrors IValidationErrors.Add(ValidationError value)
        {
            value.VerifyNotNull(nameof(value));
            return ValidationErrors.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<ValidationError> IEnumerable<ValidationError>.GetEnumerator()
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
