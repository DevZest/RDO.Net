using DevZest.Data.Windows.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        private static void SetSeverity(this DependencyObject element, ValidationSeverity? value)
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

        /// <summary>DataErrorInfo implements INotifyPropertyChanged to avoid possible memory leak:
        /// https://blogs.msdn.microsoft.com/micmcd/2008/03/07/avoiding-a-wpf-memory-leak-with-databinding-black-magic/
        /// </summary>
        private sealed class DataErrorInfo : INotifyDataErrorInfo, INotifyPropertyChanged
        {
            public DataErrorInfo(IReadOnlyList<ValidationMessage> messages)
            {
                Debug.Assert(messages != null && messages.Count > 0);
                _messages = messages;
            }

            private IReadOnlyList<ValidationMessage> _messages;
            public IReadOnlyList<ValidationMessage> Messages
            {
                get { return _messages; }
                set
                {
                    Debug.Assert(value != null && value.Count > 0);
                    if (_messages == value)
                        return;
                    _messages = value;
                    var errorsChanged = ErrorsChanged;
                    if (errorsChanged != null)
                        errorsChanged(this, Singleton.DataErrorsChangedEventArgs);
                }
            }

            public bool HasErrors
            {
                get { return _messages.Count > 0; }
            }

            public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

            /// Implements INotifyPropertyChanged to avoid possible memory leak:
            /// https://blogs.msdn.microsoft.com/micmcd/2008/03/07/avoiding-a-wpf-memory-leak-with-databinding-black-magic/
            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                var propertyChanged = PropertyChanged;
                if (propertyChanged != null)
                    propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            public IEnumerable GetErrors(string propertyName)
            {
                /// Workaround: <param name="propertyName"/> will be passed in twice, as null and <see cref="string.Empty"/> respectively.
                /// We need to ignore one of them, otherwise duplicated results will be returned.
                return propertyName == null ? null : _messages;
            }
        }

        private static readonly DependencyProperty DummyProperty = DependencyProperty.RegisterAttached("DummyProperty", typeof(INotifyDataErrorInfo),
            typeof(Validation), new PropertyMetadata(null));

        private static void SetDataErrorInfoBinding(this DependencyObject element, INotifyDataErrorInfo dataErrorInfo)
        {
            Debug.Assert(dataErrorInfo != null);
            var binding = new System.Windows.Data.Binding(".") { Source = dataErrorInfo, Mode = BindingMode.TwoWay, ValidatesOnNotifyDataErrors = true };
            BindingOperations.SetBinding(element, DummyProperty, binding);
        }

        private static void ClearDataErrorInfoBinding(this DependencyObject element)
        {
            BindingOperations.ClearBinding(element, DummyProperty);
        }

        internal static void SetDataErrorInfo(this DependencyObject element, IReadOnlyList<ValidationMessage> errors, IReadOnlyList<ValidationMessage> warnings)
        {
            if (errors != null && errors.Count > 0)
                element.SetDataErrorInfo(ValidationSeverity.Error, errors);
            else if (warnings != null && warnings.Count > 0)
                element.SetDataErrorInfo(ValidationSeverity.Warning, warnings);
            else
                element.ClearDataErrorInfo();
        }

        private static void SetDataErrorInfo(this DependencyObject element, ValidationSeverity severity, IReadOnlyList<ValidationMessage> messages)
        {
            Debug.Assert(messages != null && messages.Count > 0);

            element.SetSeverity(severity);

            var binding = BindingOperations.GetBinding(element, DummyProperty);
            if (binding == null)
                element.SetDataErrorInfoBinding(new DataErrorInfo(messages));
            else
            {
                var dataErrorInfo = (DataErrorInfo)binding.Source;
                dataErrorInfo.Messages = messages;
            }
        }

        private static void ClearDataErrorInfo(this DependencyObject element)
        {
            element.SetSeverity(null);
            element.ClearDataErrorInfoBinding();
        }
    }
}
