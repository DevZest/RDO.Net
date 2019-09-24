using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents scalar data validation error.
    /// </summary>
    public sealed class ScalarValidationError : ValidationError<IScalars>, IScalarValidationErrors
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ScalarValidationError"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="source">The error source.</param>
        public ScalarValidationError(string message, IScalars source)
            : base(message, source)
        {
            source.VerifyNotNull(nameof(source));
            if (source.Count == 0)
                throw new ArgumentException(DiagnosticMessages.ScalarValidationMessage_EmptySourceScalars, nameof(source));
        }

        #region IScalarValidationMessageGroup

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<ScalarValidationError>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        ScalarValidationError IReadOnlyList<ScalarValidationError>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        bool IScalarValidationErrors.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IScalarValidationErrors IScalarValidationErrors.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IScalarValidationErrors IScalarValidationErrors.Add(ScalarValidationError value)
        {
            value.VerifyNotNull(nameof(value));
            return ScalarValidationErrors.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<ScalarValidationError> IEnumerable<ScalarValidationError>.GetEnumerator()
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
