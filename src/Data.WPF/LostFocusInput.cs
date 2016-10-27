using DevZest.Data.Windows.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class LostFocusInput<T> : Input<T>
        where T : UIElement, new()
    {
        public LostFocusInput(Action<RowPresenter, T> action)
            : base(action)
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
            Flush((T)sender);
        }
    }
}
