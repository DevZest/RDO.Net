using System;
using System.ComponentModel;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public sealed class PropertyChangedTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        public PropertyChangedTrigger(DependencyProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            _property = property;
        }

        DependencyProperty _property;

        protected internal override void Attach(T element)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
            dpd.AddValueChanged(element, OnPropertyChanged);
        }

        protected internal override void Detach(T element)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
            dpd.RemoveValueChanged(element, OnPropertyChanged);
        }

        private void OnPropertyChanged(object sender, EventArgs e)
        {
            Execute((T)sender);
        }
    }
}
