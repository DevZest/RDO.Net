using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class GridEntry
    {
        private sealed class Behavior
        {
            internal static Behavior Create<T>(IBehavior<T> behavior)
                where T : UIElement, new()
            {
                return behavior == null ? null : new Behavior(x => behavior.Attach((T)x), x => behavior.Detach((T)x));
            }

            private Behavior(Action<UIElement> attach, Action<UIElement> detach)
            {
                Debug.Assert(attach != null);
                Debug.Assert(detach != null);
                _attach = attach;
                _detach = detach;
            }

            Action<UIElement> _attach;
            public void Attach(UIElement element)
            {
                _attach(element);
            }

            Action<UIElement> _detach;
            public void Detach(UIElement element)
            {
                _detach(element);
            }
        }

        internal GridEntry(Func<UIElement> constructor)
        {
            Debug.Assert(constructor != null);

        }

        public GridTemplate Owner { get; private set; }

        public GridRange GridRange { get; private set; }

        public int Ordinal { get; private set; }

        internal void Seal(GridTemplate owner, GridRange gridRange, int ordinal)
        {
            Debug.Assert(owner != null);

            if (Owner != null)
                throw new InvalidOperationException(Strings.GridEntry_OwnerAlreadySet);
            Owner = owner;
            GridRange = gridRange;
            Ordinal = ordinal;
        }

        Func<UIElement> _constructor;
        List<UIElement> _cachedUIElements;
        private UIElement GetOrCreate()
        {
            if (_cachedUIElements == null || _cachedUIElements.Count == 0)
                return _constructor();

            var last = _cachedUIElements.Count - 1;
            var result = _cachedUIElements[last];
            _cachedUIElements.RemoveAt(last);
            return result;
        }

        internal UIElement Generate()
        {
            return GetOrCreate();
        }

        private static Behavior[] s_emptyBehaviors = new Behavior[0];
        private IList<Behavior> _behaviors = s_emptyBehaviors;

        internal void InitBehaviors<T>(IList<IBehavior<T>> behaviors)
            where T : UIElement, new()
        {
            Debug.Assert(_behaviors == s_emptyBehaviors);

            if (behaviors == null || behaviors.Count == 0)
                return;

            _behaviors = new Behavior[behaviors.Count];
            for (int i = 0; i < _behaviors.Count; i++)
                _behaviors[i] = Behavior.Create(behaviors[i]);
        }

        private Action<UIElement> _initializer;
        internal void InitInitializer<T>(Action<T> initializer)
            where T : UIElement
        {
            if (initializer == null)
                _initializer = null;
            else
                _initializer = x => initializer((T)x);
        }

        internal virtual void OnInitialize(UIElement element)
        {
            if (_initializer != null)
                _initializer(element);

            foreach (var behavior in _behaviors)
            {
                if (behavior != null)
                    behavior.Attach(element);
            }
        }

        internal void Recycle(UIElement element)
        {
            Debug.Assert(element != null);

            OnCleanup(element);
            if (_cachedUIElements == null)
                _cachedUIElements = new List<UIElement>();
            _cachedUIElements.Add(element);
        }

        private Action<UIElement> _cleanup;
        internal void InitCleanup<T>(Action<T> cleanup)
            where T : UIElement
        {
            if (cleanup == null)
                _cleanup = null;
            else
                _cleanup = x => cleanup((T)x);
        }

        internal virtual void OnCleanup(UIElement element)
        {
            if (_cleanup != null)
                _cleanup(element);

            foreach (var behavior in _behaviors)
            {
                if (behavior != null)
                    behavior.Detach(element);
            }
        }
    }
}
