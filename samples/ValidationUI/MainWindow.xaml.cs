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
            new LoginWindow().Show(this);
        }

        private void _Login_Click(object sender, RoutedEventArgs e)
        {
            new _LoginWindow().Show(this);
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            new RegistrationWindow().Show(this);
        }

        private void _Registration_Click(object sender, RoutedEventArgs e)
        {
            new _RegistrationWindow().Show(this);
        }
    }
}
