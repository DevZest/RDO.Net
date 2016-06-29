using DevZest.Data.Windows.Primitives;
using System;
using System.ComponentModel;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class BindingTrigger
    {
        public static BindingTrigger Manual
        {
            get { return ManualTrigger.Singleton; }
        }

        public static BindingTrigger LostFocus
        {
            get { return LostFocusTrigger.Singleton; }
        }

        public static BindingTrigger PropertyChanged(DependencyProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            return new DependencyPropertyChangedTrigger(property);
        }

        private sealed class ManualTrigger : BindingTrigger
        {
            public static readonly ManualTrigger Singleton = new ManualTrigger();

            private ManualTrigger()
            {
            }

            protected internal override void Attach(UIElement element)
            {
            }

            protected internal override void Detach(UIElement element)
            {
            }
        }

        private sealed class LostFocusTrigger : BindingTrigger
        {
            public static LostFocusTrigger Singleton = new LostFocusTrigger();

            private LostFocusTrigger()
            {
            }

            protected internal override void Attach(UIElement element)
            {
                element.LostFocus += OnLostFocus; ;
            }

            protected internal override void Detach(UIElement element)
            {
                element.LostFocus -= OnLostFocus;
            }

            private void OnLostFocus(object sender, RoutedEventArgs e)
            {
                UpdateBinding((UIElement)sender);
            }
        }

        private sealed class DependencyPropertyChangedTrigger : BindingTrigger
        {
            public DependencyPropertyChangedTrigger(DependencyProperty property)
            {
                _property = property;
            }

            DependencyProperty _property;

            protected internal override void Attach(UIElement element)
            {
                var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
                dpd.AddValueChanged(element, OnPropertyChanged);
            }

            protected internal override void Detach(UIElement element)
            {
                var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
                dpd.RemoveValueChanged(element, OnPropertyChanged);
            }

            private void OnPropertyChanged(object sender, EventArgs e)
            {
                UpdateBinding((UIElement)sender);
            }
        }

        protected internal abstract void Attach(UIElement element);

        protected internal abstract void Detach(UIElement element);

        protected void UpdateBinding(UIElement element)
        {
            var rowItem = element.GetTemplateItem() as RowItem;
            if (rowItem != null)
                rowItem.UpdateBinding(element, this);
        }
    }
}
