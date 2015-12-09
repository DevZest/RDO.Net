using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataRowView : Control
    {
        public static readonly DependencyProperty ManagerProperty = DependencyProperty.Register(nameof(Manager), typeof(DataRowManager), typeof(DataRowView),
            new FrameworkPropertyMetadata(null));

        public DataRowManager Manager
        {
            get { return (DataRowManager)GetValue(ManagerProperty); }
            set { SetValue(ManagerProperty, value); }
        }
    }
}
