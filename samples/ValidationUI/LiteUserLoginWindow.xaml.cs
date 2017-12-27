using DevZest.Data;
using System.Windows;
using System.Windows.Input;

namespace ValidationUI
{
    /// <summary>
    /// Interaction logic for LiteUserLoginForm.xaml
    /// </summary>
    public partial class LiteUserLoginWindow : Window
    {
        public static class Commands
        {
            public static readonly RoutedUICommand Submit = new RoutedUICommand();
        }

        public LiteUserLoginWindow()
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

        private Presenter _presenter = new Presenter();

        public void Show(Window ownerWindow)
        {
            var dataSet = DataSet<User>.New();
            dataSet.Add(new DataRow());
            _presenter.Show(_dataView, dataSet);
            Owner = ownerWindow;
            ShowDialog();
        }
    }
}
