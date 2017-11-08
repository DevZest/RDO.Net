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
            VerifyCommandBinding(command, executed);
            return new CommandEntry(command, executed, canExecute, null);
        }

        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, InputGesture inputGesture)
        {
            return Bind(command, executed, (CanExecuteRoutedEventHandler)null, inputGesture);
        }

        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, InputGesture inputGesture)
        {
            VerifyInputBinding(command, executed, inputGesture);
            return new CommandEntry(command, executed, canExecute, inputGesture);
        }

        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, params InputGesture[] inputGestures)
        {
            return Bind(command, executed, null, inputGestures);
        }

        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, params InputGesture[] inputGestures)
        {
            VerifyInputBindings(command, executed, inputGestures);
            return new CommandEntry(command, executed, canExecute, inputGestures);
        }

        private static void VerifyCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
        }

        private static void VerifyCommandBinding(ICommand command, ExecutedRoutedEventHandler executed)
        {
            VerifyCommand(command);
            if (executed == null)
                throw new ArgumentNullException(nameof(executed));
        }

        private static void VerifyInputBinding(ICommand command, ExecutedRoutedEventHandler executed, InputGesture inputGesture)
        {
            VerifyCommandBinding(command, executed);
            if (inputGesture == null)
                throw new ArgumentNullException(nameof(inputGesture));
        }

        private static void VerifyInputBindings(ICommand command, ExecutedRoutedEventHandler executed, InputGesture[] inputGestures)
        {
            VerifyCommandBinding(command, executed);
            if (inputGestures == null || inputGestures.Length == 0)
                throw new ArgumentNullException(nameof(inputGestures));

            for (int i = 0; i < inputGestures.Length; i++)
            {
                if (inputGestures[i] == null)
                    throw new ArgumentException(Strings.ArgumentNullAtIndex(nameof(inputGestures), i), nameof(inputGestures));
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
