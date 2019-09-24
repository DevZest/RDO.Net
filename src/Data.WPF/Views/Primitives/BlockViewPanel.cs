using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Views.Primitives
{
    /// <summary>
    /// Represents panel of <see cref="BlockView"/>.
    /// </summary>
    public class BlockViewPanel : FrameworkElement
    {
        private BlockView BlockView
        {
            get { return TemplatedParent as BlockView; }
        }

        private LayoutManager LayoutManager
        {
            get { return BlockView == null ? null : BlockView.ScrollableManager; }
        }

        private IReadOnlyList<UIElement> Elements
        {
            get
            {
                var blockView = BlockView;
                if (blockView == null)
                    return Array.Empty<UIElement>();

                return blockView.Elements ?? Array.Empty<UIElement>();
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
                    return layoutManager.Measure(BlockView, availableSize);
                else
                    layoutManager.InvalidateMeasure();
            }
            return base.MeasureOverride(availableSize);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var layoutManager = LayoutManager;
            if (layoutManager == null)
                return base.ArrangeOverride(arrangeBounds);

            layoutManager.ArrangeChildren(BlockView);
            return arrangeBounds;
        }
    }
}
