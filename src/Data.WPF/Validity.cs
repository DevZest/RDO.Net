using System.Windows;

namespace DevZest.Data.Windows
{
    public static class Validity
    {
        private static class SeverityBoxes
        {
            public static readonly object Null = new ValidationSeverity?();
            public static readonly object Error = new ValidationSeverity?(ValidationSeverity.Error);
            public static readonly object Warning = new ValidationSeverity?(ValidationSeverity.Warning);

            public static object Box(ValidationSeverity value)
            {
                return value == ValidationSeverity.Error ? Error : Warning;
            }
        }

        private static readonly DependencyPropertyKey SeverityPropertyKey = DependencyProperty.RegisterAttachedReadOnly("Severity",
            typeof(ValidationSeverity?), typeof(Validity), new FrameworkPropertyMetadata(SeverityBoxes.Null));
        public static readonly DependencyProperty SeverityProperty = SeverityPropertyKey.DependencyProperty;

        public static ValidationSeverity? GetSeverity(DependencyObject element)
        {
            return (ValidationSeverity?)element.GetValue(SeverityProperty);
        }

        internal static void SetSeverity(DependencyObject element, ValidationSeverity? value)
        {
            if (value.HasValue)
                element.SetValue(SeverityPropertyKey, SeverityBoxes.Box(value.GetValueOrDefault()));
            else
                element.ClearValue(SeverityPropertyKey);
        }
    }
}
