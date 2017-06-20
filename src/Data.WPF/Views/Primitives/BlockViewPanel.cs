using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Views.Primitives
{
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
                    return Array<UIElement>.Empty;

                return blockView.Elements ?? Array<UIElement>.Empty;
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
            return layoutManager != null ? layoutManager.Measure(BlockView, availableSize) : base.MeasureOverride(availableSize);
        }

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
