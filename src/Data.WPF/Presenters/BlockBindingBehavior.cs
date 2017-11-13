using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public interface IBlockBindingBehavior
    {
        void Setup(UIElement view, BlockPresenter presenter);
        void Refresh(UIElement view, BlockPresenter presenter);
        void Cleanup(UIElement view, BlockPresenter presenter);
    }

    public abstract class BlockBindingBehavior<T> : IBlockBindingBehavior
        where T : UIElement, new()
    {
        protected abstract void Setup(T view, BlockPresenter presenter);
        protected abstract void Refresh(T view, BlockPresenter presenter);
        protected abstract void Cleanup(T view, BlockPresenter presenter);

        void IBlockBindingBehavior.Setup(UIElement view, BlockPresenter presenter)
        {
            Setup((T)view, presenter);
        }

        void IBlockBindingBehavior.Refresh(UIElement view, BlockPresenter presenter)
        {
            Refresh((T)view, presenter);
        }

        void IBlockBindingBehavior.Cleanup(UIElement view, BlockPresenter presenter)
        {
            Cleanup((T)view, presenter);
        }
    }
}
