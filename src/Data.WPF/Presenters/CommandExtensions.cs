using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    public static class CommandExtensions
    {
        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute = null)
        {
            Verify(command, executed);
            return new CommandEntry(command, executed, canExecute, null);
        }

        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, InputGesture inputGesture)
        {
            return Bind(command, executed, (CanExecuteRoutedEventHandler)null, inputGesture);
        }

        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, InputGesture inputGesture)
        {
            Verify(command, executed, inputGesture);
            return new CommandEntry(command, executed, canExecute, inputGesture);
        }

        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, params InputGesture[] inputGestures)
        {
            return Bind(command, executed, null, inputGestures);
        }

        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, params InputGesture[] inputGestures)
        {
            Verify(command, executed, inputGestures);
            return new CommandEntry(command, executed, canExecute, inputGestures);
        }

        public static CommandEntry Bind(this ICommand command, IReadOnlyList<InputGesture> inputGestures, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
        {
            Verify(command, executed, inputGestures);
            return new CommandEntry(command, executed, canExecute, inputGestures);
        }

        public static CommandEntry Bind(this ICommand command, InputGesture inputGesture)
        {
            Verify(command);
            Verify(inputGesture);
            return new CommandEntry(command, null, null, inputGesture);
        }

        public static CommandEntry Bind(this ICommand command, params InputGesture[] inputGestures)
        {
            Verify(command);
            Verify(inputGestures);
            return new CommandEntry(command, null, null, inputGestures);
        }

        private static void Verify(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
        }

        private static void Verify(ICommand command, ExecutedRoutedEventHandler executed)
        {
            Verify(command);
            if (executed == null)
                throw new ArgumentNullException(nameof(executed));
        }

        private static void Verify(ICommand command, ExecutedRoutedEventHandler executed, InputGesture inputGesture)
        {
            Verify(command, executed);
            Verify(inputGesture);
        }

        private static void Verify(InputGesture inputGesture)
        {
            if (inputGesture == null)
                throw new ArgumentNullException(nameof(inputGesture));
        }

        private static void Verify(ICommand command, ExecutedRoutedEventHandler executed, IReadOnlyList<InputGesture> inputGestures)
        {
            Verify(command, executed);
            Verify(inputGestures);
        }

        private static void Verify(IReadOnlyList<InputGesture> inputGestures)
        {
            if (inputGestures == null || inputGestures.Count == 0)
                return;

            for (int i = 0; i < inputGestures.Count; i++)
            {
                if (inputGestures[i] == null)
                    throw new ArgumentException(DiagnosticMessages._ArgumentNullAtIndex(nameof(inputGestures), i), nameof(inputGestures));
            }
        }

        public static void SetupCommandEntries(this UIElement element, IEnumerable<CommandEntry> commandEntries)
        {
            if (commandEntries == null)
                return;

            foreach (var entry in commandEntries)
            {
                if (entry.Executed != null)
                {
                    var commandBinding = new CommandBinding(entry.Command, entry.Executed, entry.CanExecute);
                    element.CommandBindings.Add(commandBinding);
                }
                for (int i = 0; i < entry.InputGesturesCount; i++)
                {
                    var inputBinding = new InputBinding(entry.Command, entry.GetInputGesture(i));
                    element.InputBindings.Add(inputBinding);
                }
            }
        }

        public static void CleanupCommandEntries(this UIElement element)
        {
            element.CommandBindings.Clear();
            element.InputBindings.Clear();
        }
    }
}
