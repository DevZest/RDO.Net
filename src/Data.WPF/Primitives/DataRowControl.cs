using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public class DataRowControl : Control
    {
        public static readonly DependencyProperty ManagerProperty = DependencyProperty.Register(nameof(DataRowManager),
            typeof(DataRowManager), typeof(DataRowControl), new FrameworkPropertyMetadata(null));

        public DataRowManager DataRowManager
        {
            get { return (DataRowManager)GetValue(ManagerProperty); }
            set { SetValue(ManagerProperty, value); }
        }
    }
}
