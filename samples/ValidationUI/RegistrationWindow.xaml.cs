using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
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
            e.CanExecute = !_presenter.HasVisibleInputError;
        }

        private DataPresenter _presenter;

        public void ShowDefault(Window ownerWindow)
        {
            Show<DefaultPresenter>(ownerWindow, "Default");
        }

        public void ShowVerbose(Window ownerWindow)
        {
            Show<VerbosePresenter>(ownerWindow, "Verbose", ValidationErrorsControl.Templates.ValidationError);
        }

        private void Show<T>(Window ownerWindow, string windowTitleSuffix, ControlTemplate errorTemplate = null)
            where T : DataPresenter<Registration>, new()
        {
            Title = string.Format("{0} ({1})", Title, windowTitleSuffix);
            if (errorTemplate != null)
                DevZest.Data.Presenters.Validation.SetErrorTemplate(_dataView, errorTemplate);
            var dataSet = DataSet<Registration>.New();
            dataSet.Add(new DataRow());
            var presenter = new T();
            _presenter = presenter;
            presenter.Show(_dataView, dataSet);
            Owner = ownerWindow;
            ShowDialog();
        }
    }
}
