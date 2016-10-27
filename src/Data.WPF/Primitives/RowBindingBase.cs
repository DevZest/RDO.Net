using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowBindingBase<T> : RowBinding
        where T : UIElement, new()
    {
        private IList<Trigger<T>> _triggers = Array<Trigger<T>>.Empty;
        public void AddTrigger(TriggerEvent<T> triggerEvent, Action<T, RowPresenter> triggerAction)
        {
            if (triggerAction == null)
                throw new ArgumentNullException(nameof(triggerAction));
            VerifyNotSealed();

            if (_triggers == Array<Trigger<T>>.Empty)
                _triggers = new List<Trigger<T>>();
            _triggers.Add(new Trigger<T>(this, triggerEvent, triggerAction));
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
            foreach (var trigger in _triggers)
                trigger.Attach(element);
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
            foreach (var trigger in _triggers)
                trigger.Detach(e);
            Cleanup(e, rowPresenter);
            e.SetRowPresenter(null);
            CachedList.Recycle(ref _cachedElements, e);
        }
    }
}
