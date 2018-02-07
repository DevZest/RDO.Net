using DevZest.Data.Views;
using System.Windows;
using System.Windows.Media;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for FolderView.xaml
    /// </summary>
    public partial class FolderView
    {
        public FolderView()
        {
            InitializeComponent();
        }

        private int _depth;
        public int Depth
        {
            get { return _depth; }
            set
            {
                _depth = value;
                _rowExpander.Margin = new Thickness(15 * value, 0, 0, 0);
            }
        }

        public ImageSource ImageSource
        {
            get { return _image.Source; }
            set { _image.Source = value; }
        }

        public InPlaceEditor InPlaceEditor
        {
            get { return _inPlaceEditor; }
        }
    }
}
