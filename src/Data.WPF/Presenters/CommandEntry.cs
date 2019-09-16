using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents the implementation of command.
    /// </summary>
    public struct CommandEntry
    {
        /// <summary>
        /// Gets the command that is implemented.
        /// </summary>
        public readonly ICommand Command;

        /// <summary>
        /// The handler when command is executed.
        /// </summary>
        public readonly ExecutedRoutedEventHandler Executed;

        /// <summary>
        /// The handler to determine whether command can be executed.
        /// </summary>
        public readonly CanExecuteRoutedEventHandler CanExecute;

        private object _inputGestures;

        /// <summary>
        /// Gets the count of input gestures for this command implementation.
        /// </summary>
        public int InputGesturesCount
        {
            get
            {
                if (_inputGestures == null)
                    return 0;
                else if (_inputGestures is InputGesture)
                    return 1;
                else
                    return ((IReadOnlyList<InputGesture>)_inputGestures).Count;
            }
        }

        /// <summary>
        /// Gets the input gesture at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <returns>The result input gesture.</returns>
        public InputGesture GetInputGesture(int index)
        {
            if (index < 0 || index >= InputGesturesCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (_inputGestures is InputGesture)
                return (InputGesture)_inputGestures;
            else
                return ((IReadOnlyList<InputGesture>)_inputGestures)[index];
        }

        internal CommandEntry(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, object inputGestures)
        {
            Command = command;
            Executed = executed;
            CanExecute = canExecute;
            _inputGestures = inputGestures;
        }

        /// <summary>
        /// Create a new <see cref="CommandEntry"/> by replaces the input gestures.
        /// </summary>
        /// <param name="inputGestures">The new input gestures.</param>
        /// <returns>The newly created <see cref="CommandEntry"/>.</returns>
        public CommandEntry ReplaceWith(params InputGesture[] inputGestures)
        {
            return Command.Bind(Executed, CanExecute, inputGestures);
        }
    }
}
