using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public interface IRowBindingBehavior
    {
        void Setup(UIElement view, RowPresenter presenter);
        void Refresh(UIElement view, RowPresenter presenter);
        void Cleanup(UIElement view, RowPresenter presenter);
    }

    public abstract class RowBindingBehavior<T> : IRowBindingBehavior
        where T : UIElement, new()
    {
        protected abstract void Setup(T view, RowPresenter presenter);
        protected abstract void Refresh(T view, RowPresenter presenter);
        protected abstract void Cleanup(T view, RowPresenter presenter);

        void IRowBindingBehavior.Setup(UIElement view, RowPresenter presenter)
        {
            Setup((T)view, presenter);
        }

        void IRowBindingBehavior.Refresh(UIElement view, RowPresenter presenter)
        {
            Refresh((T)view, presenter);
        }

        void IRowBindingBehavior.Cleanup(UIElement view, RowPresenter presenter)
        {
            Cleanup((T)view, presenter);
        }
    }
}
