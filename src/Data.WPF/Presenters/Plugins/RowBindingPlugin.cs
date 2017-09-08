using System;
using System.Windows;

namespace DevZest.Data.Presenters.Plugins
{
    public interface IRowBindingPlugin
    {
        void Setup(UIElement view, RowPresenter presenter);
        void Refresh(UIElement view, RowPresenter presenter);
        void Cleanup(UIElement view, RowPresenter presenter);
    }

    public abstract class RowBindingPlugin<T> : IRowBindingPlugin
        where T : UIElement, new()
    {
        protected abstract void Setup(T view, RowPresenter presenter);
        protected abstract void Refresh(T view, RowPresenter presenter);
        protected abstract void Cleanup(T view, RowPresenter presenter);

        void IRowBindingPlugin.Setup(UIElement view, RowPresenter presenter)
        {
            Setup((T)view, presenter);
        }

        void IRowBindingPlugin.Refresh(UIElement view, RowPresenter presenter)
        {
            Refresh((T)view, presenter);
        }

        void IRowBindingPlugin.Cleanup(UIElement view, RowPresenter presenter)
        {
            Cleanup((T)view, presenter);
        }
    }
}
