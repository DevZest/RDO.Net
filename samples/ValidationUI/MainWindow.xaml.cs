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

        private void UserLogin_Click(object sender, RoutedEventArgs e)
        {
            var window = new UserLoginWindow();
            window.Show(this);
        }

        private void DefaultUserRegister_Click(object sender, RoutedEventArgs e)
        {
            var window = new DefaultUserRegisterWindow();
            window.Show(this);
        }

        private void VerboseUserRegister_Click(object sender, RoutedEventArgs e)
        {
            var window = new VerboseUserRegisterWindow();
            window.Show(this);
        }
    }
}
