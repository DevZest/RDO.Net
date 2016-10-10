using DevZest.Data.Windows.Primitives;
using System;
using System.ComponentModel;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class TriggerEvent
    {
        public static TriggerEvent Manual
        {
            get { return ManualTriggerEvent.Singleton; }
        }

        public static TriggerEvent LostFocus
        {
            get { return LostFocusTriggerEvent.Singleton; }
        }

        public static TriggerEvent PropertyChanged(DependencyProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            return new DependencyPropertyChangedTriggerEvent(property);
        }

        private sealed class ManualTriggerEvent : TriggerEvent
        {
            public static readonly ManualTriggerEvent Singleton = new ManualTriggerEvent();

            private ManualTriggerEvent()
            {
            }

            protected internal override void Attach(UIElement element)
            {
            }

            protected internal override void Detach(UIElement element)
            {
            }
        }

        private sealed class LostFocusTriggerEvent : TriggerEvent
        {
            public static LostFocusTriggerEvent Singleton = new LostFocusTriggerEvent();

            private LostFocusTriggerEvent()
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
                Execute((UIElement)sender);
            }
        }

        private sealed class DependencyPropertyChangedTriggerEvent : TriggerEvent
        {
            public DependencyPropertyChangedTriggerEvent(DependencyProperty property)
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
                Execute((UIElement)sender);
            }
        }

        protected internal abstract void Attach(UIElement element);

        protected internal abstract void Detach(UIElement element);

        protected void Execute(UIElement element)
        {
            var rowItem = element.GetTemplateItem() as RowItem;
            if (rowItem != null)
                rowItem.ExecuteTrigger(element, this);
        }
    }
}
