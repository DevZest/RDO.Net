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

        private UIElement Create()
        {
            var result = _constructor();
            result.SetTemplateItem(this);
            return result;
        }

        internal UIElement Generate()
        {
            return CachedList.GetOrCreate(ref _cachedUIElements, Create);
        }

        private IList<BindingBase> _bindings = Array<BindingBase>.Empty;

        internal void AddBinding(BindingBase binding)
        {
            Debug.Assert(binding != null);
            if (_bindings == Array<BindingBase>.Empty)
                _bindings = new List<BindingBase>();
            _bindings.Add(binding);
        }

        private IList<Behavior> _behaviors = Array<Behavior>.Empty;

        private void InitBehaviors<T>(IList<IBehavior<T>> behaviors)
            where T : UIElement, new()
        {
            Debug.Assert(_behaviors == Array<Behavior>.Empty);

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
            CachedList.Recycle(ref _cachedUIElements, element);
        }

        private Action<UIElement> _cleanupAction;
        private void InitCleanupAction<T>(Action<T> cleanupAction)
            where T : UIElement
        {
            if (cleanupAction == null)
                _cleanupAction = null;
            else
                _cleanupAction = x => cleanupAction((T)x);
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

            if (_cleanupAction != null)
                _cleanupAction(element);

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

        internal void VerifyGridRange()
        {
            VerifyGridRange(Template.RowRange);
        }

        internal abstract void VerifyGridRange(GridRange rowRange);

        public int AutoSizeMeasureOrder { get; internal set; }

        private bool SizeToContentX
        {
            get { return Template.SizeToContentX; }
        }

        private bool SizeToContentY
        {
            get { return Template.SizeToContentY; }
        }

        private IConcatList<GridColumn> _autoWidthGridColumns;
        internal void InvalidateAutoWidthGridColumns()
        {
            _autoWidthGridColumns = null;
        }

        internal IConcatList<GridColumn> AutoWidthGridColumns
        {
            get
            {
                if (AutoSizeMeasureOrder < 0)
                    return ConcatList<GridColumn>.Empty;

                if (_autoWidthGridColumns == null)
                    _autoWidthGridColumns = GridRange.FilterColumns(x => x.IsAutoLength(SizeToContentX));
                return _autoWidthGridColumns;
            }
        }

        private IConcatList<GridRow> _autoHeightGridRows;
        internal void InvalidateAutoHeightGridRows()
        {
            _autoHeightGridRows = null;
        }

        internal IConcatList<GridRow> AutoHeightGridRows
        {
            get
            {
                if (AutoSizeMeasureOrder < 0)
                    return ConcatList<GridRow>.Empty;

                if (_autoHeightGridRows == null)
                    _autoHeightGridRows = GridRange.FilterRows(x => x.IsAutoLength(SizeToContentY));
                return _autoHeightGridRows;
            }
        }

        internal bool IsAutoSize
        {
            get { return AutoWidthGridColumns.Count > 0 || AutoHeightGridRows.Count > 0; }
        }
    }
}
