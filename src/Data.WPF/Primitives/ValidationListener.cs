using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace DevZest.Data.Windows.Primitives
{
    internal static class ValidationListener
    {
        private static readonly DependencyProperty DummyProperty = DependencyProperty.RegisterAttached("DummyProperty", typeof(INotifyDataErrorInfo),
            typeof(ValidationListener), new PropertyMetadata(null));

        internal static void AddValidationListener(this DependencyObject element, INotifyDataErrorInfo notifyDataErrorInfo)
        {
            var binding = new System.Windows.Data.Binding(".") { Source = notifyDataErrorInfo, Mode = BindingMode.TwoWay, ValidatesOnNotifyDataErrors = true };
            BindingOperations.SetBinding(element, DummyProperty, binding);
        }

        internal static void RemoveValidationListener(this DependencyObject element)
        {
            BindingOperations.ClearBinding(element, DummyProperty);
        }
    }
}
