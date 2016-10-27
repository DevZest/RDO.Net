using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowBindingBase<T> : RowBinding
        where T : UIElement, new()
    {
        private Input<T> _input;
        public Input<T> Input
        {
            get { return _input; }
            set
            {
                VerifyNotSealed();
                if (value.RowBinding != null)
                    throw new ArgumentException(Strings.RowBindingBase_InputAlreadyAssigned, nameof(value));
                value.RowBinding = this;
                _input = value;
            }
        }

        internal sealed override void FlushInput(UIElement element)
        {
            if (Input != null)
                Input.Flush((T)element);
        }

        List<T> _cachedElements;

        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        internal sealed override UIElement Setup(RowPresenter rowPresenter)
        {
            var element = CachedList.GetOrCreate(ref _cachedElements, Create);
            element.SetRowPresenter(rowPresenter);
            Setup(element, rowPresenter);
            Refresh(element, rowPresenter);
            if (Input != null)
                Input.Attach(element);
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
            if (Input != null)
                Input.Detach(e);
            Cleanup(e, rowPresenter);
            e.SetRowPresenter(null);
            CachedList.Recycle(ref _cachedElements, e);
        }
    }
}
