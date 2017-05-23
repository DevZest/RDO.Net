using DevZest.Windows.Primitives;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Windows
{
    public sealed class PropertyChangedTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        public PropertyChangedTrigger(DependencyProperty property)
        {
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
