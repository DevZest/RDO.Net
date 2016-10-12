using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowBindingBase<T> : RowBinding
        where T : UIElement, new()
    {
        List<T> _cachedElements;

        private T Create()
        {
            var result = new T();
            result.SetBinding(this);
            return result;
        }

        internal sealed override UIElement Setup(RowPresenter rowPresenter)
        {
            var element = CachedList.GetOrCreate(ref _cachedElements, Create);
            element.SetRowPresenter(rowPresenter);
            Setup(element, rowPresenter);
            Refresh(element, rowPresenter);
            return element;
        }

        protected abstract void Setup(T element, RowPresenter rowPresenter);

        protected abstract void Refresh(T element, RowPresenter rowPresenter);

        protected abstract void Cleanup(T element, RowPresenter rowPresenter);

        internal sealed override void Refresh(UIElement element)
        {
            var rowPresenter = element.GetRowPresenter();
            Refresh((T)element, rowPresenter);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            Cleanup(e, rowPresenter);
            e.SetRowPresenter(null);
            CachedList.Recycle(ref _cachedElements, e);
        }
    }
}
