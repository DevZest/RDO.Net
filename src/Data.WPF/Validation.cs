using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public static class Validation
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
            typeof(ValidationSeverity?), typeof(Validation), new FrameworkPropertyMetadata(SeverityBoxes.Null, new PropertyChangedCallback(OnSeverityChanged)));
        public static readonly DependencyProperty SeverityProperty = SeverityPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ErrorTemplateProperty = DependencyProperty.RegisterAttached("ErrorTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(CreateDefaultErrorTemplate(), FrameworkPropertyMetadataOptions.NotDataBindable, new PropertyChangedCallback(OnErrorTemplateChanged)));

        public static readonly DependencyProperty WarningTemplateProperty = DependencyProperty.RegisterAttached("WarningTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(CreateDefaultWarningTemplate(), FrameworkPropertyMetadataOptions.NotDataBindable, new PropertyChangedCallback(OnWarningTemplateChanged)));

        public static ValidationSeverity? GetSeverity(this DependencyObject element)
        {
            return (ValidationSeverity?)element.GetValue(SeverityProperty);
        }

        internal static void SetSeverity(this DependencyObject element, ValidationSeverity? value)
        {
            if (!value.HasValue)
                element.ClearValue(SeverityPropertyKey);
            else
                element.SetValue(SeverityPropertyKey, SeverityBoxes.Box(value.GetValueOrDefault()));
        }

        public static ControlTemplate GetErrorTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(ErrorTemplateProperty);
        }

        public static void SetErrorTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(ErrorTemplateProperty, value);
        }

        public static ControlTemplate GetWarningTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(WarningTemplateProperty);
        }

        public static void SetWarningTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(WarningTemplateProperty, value);
        }

        private static ControlTemplate CreateDefaultTemplate(Brush borderBrush)
        {
            ControlTemplate controlTemplate = new ControlTemplate(typeof(Control));
            FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(Border), "Border");
            frameworkElementFactory.SetValue(Border.BorderBrushProperty, borderBrush);
            frameworkElementFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1.0));
            FrameworkElementFactory child = new FrameworkElementFactory(typeof(AdornedElementPlaceholder), "Placeholder");
            frameworkElementFactory.AppendChild(child);
            controlTemplate.VisualTree = frameworkElementFactory;
            controlTemplate.Seal();
            return controlTemplate;
        }

        private static ControlTemplate CreateDefaultErrorTemplate()
        {
            return CreateDefaultTemplate(Brushes.Red);
        }

        private static ControlTemplate CreateDefaultWarningTemplate()
        {
            return CreateDefaultTemplate(Brushes.Yellow);
        }

        private static void OnSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == SeverityBoxes.Error)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetErrorTemplate());
            else if (e.NewValue == SeverityBoxes.Warning)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetWarningTemplate());
            else
            {
                Debug.Assert(e.NewValue == SeverityBoxes.Null);
                d.ClearValue(System.Windows.Controls.Validation.ErrorTemplateProperty);
            }
        }

        private static void OnErrorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetValue(SeverityProperty) == SeverityBoxes.Error)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        private static void OnWarningTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetValue(SeverityProperty) == SeverityBoxes.Warning)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }
    }
}
