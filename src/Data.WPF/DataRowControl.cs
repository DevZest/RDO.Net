using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataRowControl : Control
    {
        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(nameof(View), typeof(DataRowManager), typeof(DataRowControl),
            new FrameworkPropertyMetadata(null));

        public DataRowManager View
        {
            get { return (DataRowManager)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }
    }
}
