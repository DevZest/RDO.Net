using System;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public sealed class Scalar<T>
    {
        public Scalar(T value = default(T), Action<T> onValueChanged = null, Func<T, InputError> valueValidator = null)
        {
            _value = value;
            _onValueChanged = onValueChanged;
            _valueValidator = valueValidator;
        }

        private Func<T, InputError> _valueValidator;

        public InputError Validate(T value)
        {
            return _valueValidator != null ? _valueValidator(value) : InputError.Empty;
        }

        private T _value;
        private Action<T> _onValueChanged;
        public T Value
        {
            get { return _value; }
            set
            {
                var inputError = Validate(value);
                if (!inputError.IsEmpty)
                    throw new ArgumentException(inputError.Description, nameof(value));
                ChangeValue(value);
            }
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
    }
}
