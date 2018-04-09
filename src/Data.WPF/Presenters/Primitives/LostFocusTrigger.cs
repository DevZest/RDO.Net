using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public sealed class LostFocusTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        public LostFocusTrigger()
        {
        }

        protected internal override void Attach(T element)
        {
            element.LostFocus += OnLostFocus;
        }

        protected internal override void Detach(T element)
        {
            element.LostFocus -= OnLostFocus;
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            Execute((T)sender);
        }
    }
}
