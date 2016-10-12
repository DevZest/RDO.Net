using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public abstract partial class Binding
    {
        protected Binding()
        {
        }

        public Template Template { get; private set; }

        public GridRange GridRange { get; private set; }

        public int Ordinal { get; private set; }

        public bool IsSealed
        {
            get { return Template != null; }
        }

        internal void Seal(Template template, GridRange gridRange, int ordinal)
        {
            Debug.Assert(template != null && Template == null);

            Template = template;
            GridRange = gridRange;
            Ordinal = ordinal;
        }

        internal abstract void Refresh(UIElement element);

        internal abstract void Cleanup(UIElement element);

        internal void VerifyRowRange()
        {
            VerifyRowRange(Template.RowRange);
        }

        internal abstract void VerifyRowRange(GridRange rowRange);

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
