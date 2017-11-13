using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public interface IRowBindingBehavior<in T>
        where T : UIElement, new()
    {
        void Setup(T view, RowPresenter presenter);
        void Refresh(T view, RowPresenter presenter);
        void Cleanup(T view, RowPresenter presenter);
    }

    public abstract class RowBindingBehavior<T> : IRowBindingBehavior<T>
        where T : UIElement, new()
    {
        protected abstract void Setup(T view, RowPresenter presenter);
        protected abstract void Refresh(T view, RowPresenter presenter);
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
