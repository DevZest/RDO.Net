using DevZest.Data;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace DevZest.Data.Presenters
{
    public static class Validation
    {
        internal sealed class TemplateId : ResourceId<ControlTemplate>
        {
            public TemplateId(Type type)
                : base(type)
            {
            }

            protected override string UriSuffix
            {
                get { return "Templates"; }
            }
        }

        internal static class Templates
        {
            public static readonly TemplateId FlushError = new TemplateId(typeof(Validation));
            public static readonly TemplateId Error = new TemplateId(typeof(Validation));
            public static readonly TemplateId Warning = new TemplateId(typeof(Validation));
        }

        private static class MessageTypeBoxes
        {
            public static readonly object Null = new ValidationMessageType?();
            public static readonly object FlushError = new ValidationMessageType?(ValidationMessageType.FlushError);
            public static readonly object Error = new ValidationMessageType?(ValidationMessageType.Error);
            public static readonly object Warning = new ValidationMessageType?(ValidationMessageType.Warning);

            public static object Box(ValidationMessageType value)
            {
                return value == ValidationMessageType.Error ? Error : (value == ValidationMessageType.FlushError ? FlushError : Warning);
            }
        }

        private static readonly DependencyPropertyKey MessageTypePropertyKey = DependencyProperty.RegisterAttachedReadOnly("MessageType",
            typeof(ValidationMessageType?), typeof(Validation), new FrameworkPropertyMetadata(MessageTypeBoxes.Null, new PropertyChangedCallback(OnMessageTypeChanged)));
        public static readonly DependencyProperty MessageTypeProperty = MessageTypePropertyKey.DependencyProperty;

        public static readonly DependencyProperty FlushErrorTemplateProperty = DependencyProperty.RegisterAttached("FlushErrorTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(Templates.FlushError.GetOrLoad(), FrameworkPropertyMetadataOptions.NotDataBindable, new PropertyChangedCallback(OnFlushErrorTemplateChanged)));

        public static readonly DependencyProperty ErrorTemplateProperty = DependencyProperty.RegisterAttached("ErrorTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(Templates.Error.GetOrLoad(), FrameworkPropertyMetadataOptions.NotDataBindable, new PropertyChangedCallback(OnErrorTemplateChanged)));

        public static readonly DependencyProperty WarningTemplateProperty = DependencyProperty.RegisterAttached("WarningTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(Templates.Warning.GetOrLoad(), FrameworkPropertyMetadataOptions.NotDataBindable, new PropertyChangedCallback(OnWarningTemplateChanged)));

        public static ValidationMessageType? GetMessageType(this DependencyObject element)
        {
            return (ValidationMessageType?)element.GetValue(MessageTypeProperty);
        }

        private static void SetMessageType(this DependencyObject element, ValidationMessageType? value)
        {
            if (!value.HasValue)
                element.ClearValue(MessageTypePropertyKey);
            else
                element.SetValue(MessageTypePropertyKey, MessageTypeBoxes.Box(value.GetValueOrDefault()));
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

        public static ControlTemplate GetWarningTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(WarningTemplateProperty);
        }

        public static void SetWarningTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(WarningTemplateProperty, value);
        }

        private static void OnMessageTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == MessageTypeBoxes.Error)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetErrorTemplate());
            else if (e.NewValue == MessageTypeBoxes.FlushError)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetFlushErrorTemplate());
            else if (e.NewValue == MessageTypeBoxes.Warning)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetWarningTemplate());
            else
            {
                Debug.Assert(e.NewValue == MessageTypeBoxes.Null);
                d.ClearValue(System.Windows.Controls.Validation.ErrorTemplateProperty);
            }
        }

        private static void OnErrorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetValue(MessageTypeProperty) == MessageTypeBoxes.Error)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        private static void OnFlushErrorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetValue(MessageTypeProperty) == MessageTypeBoxes.FlushError)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        private static void OnWarningTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetValue(MessageTypeProperty) == MessageTypeBoxes.Warning)
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

        internal static void RefreshValidation(this DependencyObject element, 
            Func<FlushErrorMessage> getFlushError, Func<IReadOnlyList<ValidationMessage>> getErrors, Func<IReadOnlyList<ValidationMessage>> getWarnings)
        {
            var flushError = getFlushError();
            if (flushError != null)
            {
                element.SetDataErrorInfo(ValidationMessageType.FlushError, new FlushErrorMessage[] { flushError });
                return;
            }

            var errors = getErrors();
            if (errors != null && errors.Count > 0)
            {
                element.SetDataErrorInfo(ValidationMessageType.Error, errors);
                return;
            }

            var warnings = getWarnings();
            if (warnings != null && warnings.Count > 0)
            {
                element.SetDataErrorInfo(ValidationMessageType.Warning, warnings);
                return;
            }

            element.ClearDataErrorInfo();
        }

        private static void SetDataErrorInfo(this DependencyObject element, ValidationMessageType messageType, IEnumerable messages)
        {
            Debug.Assert(messages != null);

            element.SetMessageType(messageType);

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
    }
}
