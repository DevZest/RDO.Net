using System;
using System.Windows;

namespace DevZest.Data.Presenters.Plugins
{
    public interface IScalarBindingPlugin
    {
        void Setup(UIElement view, ScalarPresenter presenter);
        void Refresh(UIElement view, ScalarPresenter presenter);
        void Cleanup(UIElement view, ScalarPresenter presenter);
    }

    public abstract class ScalarBindingPlugin<T> : IScalarBindingPlugin
        where T : UIElement, new()
    {
        protected abstract void Setup(T view, ScalarPresenter presenter);
        protected abstract void Refresh(T view, ScalarPresenter presenter);
        protected abstract void Cleanup(T view, ScalarPresenter presenter);

        void IScalarBindingPlugin.Setup(UIElement view, ScalarPresenter presenter)
        {
            Setup((T)view, presenter);
        }

        void IScalarBindingPlugin.Refresh(UIElement view, ScalarPresenter presenter)
        {
            Refresh((T)view, presenter);
        }

        void IScalarBindingPlugin.Cleanup(UIElement view, ScalarPresenter presenter)
        {
            Cleanup((T)view, presenter);
        }
    }
}
