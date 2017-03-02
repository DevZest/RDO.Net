using System.Windows;
using System.Windows.Controls;

namespace DevZest.Windows.Data
{
    public class RowHeader : Control
    {
        public static readonly DependencyProperty IsCurrentProperty = DependencyProperty.Register(nameof(IsCurrent), typeof(bool),
            typeof(RowHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));

        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(nameof(IsEditing), typeof(bool),
            typeof(RowHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool),
            typeof(RowHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));

        static RowHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowHeader), new FrameworkPropertyMetadata(typeof(RowHeader)));
        }

        public bool IsCurrent
        {
            get { return (bool)GetValue(IsCurrentProperty); }
            set { SetValue(IsCurrentProperty, BooleanBoxes.Box(value)); }
        }

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, BooleanBoxes.Box(value)); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, BooleanBoxes.Box(value)); }
        }
    }
}
