using System;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public sealed class Scalar<T> : Scalar
    {
        public Scalar(T value = default(T), Action<T> onValueChanged = null, Func<T, FlushError> valueValidator = null)
        {
            _value = value;
            _onValueChanged = onValueChanged;
            _valueValidator = valueValidator;
        }

        private Func<T, FlushError> _valueValidator;

        public FlushError Validate(T value)
        {
            return _valueValidator != null ? _valueValidator(value) : FlushError.Empty;
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
