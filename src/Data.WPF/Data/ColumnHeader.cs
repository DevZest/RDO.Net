using DevZest.Data;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Windows.Data
{
    public class ColumnHeader : Control
    {
        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(nameof(Column), typeof(Column),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(null));

        static ColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnHeader), new FrameworkPropertyMetadata(typeof(ColumnHeader)));
        }

        public Column Column
        {
            get { return (Column)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }
    }
}
