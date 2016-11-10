using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace DevZest.Data.Windows.Primitives
{
    internal static class ValidationListener
    {
        private static readonly DependencyProperty DummyProperty = DependencyProperty.RegisterAttached("DummyProperty", typeof(INotifyDataErrorInfo), typeof(Input),
            new PropertyMetadata(null));

        public static void AddValidationListener(this DependencyObject element, INotifyDataErrorInfo notifyDataErrorInfo)
        {
            var binding = new System.Windows.Data.Binding(".") { Source = notifyDataErrorInfo, Mode = BindingMode.TwoWay, ValidatesOnNotifyDataErrors = true };
            BindingOperations.SetBinding(element, DummyProperty, binding);
        }

        public static void RemoveValidationListener(this DependencyObject element)
        {
            BindingOperations.ClearBinding(element, DummyProperty);
        }
    }
}
