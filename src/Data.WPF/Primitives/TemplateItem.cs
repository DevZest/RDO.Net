using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
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

            private Behavior(Action<UIElement> attachAction, Action<UIElement> detachAction)
            {
                Debug.Assert(attachAction != null);
                Debug.Assert(detachAction != null);
                _attachAction = attachAction;
                _detachAction = detachAction;
            }

            Action<UIElement> _attachAction;
            public void Attach(UIElement element)
            {
                _attachAction(element);
            }

            Action<UIElement> _detachAction;
            public void Detach(UIElement element)
            {
                _detachAction(element);
            }
        }

        internal TemplateItem(Func<UIElement> constructor)
        {
            Debug.Assert(constructor != null);
            _constructor = constructor;
        }

        public Template Template { get; private set; }

        public GridRange GridRange { get; private set; }

        public int Ordinal { get; private set; }

        internal void Construct(Template template, GridRange gridRange, int ordinal)
        {
            Debug.Assert(template != null && Template == null);

            Template = template;
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

        private IList<BindingBase> _bindings = EmptyArray<BindingBase>.Singleton;

        internal void AddBinding(BindingBase binding)
        {
            Debug.Assert(binding != null);
            if (_bindings == EmptyArray<BindingBase>.Singleton)
                _bindings = new List<BindingBase>();
            _bindings.Add(binding);
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

            UpdateTarget(element);
        }

        private void Recycle(UIElement element)
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

            Recycle(element);
        }

        internal void UpdateTarget(UIElement element)
        {
            var bindingContext = BindingContext.Current;
            bindingContext.Enter(this, element);
            try
            {
                foreach (var binding in _bindings)
                    binding.UpdateTarget(bindingContext, element);
            }
            finally
            {
                bindingContext.Exit();
            }
        }

        internal void UpdateSource(UIElement element)
        {
            var bindingContext = BindingContext.Current;
            bindingContext.Enter(this, element);
            try
            {
                foreach (var binding in _bindings)
                    binding.UpdateSource(bindingContext, element);
            }
            finally
            {
                bindingContext.Exit();
            }
        }

        public int AutoSizeMeasureOrder { get; internal set; }
    }
}
