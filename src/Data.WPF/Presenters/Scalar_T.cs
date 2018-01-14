using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public sealed class Scalar<T> : Scalar
    {
        internal Scalar(T value = default(T))
        {
            _value = value;
        }

        private List<Func<T, string>> _validators;

        private T _value;
        private Action<T> _onValueChanged;
        public T Value
        {
            get { return _value; }
            set { ChangeValue(value); }
        }

        public Scalar<T> WithOnValueChanged(Action<T> onValueChanged)
        {
            _onValueChanged = onValueChanged;
            return this;
        }

        public Scalar<T> AddValidator(Func<T, string> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (_validators == null)
                _validators = new List<Func<T, string>>();
            _validators.Add(validator);
            return this;
        }

        internal bool ChangeValue(T value)
        {
            var oldValue = _value;
            if (Comparer<T>.Default.Compare(oldValue, value) == 0)
                return false;

            _value = value;
            if (_onValueChanged != null)
                _onValueChanged(oldValue);
            return true;
        }

        internal override IScalarValidationErrors Validate(IScalarValidationErrors result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (_validators == null)
                return result;

            for (int i = 0; i < _validators.Count; i++)
            {
                var validator = _validators[i];
                var message = validator(Value);
                if (!string.IsNullOrEmpty(message))
                    result = result.Add(new ScalarValidationError(message, this));
            }
            return result;
        }
    }
}
