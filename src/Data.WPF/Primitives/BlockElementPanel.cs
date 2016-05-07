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

        protected override Size MeasureOverride(Size availableSize)
        {
            var layoutManager = LayoutManager;
            return layoutManager != null ? layoutManager.MeasureBlock(BlockView, availableSize) : base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var layoutManager = LayoutManager;
            if (layoutManager == null)
                return base.ArrangeOverride(arrangeBounds);

            layoutManager.ArrangeBlock(BlockView);
            return arrangeBounds;
        }
    }
}
