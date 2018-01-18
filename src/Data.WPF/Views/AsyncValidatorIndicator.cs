using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    public class AsyncValidatorIndicator : Control, IRowElement, IScalarElement
    {
        public static class Commands
        {
            public static readonly RoutedUICommand Retry = new RoutedUICommand();
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(AsyncValidatorStatus), typeof(AsyncValidatorIndicator),
            new FrameworkPropertyMetadata(AsyncValidatorStatus.Inactive));

        public static readonly DependencyProperty ExceptionProperty = DependencyProperty.Register(nameof(Exception), typeof(Exception), typeof(AsyncValidatorIndicator),
            new FrameworkPropertyMetadata(null));

        static AsyncValidatorIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AsyncValidatorIndicator), new FrameworkPropertyMetadata(typeof(AsyncValidatorIndicator)));
        }

        public AsyncValidatorIndicator()
        {
            var commandBinding = new CommandBinding(Commands.Retry, Retry, CanRetry);
            CommandBindings.Add(commandBinding);
        }

        private void Retry(object sender, ExecutedRoutedEventArgs e)
        {
            Validator.Run();
        }

        private void CanRetry(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Validator != null && Validator.Status != AsyncValidatorStatus.Running;
        }

        public AsyncValidator Validator { get; internal set; }

        public AsyncValidatorStatus Status
        {
            get { return (AsyncValidatorStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public Exception Exception { get; set; }

        private void Refresh()
        {
            Status = Validator == null ? AsyncValidatorStatus.Inactive : Validator.Status;
            Exception = Validator?.Exception;
        }

        void IScalarElement.Setup(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
            Refresh();
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
        }

        void IRowElement.Setup(RowPresenter rowPresenter)
        {
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
            Refresh();
        }

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
        }
    }
}
