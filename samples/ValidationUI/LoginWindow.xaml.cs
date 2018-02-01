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

        public void ShowScalar(Window ownerWindow)
        {
            Title = string.Format("{0} ({1})", Title, "ScalarValidation");
            var dataSet = DataSet<DummyModel>.New();
            var presenter = new ScalarPresenter();
            _presenter = presenter;
            presenter.Show(_dataView, dataSet);
            Owner = ownerWindow;
            ShowDialog();
        }
    }
}
