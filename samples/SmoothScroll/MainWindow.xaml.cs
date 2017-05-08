using System.Windows;

namespace SmoothScroll
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            new _FooList().Show(dataView, Foo.Mock(10000));
        }
    }
}
