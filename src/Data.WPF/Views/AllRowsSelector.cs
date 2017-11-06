using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Views
{
    public class AllRowsSelector : ToggleButton
    {
        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(AllRowsSelector), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility),
            typeof(AllRowsSelector), new FrameworkPropertyMetadata(Visibility.Visible));

        static AllRowsSelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AllRowsSelector), new FrameworkPropertyMetadata(typeof(AllRowsSelector)));
        }

        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }
    }
}
