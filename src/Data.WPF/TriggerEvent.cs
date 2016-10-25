using DevZest.Data.Windows.Primitives;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class TriggerEvent
    {
    }

    public abstract class TriggerEvent<T> : TriggerEvent
        where T : UIElement, new()
    {
        public static TriggerEvent<T> LostFocus
        {
            get { return new LostFocusEvent(); }
        }

        public static TriggerEvent<T> PropertyChanged(DependencyProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            return new DependencyPropertyChangedEvent(property);
        }

        private sealed class LostFocusEvent : TriggerEvent<T>
        {
            public LostFocusEvent()
            {
            }

            protected internal override void Attach(T element)
            {
                element.LostFocus += OnLostFocus; ;
            }

            protected internal override void Detach(T element)
            {
                element.LostFocus -= OnLostFocus;
            }

            private void OnLostFocus(object sender, RoutedEventArgs e)
            {
                ExecuteTriggerAction((T)sender);
            }
        }

        private sealed class DependencyPropertyChangedEvent : TriggerEvent<T>
        {
            public DependencyPropertyChangedEvent(DependencyProperty property)
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
                ExecuteTriggerAction((T)sender);
            }
        }

        private Trigger<T> _trigger;
        internal Trigger<T> Trigger
        {
            set
            {
                Debug.Assert(value != null);
                if (_trigger != null)
                    throw new InvalidOperationException(Strings.Event_AlreadyInitializedWithTrigger);
                _trigger = value;
            }
        }

        protected void ExecuteTriggerAction(T element)
        {
            Debug.Assert(_trigger != null);
            _trigger.ExecuteAction(element);
        }

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);
    }
}
