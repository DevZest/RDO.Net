using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents the behavior that can dynamically affect the look-and-feel of row binding target UI element.
    /// </summary>
    /// <typeparam name="T">Type of target UI element.</typeparam>
    public interface IRowBindingBehavior<in T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Setup the row binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The row presenter.</param>
        void Setup(T view, RowPresenter presenter);

        /// <summary>
        /// Refresh the row binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The row presenter.</param>
        void Refresh(T view, RowPresenter presenter);

        /// <summary>
        /// Cleanup the row binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The row presenter.</param>
        void Cleanup(T view, RowPresenter presenter);
    }

    /// <summary>
    /// Base class of behavior to dynamically affect the look-and-feel of scalar binding target UI element.
    /// </summary>
    /// <typeparam name="T">Type of target UI element.</typeparam>
    public abstract class RowBindingBehavior<T> : IRowBindingBehavior<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Setup the row binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The row presenter.</param>
        protected abstract void Setup(T view, RowPresenter presenter);

        /// <summary>
        /// Refresh the row binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The row presenter.</param>
        protected abstract void Refresh(T view, RowPresenter presenter);

        /// <summary>
        /// Cleanup the row binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The row presenter.</param>
        protected abstract void Cleanup(T view, RowPresenter presenter);

        void IRowBindingBehavior<T>.Setup(T view, RowPresenter presenter)
        {
            Setup(view, presenter);
        }

        void IRowBindingBehavior<T>.Refresh(T view, RowPresenter presenter)
        {
            Refresh((T)view, presenter);
        }

        void IRowBindingBehavior<T>.Cleanup(T view, RowPresenter presenter)
        {
            Cleanup((T)view, presenter);
        }
    }
}
