using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents the behavior that can dynamically affect the look-and-feel of block binding target UI element.
    /// </summary>
    /// <typeparam name="T">Type of target UI element.</typeparam>
    public interface IBlockBindingBehavior<in T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Setup the block binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The block presenter.</param>
        void Setup(T view, BlockPresenter presenter);

        /// <summary>
        /// Refresh the block binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The block presenter.</param>
        void Refresh(T view, BlockPresenter presenter);

        /// <summary>
        /// Cleanup the block binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The block presenter.</param>
        void Cleanup(T view, BlockPresenter presenter);
    }

    /// <summary>
    /// Base class of behavior to dynamically affect the look-and-feel of block binding target UI element.
    /// </summary>
    /// <typeparam name="T">Type of target UI element.</typeparam>
    public abstract class BlockBindingBehavior<T> : IBlockBindingBehavior<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Setup the block binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The block presenter.</param>
        protected abstract void Setup(T view, BlockPresenter presenter);

        /// <summary>
        /// Refresh the block binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The block presenter.</param>
        protected abstract void Refresh(T view, BlockPresenter presenter);

        /// <summary>
        /// Cleanup the block binding.
        /// </summary>
        /// <param name="view">The target UI element.</param>
        /// <param name="presenter">The block presenter.</param>
        protected abstract void Cleanup(T view, BlockPresenter presenter);

        void IBlockBindingBehavior<T>.Setup(T view, BlockPresenter presenter)
        {
            Setup(view, presenter);
        }

        void IBlockBindingBehavior<T>.Refresh(T view, BlockPresenter presenter)
        {
            Refresh(view, presenter);
        }

        void IBlockBindingBehavior<T>.Cleanup(T view, BlockPresenter presenter)
        {
            Cleanup(view, presenter);
        }
    }
}
