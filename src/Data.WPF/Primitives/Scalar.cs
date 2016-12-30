using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class Scalar : IValidationSource<Scalar>
    {
        #region IValidationSource<Scalar>

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<Scalar>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IValidationSource<Scalar>.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IValidationSource<Scalar> IValidationSource<Scalar>.Add(Scalar value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return value == this ? this : ValidationSource<Scalar>.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IValidationSource<Scalar> IValidationSource<Scalar>.Clear()
        {
            return ValidationSource<Scalar>.Empty;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IValidationSource<Scalar>.Contains(Scalar scalar)
        {
            return scalar == this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<Scalar> IEnumerable<Scalar>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IValidationSource<Scalar> IValidationSource<Scalar>.Remove(Scalar value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return value == this ? ValidationSource<Scalar>.Empty : this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IValidationSource<Scalar> IValidationSource<Scalar>.Seal()
        {
            return this;
        }

        #endregion
    }
}
