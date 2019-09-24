using DevZest.Data.Presenters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views.Primitives
{
    /// <summary>
    /// Represents the control that displays and retries the async validation fault.
    /// </summary>
    /// <remarks></remarks>
    public class AsyncValidationFaultControl : Control
    {
        /// <summary>
        /// Contains commands supported by <see cref="AsyncValidationFaultControl"/>.
        /// </summary>
        public static class Commands
        {
            /// <summary>
            /// Command to retry async validation.
            /// </summary>
            public static readonly RoutedUICommand Retry = new RoutedUICommand(UserMessages.AsyncValidationFaultControlCommands_RetryCommandText, nameof(Retry), typeof(Commands));
        }

        /// <summary>
        /// Identifies the <see cref="Fault"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FaultProperty = DependencyProperty.Register(nameof(Fault), typeof(AsyncValidationFault), typeof(AsyncValidationFaultControl),
            new FrameworkPropertyMetadata(null, OnFaultChanged));

        private static readonly DependencyPropertyKey MessagePropertyKey = DependencyProperty.RegisterReadOnly(nameof(Message), typeof(string), typeof(AsyncValidationFaultControl),
            new FrameworkPropertyMetadata(null));
        /// <summary>
        /// Identifies the <see cref="Message"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageProperty = MessagePropertyKey.DependencyProperty;

        static AsyncValidationFaultControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AsyncValidationFaultControl), new FrameworkPropertyMetadata(typeof(AsyncValidationFaultControl)));
        }

        private static void OnFaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AsyncValidationFaultControl)d).Message = ((AsyncValidationFault)e.NewValue)?.Message;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AsyncValidationFaultControl"/>.
        /// </summary>
        public AsyncValidationFaultControl()
        {
            CommandBindings.Add(new CommandBinding(Commands.Retry, ExecRetry, CanRetry));
        }

        /// <summary>
        /// Gets or sets the async validation fault. This is a dependency property.
        /// </summary>
        public AsyncValidationFault Fault
        {
            get { return (AsyncValidationFault)GetValue(FaultProperty); }
            set { SetValue(FaultProperty, value); }
        }

        /// <summary>
        /// Gets the error message. This is a depdency property.
        /// </summary>
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
