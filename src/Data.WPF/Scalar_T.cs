using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Windows
{
    public sealed class Scalar<T> : Scalar
    {
        public Scalar()
        {
        }

        public ICommand Command { get; set; }

        private T _value;
        public T Value
        {
            get { return _value; }
        }

        public bool SetValue(T value)
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return false;
            _value = value;
            if (Command != null && Command.CanExecute(null))
                Command.Execute(null);
            return true;
        }
    }
}
