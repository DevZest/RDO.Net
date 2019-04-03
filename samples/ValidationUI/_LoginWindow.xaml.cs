using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ValidationUI
{
    /// <summary>
    /// Interaction logic for _LoginWindow.xaml
    /// </summary>
    public partial class _LoginWindow : Window
    {
        private sealed class Presenter : _LoginPresenter
        {
            public Presenter(_LoginWindow window)
            {
                _window = window;
                Show(window._view);
            }

            private readonly _LoginWindow _window;
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.WithScalarValidationMode(ValidationMode.Implicit)
                    .AddBinding(_window._textBoxEmailAddress, _emailAddress.BindToTextBox())
                    .AddBinding(_window._passwordBox, _password.BindToPasswordBox());
            }
        }

        public static class Commands
        {
            public static readonly RoutedUICommand Submit = new RoutedUICommand();
        }

        public _LoginWindow()
        {
            InitializeComponent();
            InitializeCommandBindings();
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(Commands.Submit, Submit, CanSubmit));
        }

        private void Submit(object sender, ExecutedRoutedEventArgs e)
        {
            if (_presenter.SubmitInput())
                Close();
        }

        private void CanSubmit(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.CanSubmitInput;
        }

        private Presenter _presenter;
        public void Show(Window ownerWindow)
        {
            _presenter = new Presenter(this);
            Owner = ownerWindow;
            ShowDialog();
        }
    }
}
