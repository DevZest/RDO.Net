using System.Windows;
using System.Windows.Media;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for DirectoryTreeItemView.xaml
    /// </summary>
    public partial class DirectoryTreeItemView
    {
        public DirectoryTreeItemView()
        {
            InitializeComponent();
        }

        private int _depth;
        public int Depth
        {
            get { return _depth; }
            private set
            {
                _depth = value;
                _rowExpander.Margin = new Thickness(15 * value, 0, 0, 0);
            }
        }

        public ImageSource Icon
        {
            get { return _icon.Source; }
            private set { _icon.Source = value; }
        }

        public string Text
        {
            get { return _textBlock.Text; }
            private set { _textBlock.Text = value; }
        }

        public void Refresh(int depth, ImageSource icon, string text)
        {
            Depth = depth;
            Icon = icon;
            Text = text;
        }
    }
}
