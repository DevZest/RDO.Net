using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public interface IScalarBindingBehavior<in T>
        where T : UIElement, new()
    {
        void Setup(T view, ScalarPresenter presenter);
        void Refresh(T view, ScalarPresenter presenter);
        void Cleanup(T view, ScalarPresenter presenter);
    }

    public abstract class ScalarBindingBehavior<T> : IScalarBindingBehavior<T>
        where T : UIElement, new()
    {
        protected abstract void Setup(T view, ScalarPresenter presenter);
        protected abstract void Refresh(T view, ScalarPresenter presenter);
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
