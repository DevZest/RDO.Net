using DevZest.Data.Presenters;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class ValidationMessageView : Control
    {
        public static class Templates
        {
            public static readonly TemplateId ValidationError = new TemplateId(typeof(ValidationMessageView));
            public static readonly TemplateId ValidationWarning = new TemplateId(typeof(ValidationMessageView));
        }

        private static readonly DependencyPropertyKey SeverityPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Severity), typeof(ValidationSeverity?),
            typeof(ValidationMessageView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SeverityProperty = SeverityPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey DescriptionPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Description), typeof(string),
            typeof(ValidationMessageView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty DescriptionProperty = DescriptionPropertyKey.DependencyProperty;

        static ValidationMessageView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationMessageView), new FrameworkPropertyMetadata(typeof(ValidationMessageView)));
        }

        public ValidationSeverity? Severity
        {
            get { return (ValidationSeverity?)GetValue(SeverityProperty); }
            private set { SetValue(SeverityPropertyKey, value); }
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            private set { SetValue(DescriptionPropertyKey, value); }
        }

        private ValidationMessage _message;
        public ValidationMessage Message
        {
            get { return _message; }
            set
            {
                if (_message == value)
                    return;
                _message = value;
                Severity = value?.Severity;
                Description = value?.Description;
            }
        }
    }
}
