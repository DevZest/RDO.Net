using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Presenters
{
    public static class Validation
    {
        internal static class Templates
        {
            public static readonly TemplateId Error = new TemplateId(typeof(Validation));
            public static readonly TemplateId Validating = new TemplateId(typeof(Validation));
        }

        private static readonly DependencyPropertyKey StatusPropertyKey = DependencyProperty.RegisterAttachedReadOnly("Status",
            typeof(ValidationStatus?), typeof(Validation), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnStatusChanged)));
        public static readonly DependencyProperty StatusProperty = StatusPropertyKey.DependencyProperty;

        public static readonly DependencyProperty FlushingErrorTemplateProperty = DependencyProperty.RegisterAttached("FlushingErrorTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(Templates.Error.GetOrLoad(false),
                FrameworkPropertyMetadataOptions.NotDataBindable | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnFlushingErrorTemplateChanged)));

        public static readonly DependencyProperty DataErrorTemplateProperty = DependencyProperty.RegisterAttached("DataErrorTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(Templates.Error.GetOrLoad(false),
                FrameworkPropertyMetadataOptions.NotDataBindable | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDataErrorTemplateChanged)));

        public static readonly DependencyProperty ValidatingTemplateProperty = DependencyProperty.RegisterAttached("ValidatingTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(Templates.Validating.GetOrLoad(false),
                FrameworkPropertyMetadataOptions.NotDataBindable | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnValidatingTemplateChanged)));

        public static ValidationStatus? GetStatus(this DependencyObject element)
        {
            return (ValidationStatus?)element.GetValue(StatusProperty);
        }

        private static void SetStatus(this DependencyObject element, ValidationStatus? value)
        {
            if (!value.HasValue)
                element.ClearValue(StatusPropertyKey);
            else
                element.SetValue(StatusPropertyKey, value);
        }

        public static ControlTemplate GetFlushingErrorTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(FlushingErrorTemplateProperty);
        }

        public static void SetFlushingErrorTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(FlushingErrorTemplateProperty, value);
        }

        public static ControlTemplate GetDataErrorTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(DataErrorTemplateProperty);
        }

        public static void SetDataErrorTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(DataErrorTemplateProperty, value);
        }

        public static ControlTemplate GetValidatingTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(ValidatingTemplateProperty);
        }

        public static void SetValidatingTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(ValidatingTemplateProperty, value);
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var status = (ValidationStatus?)e.NewValue;
            if (status == ValidationStatus.DataError)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetDataErrorTemplate());
            else if (status == ValidationStatus.FlushingError)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetFlushingErrorTemplate());
            else if (status == ValidationStatus.Validating)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetValidatingTemplate());
            else
            {
                Debug.Assert(status == null);
                d.ClearValue(System.Windows.Controls.Validation.ErrorTemplateProperty);
            }
        }

        private static void OnDataErrorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetStatus() == ValidationStatus.DataError)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        private static void OnFlushingErrorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetStatus() == ValidationStatus.FlushingError)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        private static void OnValidatingTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetStatus() == ValidationStatus.Validating)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        /// <summary>DataErrorInfo implements INotifyPropertyChanged to avoid possible memory leak:
        /// https://blogs.msdn.microsoft.com/micmcd/2008/03/07/avoiding-a-wpf-memory-leak-with-databinding-black-magic/
        /// </summary>
        private sealed class DataErrorInfo : INotifyDataErrorInfo, INotifyPropertyChanged
        {
            public DataErrorInfo(IEnumerable messages)
            {
                Debug.Assert(messages != null);
                _messages = messages;
            }

            private IEnumerable _messages;
            public IEnumerable Messages
            {
                get { return _messages; }
                set
                {
                    Debug.Assert(value != null);
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
                get { return _messages != null; }
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

        internal static void RefreshValidation(this DependencyObject element, IValidationErrors errors)
        {
            var status = GetStatus(errors);
            if (status.HasValue)
                element.SetDataErrorInfo(status.Value, errors);
            else
                element.ClearDataErrorInfo();
        }

        private static ValidationStatus? GetStatus(IValidationErrors errors)
        {
            if (errors == null)
                return ValidationStatus.Validating;
            else if (errors.Count == 0)
                return null;
            else if (errors.Count == 1 && errors[0] is FlushingError)
                return ValidationStatus.FlushingError;
            else
                return ValidationStatus.DataError;
        }

        private static void SetDataErrorInfo(this DependencyObject element, ValidationStatus errorType, IEnumerable messages)
        {
            if (messages == null)
                messages = new string[] { UserMessages.Validation_Validating };

            element.SetStatus(errorType);

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
            element.SetStatus(null);
            element.ClearDataErrorInfoBinding();
        }

        internal static IValidationErrors Merge(this IValidationErrors result, IValidationErrors errors)
        {
            for (int i = 0; i < errors.Count; i++)
                result = result.Add(errors[i]);
            return result;
        }
    }
}
