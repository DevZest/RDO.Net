using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public abstract partial class TemplateItem
    {
        public abstract class Builder<TElement, TItem, TBuilder> : IDisposable
            where TElement : UIElement, new()
            where TItem : TemplateItem
            where TBuilder : Builder<TElement, TItem, TBuilder>
        {
            internal Builder(TemplateBuilder templateBuilder, TItem item)
            {
                _templateBuilder = templateBuilder;
                _templateItem = item;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                _templateItem = null;
            }

            private TemplateBuilder _templateBuilder;
            private Template Template
            {
                get { return _templateBuilder.Template; }
            }

            public TemplateBuilder At(int column, int row)
            {
                return At(column, row, column, row);
            }

            public TemplateBuilder At(int left, int top, int right, int bottom)
            {
                AddItem(Template, Template.Range(left, top, right, bottom), TemplateItem);
                return _templateBuilder;
            }

            internal abstract void AddItem(Template template, GridRange gridRange, TItem item);

            internal abstract TBuilder This { get; }

            private TItem _templateItem;
            internal TItem TemplateItem
            {
                get
                {
                    if (_templateItem == null)
                        throw new ObjectDisposedException(GetType().FullName);

                    return _templateItem;
                }
            }

            public TBuilder Initialize(Action<TElement> initializer)
            {
                TemplateItem.InitInitializer(initializer);
                return This;
            }

            public TBuilder Cleanup(Action<TElement> cleanupAction)
            {
                TemplateItem.InitCleanupAction(cleanupAction);
                return This;
            }

            public TBuilder Behaviors(params IBehavior<TElement>[] behaviors)
            {
                TemplateItem.InitBehaviors(behaviors);
                return This;
            }

            public TBuilder AutoSize(int autoSizeOrder)
            {
                TemplateItem.AutoSizeOrder = autoSizeOrder;
                return This;
            }

            public TBuilder AutoSize(AutoSizeWaiver autoSizeWaiver)
            {
                TemplateItem.AutoSizeWaiver = autoSizeWaiver;
                return This;
            }

            public TBuilder AutoSize(int autoSizeOrder, AutoSizeWaiver autoSizeWaiver)
            {
                TemplateItem.AutoSizeOrder = autoSizeOrder;
                TemplateItem.AutoSizeWaiver = autoSizeWaiver;
                return This;
            }
        }

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

        internal void VerifyRowRange()
        {
            VerifyRowRange(Template.RowRange);
        }

        internal abstract void VerifyRowRange(GridRange rowRange);

        private bool SizeToContentX
        {
            get { return Template.SizeToContentX; }
        }

        private bool SizeToContentY
        {
            get { return Template.SizeToContentY; }
        }

        public int AutoSizeOrder { get; internal set; }

        public AutoSizeWaiver AutoSizeWaiver { get; private set; }

        internal virtual AutoSizeWaiver CoercedAutoSizeWaiver
        {
            get { return AutoSizeWaiver; }
        }

        private IConcatList<GridColumn> _autoWidthGridColumns;
        internal void InvalidateAutoWidthGridColumns()
        {
            _autoWidthGridColumns = null;
        }

        private bool IsAutoWidthWaived
        {
            get { return (CoercedAutoSizeWaiver & AutoSizeWaiver.Width) == AutoSizeWaiver.Width; }
        }

        internal IConcatList<GridColumn> AutoWidthGridColumns
        {
            get
            {
                if (IsAutoWidthWaived)
                    return ConcatList<GridColumn>.Empty;

                if (_autoWidthGridColumns == null)
                    _autoWidthGridColumns = GridRange.FilterColumns(x => x.IsAutoLength);
                return _autoWidthGridColumns;
            }
        }

        private IConcatList<GridRow> _autoHeightGridRows;
        internal void InvalidateAutoHeightGridRows()
        {
            _autoHeightGridRows = null;
        }

        private bool IsAutoHeigthWaived
        {
            get { return (CoercedAutoSizeWaiver & AutoSizeWaiver.Height) == AutoSizeWaiver.Height; }
        }

        internal IConcatList<GridRow> AutoHeightGridRows
        {
            get
            {
                if (IsAutoHeigthWaived)
                    return ConcatList<GridRow>.Empty;

                if (_autoHeightGridRows == null)
                    _autoHeightGridRows = GridRange.FilterRows(x => x.IsAutoLength);
                return _autoHeightGridRows;
            }
        }

        internal bool IsAutoSize
        {
            get { return AutoWidthGridColumns.Count > 0 || AutoHeightGridRows.Count > 0; }
        }

        internal Size AvailableAutoSize
        {
            get
            {
                Debug.Assert(IsAutoSize);
                var width = AutoWidthGridColumns.Count > 0 || IsAutoWidthWaived ? double.PositiveInfinity : GridRange.MeasuredWidth;
                var height = AutoHeightGridRows.Count > 0 || IsAutoHeigthWaived ? double.PositiveInfinity : GridRange.MeasuredHeight;
                return new Size(width, height);
            }
        }

        internal virtual void VerifyFrozenMargins(string templateItemsName)
        {
            if (GridRange.ContainsHorizontal(Template.FrozenLeft - 1))
                throw new InvalidOperationException(Strings.TemplateItem_InvalidFrozenMargin(nameof(Template.FrozenLeft), templateItemsName, Ordinal));
            if (GridRange.ContainsVertical(Template.FrozenTop - 1))
                throw new InvalidOperationException(Strings.TemplateItem_InvalidFrozenMargin(nameof(Template.FrozenTop), templateItemsName, Ordinal));
            if (GridRange.ContainsHorizontal(Template.GridColumns.Count - Template.FrozenRight))
                throw new InvalidOperationException(Strings.TemplateItem_InvalidFrozenMargin(nameof(Template.FrozenRight), templateItemsName, Ordinal));
            if (GridRange.ContainsVertical(Template.GridRows.Count - Template.FrozenBottom))
                throw new InvalidOperationException(Strings.TemplateItem_InvalidFrozenMargin(nameof(Template.FrozenBottom), templateItemsName, Ordinal));
        }
    }
}
