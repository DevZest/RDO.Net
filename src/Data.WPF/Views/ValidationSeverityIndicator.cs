using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class ValidationSeverityIndicator : Control
    {
        public static readonly DependencyProperty ValidationSeverityProperty = DependencyProperty.Register(nameof(ValidationSeverity), typeof(ValidationSeverity?),
            typeof(ValidationSeverityIndicator), new FrameworkPropertyMetadata(null));

        static ValidationSeverityIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationSeverityIndicator), new FrameworkPropertyMetadata(typeof(ValidationSeverityIndicator)));
        }

        public ValidationSeverity? ValidationSeverity
        {
            get { return (ValidationSeverity?)GetValue(ValidationSeverityProperty); }
            set { SetValue(ValidationSeverityProperty, value); }
        }
    }
}
