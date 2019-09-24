using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Views.Primitives
{
    /// <summary>
    /// Represents panel of <see cref="RowView"/>.
    /// </summary>
    public class RowViewPanel : FrameworkElement
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RowViewPanel"/> class.
        /// </summary>
        public RowViewPanel()
        {
        }

        private RowView RowView
        {
            get { return TemplatedParent as RowView; }
        }

        private RowPresenter RowPresenter
        {
            get { return RowView?.RowPresenter; }
        }

        private LayoutManager LayoutManager
        {
            get
            {
                var rowPresenter = RowPresenter;
                return rowPresenter == null ? null : rowPresenter.LayoutManager;
            }
        }

        internal IReadOnlyList<UIElement> Elements
        {
            get
            {
                var rowView = RowView;
                if (rowView == null)
                    return Array.Empty<UIElement>();

                return rowView.Elements ?? Array.Empty<UIElement>();
            }
        }

        /// <inheritdoc/>
        protected override int VisualChildrenCount
        {
            get { return Elements.Count; }
        }

        /// <inheritdoc/>
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Elements[index];
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            var layoutManager = LayoutManager;
            if (layoutManager != null)
            {
                if (layoutManager.IsMeasuring)
                    return layoutManager.Measure(RowView, availableSize);
                else
                    layoutManager.InvalidateMeasure();
            }
            return base.MeasureOverride(availableSize);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var layoutManager = LayoutManager;
            if (layoutManager == null)
                return base.ArrangeOverride(finalSize);

            layoutManager.ArrangeChildren(RowView);
            return finalSize;
        }
    }
}
