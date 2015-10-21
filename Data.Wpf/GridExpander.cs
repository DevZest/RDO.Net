using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class GridExpander : Control
    {
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(nameof(GridExpander.IsExpanded),
            typeof(bool), typeof(GridExpander));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, BooleanBoxes.Box(value)); }
        }
    }
}
