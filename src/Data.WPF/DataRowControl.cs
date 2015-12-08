using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataRowControl : Control
    {
        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(nameof(View), typeof(DataRowView), typeof(DataRowControl),
            new FrameworkPropertyMetadata(null));

        public DataRowView View
        {
            get { return (DataRowView)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }
    }
}
