using DevZest.Data.Windows.Primitives;
using System;
using System.Windows.Input;

namespace DevZest.Data.Windows
{
    public sealed class Scalar<T> : Scalar
    {
        public Scalar()
        {
        }

        public Scalar(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            _command = command;
        }

        private readonly ICommand _command;
        public ICommand Command
        {
            get { return _command; }
        }

        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (_command != null && _command.CanExecute(null))
                    _command.Execute(null);
            }
        }
    }
}
