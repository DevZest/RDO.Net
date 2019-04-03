using DevZest.Data.Presenters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace DevZest.Data.Views
{
    public class AsyncValidationFaultControl : Control
    {
        public static class Commands
        {
            public static readonly RoutedUICommand Retry = new RoutedUICommand(UserMessages.AsyncValidationFaultControlCommands_RetryCommandText, nameof(Retry), typeof(Commands));
        }

        public static readonly DependencyProperty FaultProperty = DependencyProperty.Register(nameof(Fault), typeof(AsyncValidationFault), typeof(AsyncValidationFaultControl),
            new FrameworkPropertyMetadata(null, OnFaultChanged));
        private static readonly DependencyPropertyKey MessagePropertyKey = DependencyProperty.RegisterReadOnly(nameof(Message), typeof(string), typeof(AsyncValidationFaultControl),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty MessageProperty = MessagePropertyKey.DependencyProperty;

        static AsyncValidationFaultControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AsyncValidationFaultControl), new FrameworkPropertyMetadata(typeof(AsyncValidationFaultControl)));
        }

        private static void OnFaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AsyncValidationFaultControl)d).Message = ((AsyncValidationFault)e.NewValue)?.Message;
        }

        public AsyncValidationFaultControl()
        {
            CommandBindings.Add(new CommandBinding(Commands.Retry, ExecRetry, CanRetry));
        }

        public AsyncValidationFault Fault
        {
            get { return (AsyncValidationFault)GetValue(FaultProperty); }
            set { SetValue(FaultProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            private set { SetValue(MessagePropertyKey, value); }
        }

        private AsyncValidator Validator
        {
            get { return Fault?.Source; }
        }

        private void ExecRetry(object sender, ExecutedRoutedEventArgs e)
        {
            Validator.Run();
        }

        private void CanRetry(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Validator != null;
        }
    }
}
