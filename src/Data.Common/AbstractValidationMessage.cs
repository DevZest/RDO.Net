using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
    public abstract class AbstractValidationMessage : IAbstractValidationMessageGroup
    {
        public abstract string Id { get; }

        public abstract string Description { get; }

        public abstract ValidationSeverity Severity { get; }

        public override string ToString()
        {
            return Description;
        }

        #region IAbstractValidationMessageGroup

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IAbstractValidationMessageGroup.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<AbstractValidationMessage>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        AbstractValidationMessage IReadOnlyList<AbstractValidationMessage>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IAbstractValidationMessageGroup IAbstractValidationMessageGroup.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IAbstractValidationMessageGroup IAbstractValidationMessageGroup.Add(AbstractValidationMessage value)
        {
            Check.NotNull(value, nameof(value));
            return AbstractValidationMessageGroup.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<AbstractValidationMessage> IEnumerable<AbstractValidationMessage>.GetEnumerator()
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
