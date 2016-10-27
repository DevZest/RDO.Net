using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ScalarBindingBase<T> : ScalarBinding
        where T : UIElement, new()
    {
        private IList<Trigger<T>> _triggers = Array<Trigger<T>>.Empty;
        public void AddTrigger(TriggerEvent<T> triggerEvent, Action<T> triggerAction)
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

        internal sealed override UIElement Setup()
        {
            var element = CachedList.GetOrCreate(ref _cachedElements, Create);
            Setup(element);
            Refresh(element);
            foreach (var trigger in _triggers)
                trigger.Attach(element);
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
            foreach (var trigger in _triggers)
                trigger.Detach(e);
            Cleanup(e);
            CachedList.Recycle(ref _cachedElements, e);
        }
    }
}
