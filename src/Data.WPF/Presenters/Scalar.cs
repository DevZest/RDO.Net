using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data.Presenters
{
    public abstract class Scalar : IScalars
    {
        protected Scalar(ScalarContainer container, int ordinal)
        {
            _container = container;
            _ordinal = ordinal;
        }

        private readonly ScalarContainer _container;
        public ScalarContainer Container
        {
            get { return _container; }
        }

        private readonly int _ordinal;
        public int Ordinal
        {
            get { return _ordinal; }
        }

        public object GetValue(bool beforeEdit = false)
        {
            return PerformGetValue(beforeEdit);
        }

        public void SetValue(object value, bool beforeEdit = false)
        {
            PerformSetValue(value, beforeEdit);
        }

        protected abstract object PerformGetValue(bool beforeEdit);

        protected abstract void PerformSetValue(object value, bool beforeEdit);

        public bool IsEditing
        {
            get { return Container.IsEditing; }
        }

        internal abstract void CancelEdit();

        internal abstract bool EndEdit();

        internal abstract IScalarValidationErrors Validate(IScalarValidationErrors result);

        #region IScalars

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IScalars.Contains(Scalar value)
        {
            return value == this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<Scalar>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<Scalar> IEnumerable<Scalar>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IScalars.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IScalars IScalars.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IScalars IScalars.Add(Scalar value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value == this)
                return this;
            return Scalars.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IScalars IScalars.Remove(Scalar value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value == this)
                return Scalars.Empty;
            else
                return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IScalars IScalars.Clear()
        {
            return Scalars.Empty;
        }

        #endregion
    }
}
