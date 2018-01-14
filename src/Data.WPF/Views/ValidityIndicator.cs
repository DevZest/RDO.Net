using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class ValidityIndicator : Control
    {
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register(nameof(IsValid), typeof(bool),
            typeof(ValidityIndicator), new FrameworkPropertyMetadata(BooleanBoxes.False));

        static ValidityIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidityIndicator), new FrameworkPropertyMetadata(typeof(ValidityIndicator)));
        }

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, BooleanBoxes.Box(value)); }
        }
    }
}
