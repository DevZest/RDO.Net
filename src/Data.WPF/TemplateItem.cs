using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract partial class TemplateItem
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

        internal TemplateItem(Func<UIElement> constructor)
        {
            Debug.Assert(constructor != null);
            _constructor = constructor;
        }

        public Template Owner { get; private set; }

        public GridRange GridRange { get; private set; }

        public int Ordinal { get; private set; }

        internal void Construct(Template owner, GridRange gridRange, int ordinal)
        {
            Debug.Assert(owner != null && Owner == null);

            Owner = owner;
            GridRange = gridRange;
            Ordinal = ordinal;
        }

        Func<UIElement> _constructor;
        List<UIElement> _cachedUIElements;
        private UIElement GetOrCreate()
        {
            if (_cachedUIElements == null || _cachedUIElements.Count == 0)
                return Create();

            var last = _cachedUIElements.Count - 1;
            var result = _cachedUIElements[last];
            _cachedUIElements.RemoveAt(last);
            return result;
        }

        private UIElement Create()
        {
            var result = _constructor();
            result.SetTemplateItem(this);
            return result;
        }

        internal UIElement Generate()
        {
            return GetOrCreate();
        }

        private IList<Behavior> _behaviors = EmptyArray<Behavior>.Singleton;

        private void InitBehaviors<T>(IList<IBehavior<T>> behaviors)
            where T : UIElement, new()
        {
            Debug.Assert(_behaviors == EmptyArray<Behavior>.Singleton);

            if (behaviors == null || behaviors.Count == 0)
                return;

            _behaviors = new Behavior[behaviors.Count];
            for (int i = 0; i < _behaviors.Count; i++)
                _behaviors[i] = Behavior.Create(behaviors[i]);
        }

        private Action<UIElement> _initializer;
        private void InitInitializer<T>(Action<T> initializer)
            where T : UIElement
        {
            if (initializer == null)
                _initializer = null;
            else
                _initializer = x => initializer((T)x);
        }

        internal virtual void Initialize(UIElement element)
        {
            Debug.Assert(element != null && element.GetTemplateItem() == this);

            if (_initializer != null)
                _initializer(element);

            foreach (var binding in _bindings)
            {
                foreach (var trigger in binding.Triggers)
                    trigger.Attach(element);
            }

            foreach (var behavior in _behaviors)
                behavior.Attach(element);
        }

        internal void Recycle(UIElement element)
        {
            Debug.Assert(element != null && element.GetTemplateItem() == this);

            if (_cachedUIElements == null)
                _cachedUIElements = new List<UIElement>();
            _cachedUIElements.Add(element);
        }

        private Action<UIElement> _cleanup;
        private void InitCleanup<T>(Action<T> cleanup)
            where T : UIElement
        {
            if (cleanup == null)
                _cleanup = null;
            else
                _cleanup = x => cleanup((T)x);
        }

        internal virtual void Cleanup(UIElement element)
        {
            foreach (var binding in _bindings)
            {
                foreach (var trigger in binding.Triggers)
                    trigger.Detach(element);
            }

            foreach (var behavior in _behaviors)
                behavior.Detach(element);

            if (_cleanup != null)
                _cleanup(element);
        }

        private IList<Binding> _bindings = EmptyArray<Binding>.Singleton;

        private void AddBinding(Binding binding)
        {
            Debug.Assert(binding != null);
            if (_bindings == EmptyArray<Binding>.Singleton)
                _bindings = new List<Binding>();
            _bindings.Add(binding);
        }

        public void UpdateTarget(UIElement element)
        {
            VerifyElement(element, nameof(element));

            foreach (var binding in _bindings)
                binding.UpdateTarget(element);
        }

        public void UpdateSource(UIElement element)
        {
            VerifyElement(element, nameof(element));

            foreach (var binding in _bindings)
                binding.UpdateSource(element);
        }

        private void VerifyElement(UIElement element, string paramName)
        {
            if (element == null)
                throw new ArgumentNullException(paramName);

            if (element.GetTemplateItem() != this)
                throw new ArgumentException(Strings.TemplateItem_InvalidElement, paramName);
        }

        public int AutoSizeMeasureOrder { get; internal set; }
    }
}
