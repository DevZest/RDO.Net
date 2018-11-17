using System.Windows;

namespace ValidationUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var window = new LoginWindow();
            window.Show(this);
        }

        private void ScalarLogin_Click(object sender, RoutedEventArgs e)
        {
            var window = new _LoginWindow();
            window.Show(this);
        }

        private void DefaultUserRegister_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegistrationWindow();
            window.ShowDefault(this);
        }

        private void VerboseUserRegister_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegistrationWindow();
            window.ShowVerbose(this);
        }

        private void DefaultScalarUserRegister_Click(object sender, RoutedEventArgs e)
        {
            var window = new _DefaultRegistrationWindow();
            window.Show(this);
        }

        private void VerboseScalarUserRegister_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegistrationWindow();
            window.ShowVerboseScalar(this);
        }
    }
}
