using DevZest.Windows.Controls;
using DevZest.Windows.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Windows.Data
{
    public static class BindingManager
    {
        public static T WithStyle<T>(this T binding, Style value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.Style = value;
            return binding;
        }

        public static T WithAutoSizeOrder<T>(this T binding, int value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.AutoSizeOrder = value;
            return binding;
        }

        public static T WithAutoSizeWaiver<T>(this T binding, AutoSizeWaiver value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.AutoSizeWaiver = value;
            return binding;
        }

        public static T WithFlowable<T>(this T scalarBinding, bool value)
            where T : ScalarBinding
        {
            scalarBinding.VerifyNotSealed();
            scalarBinding.Flowable = value;
            return scalarBinding;
        }

        private static readonly DependencyProperty BindingProperty = DependencyProperty.RegisterAttached(nameof(Binding),
            typeof(Binding), typeof(BindingManager), new PropertyMetadata(null));

        internal static Binding GetBinding(this UIElement element)
        {
            return (Binding)element.GetValue(BindingProperty);
        }

        internal static void SetBinding(this UIElement element, Binding value)
        {
            Debug.Assert(value != null && element.GetBinding() == null);
            element.SetValue(BindingProperty, value);
        }

        private static readonly DependencyProperty ScalarFlowIndexProperty = DependencyProperty.RegisterAttached("ScalarFlowIndex",
            typeof(int), typeof(BindingManager), new PropertyMetadata(0));

        internal static int GetScalarFlowIndex(this UIElement element)
        {
            return (int)element.GetValue(ScalarFlowIndexProperty);
        }

        internal static void SetScalarFlowIndex(this UIElement element, int value)
        {
            if (value == 0)
                element.ClearValue(ScalarFlowIndexProperty);
            else
                element.SetValue(ScalarFlowIndexProperty, value);
        }


        private static readonly DependencyProperty BlockViewProperty = DependencyProperty.RegisterAttached(nameof(BlockView),
            typeof(BlockView), typeof(BindingManager), new PropertyMetadata(null));

        internal static BlockView GetBlockView(this UIElement element)
        {
            return (BlockView)element.GetValue(BlockViewProperty);
        }

        internal static void SetBlockView(this UIElement element, BlockView value)
        {
            if (value == null)
                element.ClearValue(BlockViewProperty);
            else
                element.SetValue(BlockViewProperty, value);
        }

        private static readonly DependencyProperty RowPresenterProperty = DependencyProperty.RegisterAttached(nameof(RowPresenter),
            typeof(RowPresenter), typeof(BindingManager), new PropertyMetadata(null));

        internal static RowPresenter GetRowPresenter(this UIElement element)
        {
            return (RowPresenter)element.GetValue(RowPresenterProperty);
        }

        internal static void SetRowPresenter(this UIElement element, RowPresenter value)
        {
            if (value == null)
                element.ClearValue(RowPresenterProperty);
            else
                element.SetValue(RowPresenterProperty, value);
        }

        public static CommandEntry CommandBinding(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute = null)
        {
            VerifyCommandBinding(command, executed);
            return new CommandEntry(command, executed, canExecute, null);
        }

        public static CommandEntry InputBinding(this ICommand command, ExecutedRoutedEventHandler executed, InputGesture inputGesture)
        {
            return InputBinding(command, executed, null, inputGesture);
        }

        public static CommandEntry InputBinding(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, InputGesture inputGesture)
        {
            VerifyInputBinding(command, executed, inputGesture);
            return new CommandEntry(command, executed, canExecute, inputGesture);
        }

        public static CommandEntry InputBindings(this ICommand command, ExecutedRoutedEventHandler executed, params InputGesture[] inputGestures)
        {
            return InputBindings(command, executed, null, inputGestures);
        }

        public static CommandEntry InputBindings(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, params InputGesture[] inputGestures)
        {
            VerifyInputBindings(command, executed, inputGestures);
            return new CommandEntry(command, executed, canExecute, inputGestures);
        }

        private static void VerifyCommandBinding(ICommand command, ExecutedRoutedEventHandler executed)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
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

        internal static void SetupCommandEntries(this UIElement element, IEnumerable<CommandEntry> commandEntries)
        {
            if (commandEntries == null)
                return;

            foreach (var entry in commandEntries)
            {
                var commandBinding = new CommandBinding(entry.Command, entry.Executed, entry.CanExecute);
                element.CommandBindings.Add(commandBinding);
                for (int i = 0; i < entry.InputGesturesCount; i++)
                {
                    var inputBinding = new InputBinding(entry.Command, entry.GetInputGesture(i));
                    element.InputBindings.Add(inputBinding);
                }
            }
        }

        internal static void CleanupCommandEntries(this UIElement element)
        {
            element.CommandBindings.Clear();
            element.InputBindings.Clear();
        }

        internal static Action<TElement, TPresenter> Override<TElement, TPresenter>(this Action<TElement, TPresenter> action,
            Action<TElement, TPresenter, Action<TElement, TPresenter>> overrideAction)
        {
            if (action == null)
                action = (e, sp) => { };
            return (e, sp) => overrideAction(e, sp, action);
        }
    }
}
