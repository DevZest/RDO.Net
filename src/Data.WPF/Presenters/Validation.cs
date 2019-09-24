using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Provides validation related attached properties and utility methods.
    /// </summary>
    public static class Validation
    {
        internal static class TemplateIds
        {
            public static readonly TemplateId Failed = new TemplateId(typeof(Validation));
            public static readonly TemplateId Validating = new TemplateId(typeof(Validation));
            public static readonly TemplateId Succeeded = new TemplateId(typeof(Validation));
        }

        internal static class Templates
        {
            public static ControlTemplate Failed
            {
                get { return TemplateIds.Failed.GetOrLoad(); }
            }

            public static ControlTemplate Validating
            {
                get { return TemplateIds.Validating.GetOrLoad(); }
            }

            public static ControlTemplate Succeeded
            {
                get { return TemplateIds.Succeeded.GetOrLoad(); }
            }
        }

        private static readonly DependencyPropertyKey StatusPropertyKey = DependencyProperty.RegisterAttachedReadOnly("Status",
            typeof(ValidationStatus?), typeof(Validation), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnStatusChanged)));

        /// <summary>
        /// Identifies the Status readonly attached property (<see cref="GetStatus(DependencyObject)"/>).
        /// </summary>
        public static readonly DependencyProperty StatusProperty = StatusPropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the FailedFlushingTemplate attached property (<see cref="GetFailedFlushingTemplate(DependencyObject)"/>/<see cref="SetFailedFlushingTemplate(DependencyObject, ControlTemplate)"/>).
        /// </summary>
        public static readonly DependencyProperty FailedFlushingTemplateProperty = DependencyProperty.RegisterAttached("FailedFlushingTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(TemplateIds.Failed.GetOrLoad(false),
                FrameworkPropertyMetadataOptions.NotDataBindable | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnFailedFlushingTemplateChanged)));

        /// <summary>
        /// Identifies the FailedTemplate attached property (<see cref="GetFailedTemplate(DependencyObject)"/>/<see cref="SetFailedTemplate(DependencyObject, ControlTemplate)"/>).
        /// </summary>
        public static readonly DependencyProperty FailedTemplateProperty = DependencyProperty.RegisterAttached("FailedTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(TemplateIds.Failed.GetOrLoad(false),
                FrameworkPropertyMetadataOptions.NotDataBindable | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnFailedTemplateChanged)));

        /// <summary>
        /// Identifies the ValidatinigTemplate attached property (<see cref="GetValidatingTemplate(DependencyObject)"/>/<see cref="SetValidatingTemplate(DependencyObject, ControlTemplate)"/>).
        /// </summary>
        public static readonly DependencyProperty ValidatingTemplateProperty = DependencyProperty.RegisterAttached("ValidatingTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(TemplateIds.Validating.GetOrLoad(false),
                FrameworkPropertyMetadataOptions.NotDataBindable | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnValidatingTemplateChanged)));

        /// <summary>
        /// Identifies the SucceededTemplate attached property (<see cref="GetSucceededTemplate(DependencyObject)"/>/<see cref="SetSucceededTemplate(DependencyObject, ControlTemplate)"/>).
        /// </summary>
        public static readonly DependencyProperty SucceededTemplateProperty = DependencyProperty.RegisterAttached("SucceededTemplate",
            typeof(ControlTemplate), typeof(Validation), new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.NotDataBindable | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnSucceededTemplateChanged)));

        /// <summary>
        /// Gets validation status for specified view element. This is the getter of Status attached property.
        /// </summary>
        /// <param name="element">The view element.</param>
        /// <returns>The validation status.</returns>
        public static ValidationStatus? GetStatus(this DependencyObject element)
        {
            return (ValidationStatus?)element.GetValue(StatusProperty);
        }

        private static void SetStatus(this DependencyObject element, ValidationStatus? value)
        {
            if (value == null)
                element.ClearValue(StatusPropertyKey);
            else
                element.SetValue(StatusPropertyKey, value);
        }

        /// <summary>
        /// Gets the control template for specified view element with <see cref="ValidationStatus.FailedFlushing"/> status.
        /// This is the getter of FailedFlushingTemplate attached property.
        /// </summary>
        /// <param name="element">The view element</param>
        /// <returns>The control template.</returns>
        public static ControlTemplate GetFailedFlushingTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(FailedFlushingTemplateProperty);
        }

        /// <summary>
        /// Sets the control template for specified view element with <see cref="ValidationStatus.FailedFlushing"/> status.
        /// This is the setter of FailedFlushingTemplate attached property.
        /// </summary>
        /// <param name="element">The view element</param>
        /// <param name="value">The control template.</param>
        public static void SetFailedFlushingTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(FailedFlushingTemplateProperty, value);
        }

        /// <summary>
        /// Gets the control template for specified view element with <see cref="ValidationStatus.Failed"/> status.
        /// This is the getter of FailedTemplate attached property.
        /// </summary>
        /// <param name="element">The view element</param>
        /// <returns>The control template.</returns>
        public static ControlTemplate GetFailedTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(FailedTemplateProperty);
        }

        /// <summary>
        /// Sets the control template for specified view element with <see cref="ValidationStatus.Failed"/> status.
        /// This is the setter of FailedTemplate attached property.
        /// </summary>
        /// <param name="element">The view element</param>
        /// <param name="value">The control template.</param>
        public static void SetFailedTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(FailedTemplateProperty, value);
        }

        /// <summary>
        /// Gets the control template for specified view element with <see cref="ValidationStatus.Validating"/> status.
        /// This is the getter of ValidatingTemplate attached property.
        /// </summary>
        /// <param name="element">The view element</param>
        /// <returns>The control template.</returns>
        public static ControlTemplate GetValidatingTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(ValidatingTemplateProperty);
        }

        /// <summary>
        /// Sets the control template for specified view element with <see cref="ValidationStatus.Validating"/> status.
        /// This is the setter of ValidatingTemplate attached property.
        /// </summary>
        /// <param name="element">The view element</param>
        /// <param name="value">The control template.</param>
        public static void SetValidatingTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(ValidatingTemplateProperty, value);
        }

        /// <summary>
        /// Gets the control template for specified view element with <see cref="ValidationStatus.Succeeded"/> status.
        /// This is the getter of SucceededTemplate attached property.
        /// </summary>
        /// <param name="element">The view element</param>
        /// <returns>The control template.</returns>
        public static ControlTemplate GetSucceededTemplate(this DependencyObject element)
        {
            return (ControlTemplate)element.GetValue(SucceededTemplateProperty);
        }

        /// <summary>
        /// Sets the control template for specified view element with <see cref="ValidationStatus.Succeeded"/> status.
        /// This is the setter of SucceededTemplate attached property.
        /// </summary>
        /// <param name="element">The view element</param>
        /// <param name="value">The control template.</param>
        public static void SetSucceededTemplate(this DependencyObject element, ControlTemplate value)
        {
            element.SetValue(SucceededTemplateProperty, value);
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var status = (ValidationStatus?)e.NewValue;
            if (status == ValidationStatus.Failed)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetFailedTemplate());
            else if (status == ValidationStatus.FailedFlushing)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetFailedFlushingTemplate());
            else if (status == ValidationStatus.Validating)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetValidatingTemplate());
            else if (status == ValidationStatus.Succeeded)
                System.Windows.Controls.Validation.SetErrorTemplate(d, d.GetSucceededTemplate());
            else
            {
                Debug.Assert(status == null);
                d.ClearValue(System.Windows.Controls.Validation.ErrorTemplateProperty);
            }
        }

        private static void OnFailedTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetStatus() == ValidationStatus.Failed)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        private static void OnFailedFlushingTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetStatus() == ValidationStatus.FailedFlushing)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        private static void OnValidatingTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetStatus() == ValidationStatus.Validating)
                System.Windows.Controls.Validation.SetErrorTemplate(d, (ControlTemplate)e.NewValue);
        }

        private static void OnSucceededTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetStatus() == ValidationStatus.Succeeded)
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
                // Workaround: <param name="propertyName"/> will be passed in twice, as null and <see cref="string.Empty"/> respectively.
                // We need to ignore one of them, otherwise duplicated results will be returned.
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

        internal static void RefreshValidation(this DependencyObject element, ValidationInfo info)
        {
            var status = info.Status;
            element.SetStatus(status);

            if (status == ValidationStatus.Succeeded && element.GetSucceededTemplate() == null)
                status = null;
            if (status.HasValue)
                element.SetDataErrorInfo(info.Messages);
            else
                element.ClearDataErrorInfoBinding();
        }

        private static void SetDataErrorInfo(this DependencyObject v, IEnumerable messages)
        {
            Debug.Assert(messages != null);
            var binding = BindingOperations.GetBinding(v, DummyProperty);
            if (binding == null)
                v.SetDataErrorInfoBinding(new DataErrorInfo(messages));
            else
            {
                var dataErrorInfo = (DataErrorInfo)binding.Source;
                dataErrorInfo.Messages = messages;
            }
        }

        internal static IValidationErrors Merge(this IValidationErrors result, IValidationErrors errors)
        {
            for (int i = 0; i < errors.Count; i++)
                result = result.Add(errors[i]);
            return result;
        }
    }
}
