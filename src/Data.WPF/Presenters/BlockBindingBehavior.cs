using System.Windows;

namespace DevZest.Data.Presenters
{
    public interface IBlockBindingBehavior<in T>
        where T : UIElement, new()
    {
        void Setup(T view, BlockPresenter presenter);
        void Refresh(T view, BlockPresenter presenter);
        void Cleanup(T view, BlockPresenter presenter);
    }

    public abstract class BlockBindingBehavior<T> : IBlockBindingBehavior<T>
        where T : UIElement, new()
    {
        protected abstract void Setup(T view, BlockPresenter presenter);
        protected abstract void Refresh(T view, BlockPresenter presenter);
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
