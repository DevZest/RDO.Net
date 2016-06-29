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

        protected UIElement Mount(params Action<UIElement>[] initializers)
        {
            var element = CachedList.GetOrCreate(ref _cachedUIElements, Create);
            if (initializers != null)
            {
                foreach (var initializer in initializers)
                {
                    if (initializer != null)
                        initializer(element);
                }
            }
            OnMount(element);
            Refresh(element);
            SetBindings(element);
            return element;
        }

        protected virtual void SetBindings(UIElement element)
        {
        }

        protected abstract void OnMount(UIElement element);

        protected abstract void OnUnmount(UIElement element);

        internal void Unmount(UIElement element)
        {
            Debug.Assert(element != null && element.GetTemplateItem() == this);
            OnUnmount(element);
            Cleanup(element);
            CachedList.Recycle(ref _cachedUIElements, element);
        }

        protected abstract void Cleanup(UIElement element);

        internal abstract void Refresh(UIElement elment);

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
            if (GridRange.HorizontallyIntersectsWith(Template.FrozenLeft))
                throw new InvalidOperationException(Strings.TemplateItem_InvalidFrozenMargin(nameof(Template.FrozenLeft), templateItemsName, Ordinal));
            if (GridRange.VerticallyIntersectsWith(Template.FrozenTop))
                throw new InvalidOperationException(Strings.TemplateItem_InvalidFrozenMargin(nameof(Template.FrozenTop), templateItemsName, Ordinal));
            if (GridRange.HorizontallyIntersectsWith(Template.GridColumns.Count - Template.FrozenRight))
                throw new InvalidOperationException(Strings.TemplateItem_InvalidFrozenMargin(nameof(Template.FrozenRight), templateItemsName, Ordinal));
            if (GridRange.VerticallyIntersectsWith(Template.GridRows.Count - Template.FrozenBottom))
                throw new InvalidOperationException(Strings.TemplateItem_InvalidFrozenMargin(nameof(Template.FrozenBottom), templateItemsName, Ordinal));
        }
    }
}
