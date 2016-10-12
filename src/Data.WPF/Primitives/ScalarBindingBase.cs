using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ScalarBindingBase<T> : ScalarBinding
        where T : UIElement, new()
    {
        List<T> _cachedElements;

        private T Create()
        {
            var result = new T();
            result.SetBinding(this);
            return result;
        }

        internal sealed override UIElement Setup()
        {
            var element = CachedList.GetOrCreate(ref _cachedElements, Create);
            Setup(element);
            Refresh(element);
            return element;
        }

        protected abstract void Setup(T element);

        protected abstract void Refresh(T element);

        protected abstract void Cleanup(T element);

        internal sealed override void Refresh(UIElement element)
        {
            Refresh((T)element);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var e = (T)element;
            Cleanup(e);
            CachedList.Recycle(ref _cachedElements, e);
        }
    }
}
