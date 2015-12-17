using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataRowView : Control
    {
        public static readonly DependencyProperty PresenterProperty = DependencyProperty.Register(nameof(Presenter),
            typeof(DataRowPresenter), typeof(DataRowView), new FrameworkPropertyMetadata(null));

        public DataRowPresenter Presenter
        {
            get { return (DataRowPresenter)GetValue(PresenterProperty); }
            set { SetValue(PresenterProperty, value); }
        }
    }
}
