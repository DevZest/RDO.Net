﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract partial class Binding
    {
        protected Binding()
        {
        }

        private Template _template;
        public virtual Template Template
        {
            get { return ParentBinding != null ? ParentBinding.Template : _template; }
            private set { _template = value; }
        }

        private GridRange _gridRange;
        public GridRange GridRange
        {
            get { return ParentBinding != null ? ParentBinding.GridRange : _gridRange; }
            private set { _gridRange = value; }
        }

        public int Ordinal { get; internal set; } = -1;

        public abstract Binding ParentBinding { get; }

        public bool IsSealed
        {
            get { return (ParentBinding != null && ParentBinding.IsSealed) || (Template != null && Template.IsSealed); }
        }

        /// <summary>Throws an <see cref="InvalidOperationException"/> if this <see cref="Binding"/> is sealed.</summary>
        /// <remarks>
        /// <see cref="Binding"/> objects will be sealed when added to template. Drived class should call this method before making modification to this object.
        /// </remarks>
        internal void VerifyNotSealed()
        {
            if (IsSealed)
                throw new InvalidOperationException(DiagnosticMessages.Binding_VerifyNotSealed);
        }

        internal static void VerifyAdding(Binding binding, string paramName)
        {
            if (binding == null)
                throw new ArgumentNullException(paramName);

            if (binding.Template != null || binding.ParentBinding != null)
                throw new ArgumentException(DiagnosticMessages.Binding_VerifyAdding, paramName);
        }

        internal void Seal(Template template, GridRange gridRange, int ordinal)
        {
            Debug.Assert(template != null && Template == null);

            VerifyNotSealed();
            Template = template;
            GridRange = gridRange;
            Ordinal = ordinal;
        }

        public Style Style { get; internal set; }

        internal void OnCreated(UIElement element)
        {
            element.SetBinding(this);
            if (Style != null)
            {
                var frameworkElement = element as FrameworkElement;
                if (frameworkElement != null)
                    frameworkElement.Style = Style;
            }
        }

        internal void VerifyRowRange()
        {
            VerifyRowRange(Template.RowRange);
        }

        internal abstract void VerifyRowRange(GridRange rowRange);

        public int AutoSizeOrder { get; internal set; }

        public AutoSizeWaiver AutoSizeWaiver { get; internal set; }

        internal virtual AutoSizeWaiver CoercedAutoSizeWaiver
        {
            get { return AutoSizeWaiver; }
        }

        private IConcatList<GridColumn> _autoWidthGridColumns;
        internal void InvalidateAutoWidthGridColumns()
        {
            _autoWidthGridColumns = null;
        }

        internal bool IsAutoWidthWaived
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
                    _autoWidthGridColumns = GridRange.FilterColumns(x => x.IsAutoLength).Seal();
                return _autoWidthGridColumns;
            }
        }

        private IConcatList<GridRow> _autoHeightGridRows;
        internal void InvalidateAutoHeightGridRows()
        {
            _autoHeightGridRows = null;
        }

        internal bool IsAutoHeigthWaived
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
                    _autoHeightGridRows = GridRange.FilterRows(x => x.IsAutoLength).Seal();
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
                var width = AutoWidthGridColumns.Count > 0 ? double.PositiveInfinity : GridRange.MeasuredWidth;
                var height = AutoHeightGridRows.Count > 0 ? double.PositiveInfinity : GridRange.MeasuredHeight;
                return new Size(width, height);
            }
        }

        internal virtual void VerifyFrozenMargins(string bindingsName)
        {
            if (GridRange.HorizontallyIntersectsWith(Template.FrozenLeft))
                throw new InvalidOperationException(DiagnosticMessages.Binding_InvalidFrozenMargin(nameof(Template.FrozenLeft), bindingsName, Ordinal));
            if (GridRange.VerticallyIntersectsWith(Template.FrozenTop))
                throw new InvalidOperationException(DiagnosticMessages.Binding_InvalidFrozenMargin(nameof(Template.FrozenTop), bindingsName, Ordinal));
            if (GridRange.HorizontallyIntersectsWith(Template.GridColumns.Count - Template.FrozenRight))
                throw new InvalidOperationException(DiagnosticMessages.Binding_InvalidFrozenMargin(nameof(Template.FrozenRight), bindingsName, Ordinal));
            if (GridRange.VerticallyIntersectsWith(Template.GridRows.Count - Template.FrozenBottom))
                throw new InvalidOperationException(DiagnosticMessages.Binding_InvalidFrozenMargin(nameof(Template.FrozenBottom), bindingsName, Ordinal));
        }

        internal abstract UIElement GetSettingUpElement();

        internal abstract void EndSetup();

        internal abstract void Refresh(UIElement element);

        internal abstract void Cleanup(UIElement element);

        [DefaultValue(false)]
        public bool FrozenLeftShrink { get; internal set; }

        [DefaultValue(false)]
        public bool FrozenTopShrink { get; internal set; }

        [DefaultValue(false)]
        public bool FrozenRightShrink { get; internal set; }

        [DefaultValue(false)]
        public bool FrozenBottomShrink { get; internal set; }

        public abstract Type ViewType { get; }
    }
}
