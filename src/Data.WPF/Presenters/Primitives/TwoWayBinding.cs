using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents two way data binding.
    /// </summary>
    public abstract class TwoWayBinding : Binding
    {
        /// <summary>
        /// Gets a value indicates whether this binding is refreshing.
        /// </summary>
        public abstract bool IsRefreshing { get; }

        internal abstract void FlushInput(UIElement element);
    }
}
