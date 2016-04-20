using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public class BlockElementPanel : FrameworkElement
    {
        private BlockView BlockView
        {
            get { return TemplatedParent as BlockView; }
        }

        private LayoutManager LayoutManager
        {
            get { return BlockView == null ? null : BlockView.LayoutManager; }
        }

        private Template Template
        {
            get
            {
                Debug.Assert(LayoutManager != null);
                return LayoutManager.Template;
            }
        }

        private int BlockItemsSplit
        {
            get { return Template.BlockItemsSplit; }
        }

        private IReadOnlyList<UIElement> Elements
        {
            get
            {
                var blockView = BlockView;
                if (blockView == null)
                    return Array<UIElement>.Empty;

                if (blockView.ElementCollection == null || blockView.ElementCollection.Parent != this)
                    blockView.SetElementsPanel(this);

                Debug.Assert(blockView.ElementCollection.Parent == this);
                return blockView.ElementCollection;
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

        private LayoutManager SafeLayoutManager
        {
            get
            {
                var result = LayoutManager;
#if DEBUG
                if (result != null && Template.IsInitializingBlockView)
                    return null;
#endif
                return result;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var layoutManager = SafeLayoutManager;
            return layoutManager != null ? layoutManager.Measure(BlockView, availableSize) : base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var layoutManager = SafeLayoutManager;
            if (layoutManager == null)
                return base.ArrangeOverride(arrangeBounds);

            layoutManager.Arrange(BlockView);
            return arrangeBounds;
        }
    }
}
