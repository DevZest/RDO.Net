using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public sealed class Scalar<T> : Scalar
    {
        internal Scalar(ScalarContainer container, int ordinal, T value, IComparer<T> comparer)
            : base(container, ordinal)
        {
            _value = value;
            _comparer = comparer ?? Comparer<T>.Default;
        }

        private T _value;
        private T _editingValue;
        private readonly IComparer<T> _comparer;
        public IComparer<T> Comparer
        {
            get { return _comparer; }
        }

        public new T GetValue(bool beforeEdit = false)
        {
            return IsEditing ? _editingValue : _value;
        }

        public bool AssignValue(T value, bool beforeEdit = false)
        {
            if (IsEditing && !beforeEdit)
                return Assign(ref _editingValue, value);
            else
                return Assign(ref _value, value);
        }

        private bool Assign(ref T reference, T value)
        {
            if (Comparer.Compare(reference, value) == 0)
                return false;

            reference = value;
            Container?.OnValueChanged(this);
            return true;
        }

        public bool EditValue(T value)
        {
            return Container.Edit(this, value);
        }

        protected override object PerformGetValue(bool beforeEdit)
        {
            return GetValue(beforeEdit);
        }

        protected override void PerformSetValue(object value, bool beforeEdit)
        {
            AssignValue((T)value, beforeEdit);
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

        private List<Func<T, string>> _validators;
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
    }
}
