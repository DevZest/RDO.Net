using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class BlockBindingBase<T> : BlockBinding
        where T : UIElement, new()
    {
        private IList<Trigger<T>> _triggers = Array<Trigger<T>>.Empty;
        public void AddTrigger(TriggerEvent<T> triggerEvent, Action<T, int, IReadOnlyList<RowPresenter>> triggerAction)
        {
            if (triggerAction == null)
                throw new ArgumentNullException(nameof(triggerAction));
            VerifyNotSealed();

            if (_triggers == Array<Trigger<T>>.Empty)
                _triggers = new List<Trigger<T>>();
            _triggers.Add(new Trigger<T>(triggerEvent, triggerAction));
        }

        List<T> _cachedElements;

        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        internal sealed override UIElement Setup(BlockView blockView)
        {
            var element = CachedList.GetOrCreate(ref _cachedElements, Create);
            element.SetBlockView(blockView);
            Setup(element, blockView.Ordinal, blockView);
            Refresh(element, blockView.Ordinal, blockView);
            foreach (var trigger in _triggers)
                trigger.Attach(element);
            return element;
        }

        protected abstract void Setup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows);

        protected abstract void Refresh(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows);

        protected abstract void Cleanup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows);

        internal sealed override void Refresh(UIElement element)
        {
            var blockView = element.GetBlockView();
            Refresh((T)element, blockView.Ordinal, blockView);
        }

        internal override void Cleanup(UIElement element)
        {
            var blockView = element.GetBlockView();
            var e = (T)element;
            foreach (var trigger in _triggers)
                trigger.Detach(e);
            Cleanup(e, blockView.Ordinal, blockView);
            e.SetBlockView(null);
            CachedList.Recycle(ref _cachedElements, e);
        }
    }
}
