using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Base class of scalar data that can be used as data binding source.
    /// </summary>
    public abstract class Scalar : IScalars
    {
        internal Scalar(ScalarContainer container, int ordinal)
        {
            _container = container;
            _ordinal = ordinal;
        }

        private readonly ScalarContainer _container;
        /// <summary>
        /// Gets the container that manages all scalar data.
        /// </summary>
        public ScalarContainer Container
        {
            get { return _container; }
        }

        private readonly int _ordinal;
        /// <summary>
        /// Gets the ordinal.
        /// </summary>
        public int Ordinal
        {
            get { return _ordinal; }
        }

        /// <summary>
        /// Gets the data value.
        /// </summary>
        /// <param name="beforeEdit">Indicates whether should get data value before edit.</param>
        /// <returns>The data value.</returns>
        public object GetValue(bool beforeEdit = false)
        {
            return GetValueOverride(beforeEdit);
        }

        internal abstract object GetValueOverride(bool beforeEdit);

        /// <summary>
        /// Sets the data value.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <param name="beforeEdit">Indicates whether should get data value before edit.</param>
        public void SetValue(object value, bool beforeEdit = false)
        {
            SetValueOverride(value, beforeEdit);
        }

        internal abstract void SetValueOverride(object value, bool beforeEdit);

        /// <summary>
        /// Gets a value indicates whether this scalar data is being edit.
        /// </summary>
        public virtual bool IsEditing
        {
            get { return Container.IsEditing; }
        }

        internal abstract void BeginEdit();

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
