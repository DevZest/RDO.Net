using System.Windows.Controls;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Interaction logic for XamlPane.xaml
    /// </summary>
    public partial class XamlPane
    {
        public XamlPane()
        {
            InitializeComponent();
        }

        public Label Label
        {
            get { return _label; }
        }

        public TextBlock TextBlock
        {
            get { return _textBlock; }
        }
    }
}
