using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ValidationUI
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        private const string LABEL_FORMAT = "{0}:";

        private static string[] s_takenUserNames = new string[] { "paul", "john", "tony" };
        private static string[] s_errorNames = new string[] { "error" };
        private static int s_retryCount = 0;

        internal static async Task<string> PerformValidateUserName(string userName)
        {
            await Task.Delay(1000);
            if (s_takenUserNames.Any(x => x == userName))
                return string.Format("User name '{0}' is taken.", userName);

            if (s_errorNames.Any(x => x == userName))
            {
                s_retryCount++;
                if (s_retryCount % 2 == 1)
                    throw new InvalidOperationException("An exception is thrown");
            }

            return null;
        }

        private sealed class Presenter : DataPresenter<Registration>
        {
            private Func<DataRow, Task<string>> ValidateUserNameFunc
            {
                get { return ValidateUserName; }
            }

            private Task<string> ValidateUserName(DataRow dataRow)
            {
                return PerformValidateUserName(_.UserName[dataRow]);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var userName = _.UserName.BindToTextBox();
                var emailAddress = _.EmailAddress.BindToTextBox();
                var password = _.Password.BindToPasswordBox();
                var confirmPassword = _.PasswordConfirmation.BindToPasswordBox();
                var passwordMismatch = new RowBinding[] { password, confirmPassword }.BindToValidationPlaceholder();
                var interests1 = _.Interests.BindToCheckBox(Interests.Books);
                var interests2 = _.Interests.BindToCheckBox(Interests.Comics);
                var interests3 = _.Interests.BindToCheckBox(Interests.Hunting);
                var interests4 = _.Interests.BindToCheckBox(Interests.Movies);
                var interests5 = _.Interests.BindToCheckBox(Interests.Music);
                var interests6 = _.Interests.BindToCheckBox(Interests.Physics);
                var interests7 = _.Interests.BindToCheckBox(Interests.Shopping);
                var interests8 = _.Interests.BindToCheckBox(Interests.Sports);
                var interestsValidation = new RowBinding[] { interests1, interests2, interests3, interests4, interests5, interests6, interests7, interests8 }.BindToValidationPlaceholder();
                builder
                    .GridColumns("Auto", "*", "*")
                    .GridRows("Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto")
                    .AddBinding(1, 2, 2, 3, passwordMismatch)
                    .AddBinding(1, 4, 2, 7, interestsValidation)
                    .AddBinding(0, 0, _.UserName.BindToLabel(userName, LABEL_FORMAT))
                    .AddBinding(0, 1, _.EmailAddress.BindToLabel(emailAddress, LABEL_FORMAT))
                    .AddBinding(0, 2, _.Password.BindToLabel(password, LABEL_FORMAT))
                    .AddBinding(0, 3, _.PasswordConfirmation.BindToLabel(confirmPassword, LABEL_FORMAT))
                    .AddBinding(0, 4, 0, 7, _.Interests.BindToLabel(interests1, LABEL_FORMAT))
                    .AddBinding(1, 0, 2, 0, userName)
                    .AddBinding(1, 1, 2, 1, emailAddress)
                    .AddBinding(1, 2, 2, 2, password)
                    .AddBinding(1, 3, 2, 3, confirmPassword)
                    .AddBinding(1, 4, interests1)
                    .AddBinding(2, 4, interests2)
                    .AddBinding(1, 5, interests3)
                    .AddBinding(2, 5, interests4)
                    .AddBinding(1, 6, interests5)
                    .AddBinding(2, 6, interests6)
                    .AddBinding(1, 7, interests7)
                    .AddBinding(2, 7, interests8)
                    .AddBinding(0, 8, 2, 8, _.BindToValidationErrorsControl().WithAutoSizeWaiver(AutoSizeWaiver.Width))
                    .AddAsyncValidator(userName.Input, ValidateUserNameFunc);
            }
        }

        public static class Commands
        {
            public static readonly RoutedUICommand Submit = new RoutedUICommand();
        }

        public RegistrationWindow()
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
            var dataSet = DataSet<Registration>.New();
            dataSet.Add(new DataRow());
            var presenter = new Presenter();
            _presenter = presenter;
            presenter.Show(_dataView, dataSet);
            Owner = ownerWindow;
            ShowDialog();
        }
    }
}
