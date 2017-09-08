using System;
using System.Windows;

namespace DevZest.Data.Presenters.Plugins
{
    public interface IBlockBindingPlugin
    {
        void Setup(UIElement view, BlockPresenter presenter);
        void Refresh(UIElement view, BlockPresenter presenter);
        void Cleanup(UIElement view, BlockPresenter presenter);
    }

    public abstract class BlockBindingPlugin<T> : IBlockBindingPlugin
        where T : UIElement, new()
    {
        protected abstract void Setup(T view, BlockPresenter presenter);
        protected abstract void Refresh(T view, BlockPresenter presenter);
        protected abstract void Cleanup(T view, BlockPresenter presenter);

        void IBlockBindingPlugin.Setup(UIElement view, BlockPresenter presenter)
        {
            Setup((T)view, presenter);
        }

        void IBlockBindingPlugin.Refresh(UIElement view, BlockPresenter presenter)
        {
            Refresh((T)view, presenter);
        }

        void IBlockBindingPlugin.Cleanup(UIElement view, BlockPresenter presenter)
        {
            Cleanup((T)view, presenter);
        }
    }
}
