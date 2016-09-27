using DevZest.Data.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public class RowElementPanel : FrameworkElement
    {
        public RowElementPanel()
        {
        }

        private RowView RowView
        {
            get { return TemplatedParent as RowView; }
        }

        private RowPresenter RowPresenter
        {
            get
            {
                var rowView = RowView;
                return rowView == null ? null : rowView.RowPresenter;
            }
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
                    return Array<UIElement>.Empty;

                return rowView.Elements ?? Array<UIElement>.Empty;
            }
        }

        protected override int VisualChildrenCount
        {
            get { return Elements.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Elements[index];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var layoutManager = LayoutManager;
            return layoutManager != null ? layoutManager.MeasureRow(RowView, availableSize) : base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var layoutManager = LayoutManager;
            if (layoutManager == null)
                return base.ArrangeOverride(finalSize);

            layoutManager.ArrangeRow(RowView);
            return finalSize;
        }
    }
}
