using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class ValidationPlaceholder : Control
    {
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.RegisterAttached("IsActive", typeof(bool), typeof(ValidationPlaceholder),
            new FrameworkPropertyMetadata(BooleanBoxes.False));

        static ValidationPlaceholder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationPlaceholder), new FrameworkPropertyMetadata(typeof(ValidationPlaceholder)));
        }

        public static bool GetIsActive(DependencyObject element)
        {
            return (bool)element.GetValue(IsActiveProperty);
        }

        public static void SetIsActive(DependencyObject element, bool value)
        {
            element.SetValue(IsActiveProperty, BooleanBoxes.Box(value));
        }
    }
}
