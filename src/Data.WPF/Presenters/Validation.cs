using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
        }

        private static class ErrorTypeBoxes
        {
            public static readonly object Null = new ValidationErrorType?();
            public static readonly object FlushError = new ValidationErrorType?(ValidationErrorType.FlushError);
            public static readonly object Error = new ValidationErrorType?(ValidationErrorType.Error);

            public static object Box(ValidationErrorType value)
            {
                return value == ValidationErrorType.Error ? Error : FlushError;
            }
        }

        private static readonly DependencyPropertyKey ErrorTypePropertyKey = DependencyProperty.RegisterAttachedReadOnly("ErrorType",
            typeof(ValidationErrorType?), typeof(Validation), new FrameworkPropertyMetadata(ErrorTypeBoxes.Null, new PropertyChangedCallback(OnErrorTypeChanged)));
        public static readonly DependencyProperty MessageTypeProperty = ErrorTypePropertyKey1.DependencyProperty;

        public static readonly DependencyProperty FlushErrorTemplateProperty = DependencyProperty.RegisterAttached("FlushErrorTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(Templates.Error.GetOrLoad(false),
                FrameworkPropertyMetadataOptions.NotDataBindable | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnFlushErrorTemplateChanged)));

        public static readonly DependencyProperty ErrorTemplateProperty = DependencyProperty.RegisterAttached("ErrorTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(Templates.Error.GetOrLoad(false),
                FrameworkPropertyMetadataOptions.NotDataBindable | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnErrorTemplateChanged)));

        public static ValidationErrorType? GetMessageType(this DependencyObject element)
        {
            return (ValidationErrorType?)element.GetValue(MessageTypeProperty);
        }

        private static void SetMessageType(this DependencyObject element, ValidationErrorType? value)
        {
            if (!value.HasValue)
                element.ClearValue(ErrorTypePropertyKey1);
            else
                element.SetValue(ErrorTypePropertyKey1, ErrorTypeBoxes.Box(value.GetValueOrDefault()));
        }

        public static ControlTemplate GetFlushErrorTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(FlushErrorTemplateProperty);
        }

        public static void SetFlushErrorTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(FlushErrorTemplateProperty, value);
        }

        public static ControlTemplate GetErrorTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(ErrorTemplateProperty);
        }

        public static void SetErrorTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(ErrorTemplateProperty, value);
        }

        private static void OnErrorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == ErrorTypeBoxes.Error)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetErrorTemplate());
            else if (e.NewValue == ErrorTypeBoxes.FlushError)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetFlushErrorTemplate());
            else
            {
                Debug.Assert(e.NewValue == ErrorTypeBoxes.Null);
                d.ClearValue(System.Windows.Controls.Validation.ErrorTemplateProperty);
            }
        }

        private static void OnErrorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetValue(MessageTypeProperty) == ErrorTypeBoxes.Error)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        private static void OnFlushErrorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetValue(MessageTypeProperty) == ErrorTypeBoxes.FlushError)
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

        public static DependencyPropertyKey ErrorTypePropertyKey1 => ErrorTypePropertyKey;

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
            var errorType = GetValidationErrorType(errors);
            if (errorType.HasValue)
                element.SetDataErrorInfo(errorType.Value, errors);
            else
                element.ClearDataErrorInfo();
        }

        private static ValidationErrorType? GetValidationErrorType(IValidationErrors errors)
        {
            if (errors == null || errors.Count == 0)
                return null;

            if (errors.Count == 1 && errors[0] is FlushError)
                return ValidationErrorType.FlushError;
            else
                return ValidationErrorType.Error;
        }

        private static void SetDataErrorInfo(this DependencyObject element, ValidationErrorType errorType, IEnumerable messages)
        {
            Debug.Assert(messages != null);

            element.SetMessageType(errorType);

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
            element.SetMessageType(null);
            element.ClearDataErrorInfoBinding();
        }

        internal static IValidationErrors Merge(this IValidationErrors result, IValidationErrors errors)
        {
            for (int i = 0; i < errors.Count; i++)
                result = result.Add(errors[i]);
            return result;
        }

        public static ScalarAsyncValidator CreateAsyncValidator(this IScalars sourceScalars, Func<Task<string>> validator)
        {
            if (sourceScalars == null || sourceScalars.Count == 0)
                throw new ArgumentNullException(nameof(sourceScalars));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            return ScalarAsyncValidator.Create(sourceScalars, validator);
        }

        public static ScalarAsyncValidator CreateAsyncValidator(this IScalars sourceScalars, Func<Task<IEnumerable<string>>> validator)
        {
            if (sourceScalars == null || sourceScalars.Count == 0)
                throw new ArgumentNullException(nameof(sourceScalars));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            return ScalarAsyncValidator.Create(sourceScalars, validator);
        }

        public static RowAsyncValidator CreateAsyncValidator(this IColumns sourceColumns, Func<DataRow, Task<string>> validator)
        {
            if (sourceColumns == null || sourceColumns.Count == 0)
                throw new ArgumentNullException(nameof(sourceColumns));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            return RowAsyncValidator.Create(sourceColumns, validator);
        }

        public static RowAsyncValidator CreateAsyncValidator(this IColumns sourceColumns, Func<DataRow, Task<IEnumerable<string>>> validator)
        {
            if (sourceColumns == null || sourceColumns.Count == 0)
                throw new ArgumentNullException(nameof(sourceColumns));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            return RowAsyncValidator.Create(sourceColumns, validator);
        }
    }
}
