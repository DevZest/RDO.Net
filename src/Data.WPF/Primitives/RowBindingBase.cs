using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowBindingBase<T> : RowBinding
        where T : UIElement, new()
    {
        private struct Trigger
        {
            public Trigger(TriggerEvent triggerEvent, Action<RowPresenter, T> action)
            {
                Event = triggerEvent;
                Action = action;
            }

            public readonly TriggerEvent Event;
            public readonly Action<RowPresenter, T> Action;

            public void Execute(T element, TriggerEvent triggerEvent)
            {
                if (triggerEvent == Event)
                    Action(element.GetRowPresenter(), element);
            }
        }

        private IList<Trigger> _triggers = Array<Trigger>.Empty;
        public void AddTrigger(TriggerEvent trigger, Action<RowPresenter, T> action)
        {
            VerifyNotSealed();
            if (_triggers == Array<Trigger>.Empty)
                _triggers = new List<Trigger>();

            _triggers.Add(new Trigger(trigger, action));
        }

        internal sealed override void ExecuteTrigger(UIElement element, TriggerEvent triggerEvent)
        {
            foreach (var trigger in _triggers)
                trigger.Execute((T)element, triggerEvent);
        }

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
            foreach (var trigger in _triggers)
                trigger.Event.Attach(element);
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
                trigger.Event.Detach(e);
            Cleanup(e, rowPresenter);
            e.SetRowPresenter(null);
            CachedList.Recycle(ref _cachedElements, e);
        }
    }
}
