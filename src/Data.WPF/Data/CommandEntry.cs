using System;
using System.Windows.Input;

namespace DevZest.Windows.Data
{
    public struct CommandEntry
    {
        public readonly ICommand Command;
        public readonly ExecutedRoutedEventHandler Executed;
        public readonly CanExecuteRoutedEventHandler CanExecute;
        private object _inputGestures;

        public int InputGesturesCount
        {
            get
            {
                if (_inputGestures == null)
                    return 0;
                else if (_inputGestures is InputGesture)
                    return 1;
                else
                    return ((InputGesture[])_inputGestures).Length;
            }
        }

        public InputGesture GetInputGesture(int index)
        {
            if (index < 0 || index >= InputGesturesCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (_inputGestures is InputGesture)
                return (InputGesture)_inputGestures;
            else
                return ((InputGesture[])_inputGestures)[index];
        }

        internal CommandEntry(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, object inputGestures)
        {
            Command = command;
            Executed = executed;
            CanExecute = canExecute;
            _inputGestures = inputGestures;
        }
    }
}
