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

        private void DefaultUserLogin_Click(object sender, RoutedEventArgs e)
        {
            var window = new DefaultUserLoginWindow();
            window.Show(this);
        }

        private void LiteUserLogin_Click(object sender, RoutedEventArgs e)
        {
            var window = new LiteUserLoginWindow();
            window.Show(this);
        }

        private void UserRegister_Click(object sender, RoutedEventArgs e)
        {
            var window = new UserRegisterWindow();
            window.Show(this);
        }
    }
}
