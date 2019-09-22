using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents the behavior that can dynamically affect the look-and-feel of scalar binding target UI element.
    /// </summary>
    /// <typeparam name="T">Type of target UI element.</typeparam>
    public interface IScalarBindingBehavior<in T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Setup the scalar binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The scalar presenter.</param>
        void Setup(T view, ScalarPresenter presenter);

        /// <summary>
        /// Refresh the scalar binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The scalar presenter.</param>
        void Refresh(T view, ScalarPresenter presenter);

        /// <summary>
        /// Cleanup the scalar binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The scalar presenter.</param>
        void Cleanup(T view, ScalarPresenter presenter);
    }

    /// <summary>
    /// Base class of behavior to dynamically affect the look-and-feel of scalar binding target UI element.
    /// </summary>
    /// <typeparam name="T">Type of target UI element.</typeparam>
    public abstract class ScalarBindingBehavior<T> : IScalarBindingBehavior<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Setup the scalar binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The scalar presenter.</param>
        protected abstract void Setup(T view, ScalarPresenter presenter);

        /// <summary>
        /// Refresh the scalar binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The scalar presenter.</param>
        protected abstract void Refresh(T view, ScalarPresenter presenter);

        /// <summary>
        /// Cleanup the scalar binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The scalar presenter.</param>
        protected abstract void Cleanup(T view, ScalarPresenter presenter);

        void IScalarBindingBehavior<T>.Setup(T view, ScalarPresenter presenter)
        {
            Setup(view, presenter);
        }

        void IScalarBindingBehavior<T>.Refresh(T view, ScalarPresenter presenter)
        {
            Refresh(view, presenter);
        }

        void IScalarBindingBehavior<T>.Cleanup(T view, ScalarPresenter presenter)
        {
            Cleanup(view, presenter);
        }
    }
}
