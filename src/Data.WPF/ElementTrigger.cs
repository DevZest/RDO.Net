using System;
using System.ComponentModel;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ElementTrigger : IBehavior<UIElement>
    {
        public static ElementTrigger LostFocus
        {
            get { return LostFocusTrigger.Singleton; }
        }

        public static ElementTrigger PropertyChanged(DependencyProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            return new DependencyPropertyChangedTrigger(property);
        }

        private sealed class LostFocusTrigger : ElementTrigger
        {
            public static LostFocusTrigger Singleton = new LostFocusTrigger();

            private LostFocusTrigger()
            {
            }

            public override void Attach(UIElement element)
            {
                element.LostFocus += OnLostFocus; ;
            }

            public override void Detach(UIElement element)
            {
                element.LostFocus -= OnLostFocus;
            }
            private void OnLostFocus(object sender, RoutedEventArgs e)
            {
                UpdateSource((UIElement)sender);
            }
        }

        private sealed class DependencyPropertyChangedTrigger : ElementTrigger
        {
            public DependencyPropertyChangedTrigger(DependencyProperty property)
            {
                _property = property;
            }

            DependencyProperty _property;

            public override void Attach(UIElement element)
            {
                var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
                dpd.AddValueChanged(element, OnPropertyChanged);
            }

            public override void Detach(UIElement element)
            {
                var dpd = DependencyPropertyDescriptor.FromProperty(_property, element.GetType());
                dpd.RemoveValueChanged(element, OnPropertyChanged);
            }

            private void OnPropertyChanged(object sender, EventArgs e)
            {
                UpdateSource((UIElement)sender);
            }
        }

        public abstract void Attach(UIElement element);

        public abstract void Detach(UIElement element);

        protected void UpdateSource(UIElement element)
        {
            var gridEntry = element.GetGridEntry();
            if (gridEntry == null)
                gridEntry.UpdateSource(element);
        }
    }
}
