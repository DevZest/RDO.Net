using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents a trigger that executes when routed event occurs.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public sealed class RoutedEventTrigger<T> : Trigger<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RoutedEventTrigger{T}"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event.</param>
        public RoutedEventTrigger(RoutedEvent routedEvent)
        {
            routedEvent.VerifyNotNull(nameof(routedEvent));
            _routedEvent = routedEvent;
        }

        private readonly RoutedEvent _routedEvent;

        /// <inheritdoc/>
        protected internal override void Attach(T element)
        {
            element.AddHandler(_routedEvent, new RoutedEventHandler(OnExecute));
        }

        /// <inheritdoc/>
        protected internal override void Detach(T element)
        {
            element.RemoveHandler(_routedEvent, new RoutedEventHandler(OnExecute));
        }

        private void OnExecute(object sender, RoutedEventArgs e)
        {
            Execute((T)sender);
        }
    }
}
