using DevZest.Data;
using DevZest.Data.Presenters;
using System.Windows;
using System.Windows.Input;

namespace ValidationUI
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private sealed class Presenter : DataPresenter<Login>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var emailAddress = _.EmailAddress.BindToTextBox();
                var password = _.Password.BindToPasswordBox();
                builder
                    .WithRowValidationMode(ValidationMode.Implicit)
                    .GridColumns("Auto", "*", "20")
                    .GridRows("Auto", "Auto")
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddress, "{0}:")).AddBinding(1, 0, emailAddress)
                    .AddBinding(0, 1, _.Password.BindToLabel(password, "{0}:")).AddBinding(1, 1, password);
            }
        }

        public static class Commands
        {
            public static readonly RoutedUICommand Submit = new RoutedUICommand();
        }

        public LoginWindow()
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

        private DataPresenter _presenter;

        public void Show(Window ownerWindow)
        {
            var dataSet = DataSet<Login>.New();
            dataSet.Add(new DataRow());
            var presenter = new Presenter();
            _presenter = presenter;
            presenter.Show(_dataView, dataSet);
            Owner = ownerWindow;
            ShowDialog();
        }
    }
}
