using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents scalar data that can be used as data binding source.
    /// </summary>
    public abstract class Scalar<T> : Scalar
    {
        internal static Scalar<T> Create(ScalarContainer container, int ordinal, IEqualityComparer<T> equalityComparer, T value)
        {
            return new EntityScalar(container, ordinal, equalityComparer, value);
        }

        internal static Scalar<T> Create(ScalarContainer container, int ordinal, IEqualityComparer<T> equalityComparer, Func<T> getter, Action<T> setter)
        {
            return new LinkedScalar(container, ordinal, equalityComparer, getter, setter);
        }

        internal Scalar(ScalarContainer container, int ordinal, IEqualityComparer<T> equalityComparer)
            : base(container, ordinal)
        {
            if (equalityComparer == null)
                equalityComparer = EqualityComparer<T>.Default;
            _equalityComparer = equalityComparer;
        }

        private readonly IEqualityComparer<T> _equalityComparer;
        /// <summary>
        /// Gets the equality comparer.
        /// </summary>
        public IEqualityComparer<T> EqualityComparer
        {
            get { return _equalityComparer; }
        }

        /// <summary>
        /// Gets or sets the data value.
        /// </summary>
        public T Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        /// <summary>
        /// Gets the data value.
        /// </summary>
        /// <param name="beforeEdit">Indicates whether should get data value before edit.</param>
        /// <returns>The data value.</returns>
        public new T GetValue(bool beforeEdit = false)
        {
            return PerformGetValue(beforeEdit);
        }

        internal abstract T PerformGetValue(bool beforeEdit);

        /// <summary>
        /// Sets the data value.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <param name="beforeEdit">Indicates whether should get data value before edit.</param>
        public bool SetValue(T value, bool beforeEdit = false)
        {
            return PerformSetValue(value, beforeEdit);
        }

        internal abstract bool PerformSetValue(T value, bool beforeEdit);

        /// <summary>
        /// Tries to edit data value.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <returns><see langword="true"/> if editing value operation completed successfully, otherwise <see langword="false"/>.</returns>
        public bool EditValue(T value)
        {
            return Container.Edit(this, value);
        }

        internal sealed override object GetValueOverride(bool beforeEdit)
        {
            return GetValue(beforeEdit);
        }

        internal sealed override void SetValueOverride(object value, bool beforeEdit)
        {
            SetValue((T)value, beforeEdit);
        }

        private List<Func<T, string>> _validators;
        /// <summary>
        /// Adds validator.
        /// </summary>
        /// <param name="validator">The validator.</param>
        /// <returns>This scalar data for fluent coding.</returns>
        public Scalar<T> AddValidator(Func<T, string> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (_validators == null)
                _validators = new List<Func<T, string>>();
            _validators.Add(validator);
            return this;
        }

        internal override IScalarValidationErrors Validate(IScalarValidationErrors result)
        {
            Debug.Assert(result != null);
            if (_validators == null)
                return result;

            for (int i = 0; i < _validators.Count; i++)
            {
                var validator = _validators[i];
                var message = validator(GetValue());
                if (!string.IsNullOrEmpty(message))
                    result = result.Add(new ScalarValidationError(message, this));
            }
            return result;
        }

        private sealed class EntityScalar : Scalar<T>
        {
            public EntityScalar(ScalarContainer container, int ordinal, IEqualityComparer<T> equalityComparer, T value)
                : base(container, ordinal, equalityComparer)
            {
                _value = value;
                if (IsEditing)
                    _editingValue = _value;
            }

            private T _value;
            private T _editingValue;

            internal override T PerformGetValue(bool beforeEdit)
            {
                return IsEditing && !beforeEdit ? _editingValue : _value;
            }

            internal override bool PerformSetValue(T value, bool beforeEdit)
            {
                if (IsEditing && !beforeEdit)
                    return Assign(ref _editingValue, value);
                else
                    return Assign(ref _value, value);
            }

            private bool Assign(ref T reference, T value)
            {
                if (EqualityComparer.Equals(reference, value))
                    return false;

                reference = value;
                Container.OnValueChanged(this);
                return true;
            }

            internal override void BeginEdit()
            {
                _editingValue = _value;
            }

            internal override void CancelEdit()
            {
                _editingValue = default(T);
            }

            internal override bool EndEdit()
            {
                var result = Assign(ref _value, _editingValue);
                _editingValue = default(T);
                return result;
            }
        }

        private sealed class LinkedScalar : Scalar<T>
        {
            public LinkedScalar(ScalarContainer container, int ordinal, IEqualityComparer<T> equalityComparer, Func<T> getter, Action<T> setter)
                : base(container, ordinal, equalityComparer)
            {
                Debug.Assert(getter != null);
                Debug.Assert(setter != null);
                _getter = getter;
                _setter = setter;
            }

            private readonly Func<T> _getter;
            private readonly Action<T> _setter;

            internal override T PerformGetValue(bool beforeEdit)
            {
                return _getter();
            }

            internal override bool PerformSetValue(T value, bool beforeEdit)
            {
                var oldValue = _getter();
                if (EqualityComparer.Equals(oldValue, value))
                    return false;
                _setter(value);
                return true;
            }

            public override bool IsEditing
            {
                get { return false; }
            }

            internal override void BeginEdit()
            {
            }

            internal override void CancelEdit()
            {
            }

            internal override bool EndEdit()
            {
                return false;
            }
        }
    }
}
