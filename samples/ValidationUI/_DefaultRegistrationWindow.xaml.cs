using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ValidationUI
{
    /// <summary>
    /// Interaction logic for _DefaultRegistrationWindow.xaml
    /// </summary>
    public partial class _DefaultRegistrationWindow : Window
    {
        private sealed class Presenter : _RegistrationPresenter
        {
            public Presenter(_DefaultRegistrationWindow window)
            {
                var userName = _userName.BindToTextBox();
                var password = _password.BindToPasswordBox();
                var confirmPassword = _passwordConfirmation.BindToPasswordBox();
                var interests1 = _interests.BindToCheckBox(Interests.Books);
                var interests2 = _interests.BindToCheckBox(Interests.Comics);
                var interests3 = _interests.BindToCheckBox(Interests.Hunting);
                var interests4 = _interests.BindToCheckBox(Interests.Movies);
                var interests5 = _interests.BindToCheckBox(Interests.Music);
                var interests6 = _interests.BindToCheckBox(Interests.Physics);
                var interests7 = _interests.BindToCheckBox(Interests.Shopping);
                var interests8 = _interests.BindToCheckBox(Interests.Sports);

                Attach(window._textBoxUserName, userName);
                Attach(window._textBoxEmailAddress, _emailAddress.BindToTextBox());
                Attach(window._passwordBoxPassword, password);
                Attach(window._passwordBoxConfirmPassword, confirmPassword);
                //Attach(window._passwordMismatch, new ScalarBinding[] { password, confirmPassword }.BindToValidationPlaceholder());
                Attach(window._checkBoxInterests1, interests1);
                Attach(window._checkBoxInterests2, interests2);
                Attach(window._checkBoxInterests3, interests3);
                Attach(window._checkBoxInterests4, interests4);
                Attach(window._checkBoxInterests5, interests5);
                Attach(window._checkBoxInterests6, interests6);
                Attach(window._checkBoxInterests7, interests7);
                Attach(window._checkBoxInterests8, interests8);
                //Attach(window._interestsValidation, new ScalarBinding[] { interests1, interests2, interests3, interests4, interests5, interests6, interests7, interests8 }.BindToValidationPlaceholder());
                Attach(window._validationErrorsControl, this.BindToValidationErrorsControl());
                Show(window._view);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                //builder.AddAsyncValidator(userName.Input, ValidateUserNameFunc, "User Name");
            }
        }

        public static class Commands
        {
            public static readonly RoutedUICommand Submit = new RoutedUICommand();
        }

        public _DefaultRegistrationWindow()
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
