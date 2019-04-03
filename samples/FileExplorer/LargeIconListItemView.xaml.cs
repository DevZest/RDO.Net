using DevZest.Data.Views;
using System.Windows.Media;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for LargeIconListItemView.xaml
    /// </summary>
    public partial class LargeIconListItemView
    {
        public LargeIconListItemView()
        {
            InitializeComponent();
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
