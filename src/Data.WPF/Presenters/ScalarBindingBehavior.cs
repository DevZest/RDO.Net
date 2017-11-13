using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public interface IScalarBindingBehavior
    {
        void Setup(UIElement view, ScalarPresenter presenter);
        void Refresh(UIElement view, ScalarPresenter presenter);
        void Cleanup(UIElement view, ScalarPresenter presenter);
    }

    public abstract class ScalarBindingBehavior<T> : IScalarBindingBehavior
        where T : UIElement, new()
    {
        protected abstract void Setup(T view, ScalarPresenter presenter);
        protected abstract void Refresh(T view, ScalarPresenter presenter);
        protected abstract void Cleanup(T view, ScalarPresenter presenter);

        void IScalarBindingBehavior.Setup(UIElement view, ScalarPresenter presenter)
        {
            Setup((T)view, presenter);
        }

        void IScalarBindingBehavior.Refresh(UIElement view, ScalarPresenter presenter)
        {
            Refresh((T)view, presenter);
        }

        void IScalarBindingBehavior.Cleanup(UIElement view, ScalarPresenter presenter)
        {
            Cleanup((T)view, presenter);
        }
    }
}
