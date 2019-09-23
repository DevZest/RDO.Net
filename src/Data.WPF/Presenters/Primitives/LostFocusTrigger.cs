using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents a trigger that executes when view element lost focus.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public sealed class LostFocusTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new <see cref="LostFocusTrigger{T}"/> class.
        /// </summary>
        public LostFocusTrigger()
        {
        }

        /// <inheritdoc/>
        protected internal override void Attach(T element)
        {
            element.LostFocus += OnLostFocus;
        }

        /// <inheritdoc/>
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
