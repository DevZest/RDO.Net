using DevZest.Data;
using System.Windows;

namespace ValidationUI
{
    /// <summary>
    /// Interaction logic for DefaultUserLoginForm.xaml
    /// </summary>
    public partial class DefaultUserLoginWindow : Window
    {
        public DefaultUserLoginWindow()
        {
            InitializeComponent();
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

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            _presenter.RowValidation.Validate();
            if (!_presenter.HasVisibleInputError)
                Close();
        }
    }
}
