using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class DataElementPanel : FrameworkElement, IScrollInfo
    {
        #region IScrollInfo

        private double ScrollLineHeight
        {
            get { return DataView.ScrollLineHeight; }
        }

        private double ScrollLineWidth
        {
            get { return DataView.ScrollLineWidth; }
        }

        bool _canVerticallyScroll;
        bool IScrollInfo.CanVerticallyScroll
        {
            get { return _canVerticallyScroll; }
            set { _canVerticallyScroll = value; }
        }

        bool _canHorizontallyScroll;
        bool IScrollInfo.CanHorizontallyScroll
        {
            get { return _canHorizontallyScroll; }
            set { _canHorizontallyScroll = value; }
        }

        double IScrollInfo.ExtentWidth
        {
            get { return ScrollHandler.ExtentWidth; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return ScrollHandler.ExtentHeight; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return ScrollHandler.ViewportWidth; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return ScrollHandler.ViewportHeight; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return ScrollHandler.ScrollOffsetX; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return ScrollHandler.ScrollOffsetY; }
        }

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return ScrollHandler.ScrollOwner; }
            set { ScrollHandler.ScrollOwner = value; }
        }

        void IScrollInfo.LineUp()
        {
            ScrollHandler.ScrollOffsetY -= ScrollLineHeight;
        }

        void IScrollInfo.LineDown()
        {
            ScrollHandler.ScrollOffsetY += ScrollLineHeight;
        }

        void IScrollInfo.LineLeft()
        {
            ScrollHandler.ScrollOffsetX -= ScrollLineWidth;
        }

        void IScrollInfo.LineRight()
        {
            ScrollHandler.ScrollOffsetX += ScrollLineWidth;
        }

        void IScrollInfo.PageUp()
        {
            ScrollHandler.ScrollOffsetY -= ScrollHandler.ViewportHeight;
        }

        void IScrollInfo.PageDown()
        {
            ScrollHandler.ScrollOffsetY += ScrollHandler.ViewportHeight;
        }

        void IScrollInfo.PageLeft()
        {
            ScrollHandler.ScrollOffsetX -= ScrollHandler.ViewportWidth;
        }

        void IScrollInfo.PageRight()
        {
            ScrollHandler.ScrollOffsetX += ScrollHandler.ViewportWidth;
        }

        void IScrollInfo.MouseWheelUp()
        {
            ScrollHandler.ScrollOffsetY -= ScrollLineHeight * SystemParameters.WheelScrollLines;
        }

        void IScrollInfo.MouseWheelDown()
        {
            ScrollHandler.ScrollOffsetY += ScrollLineHeight * SystemParameters.WheelScrollLines;
        }

        void IScrollInfo.MouseWheelLeft()
        {
            ScrollHandler.ScrollOffsetX -= ScrollLineWidth * SystemParameters.WheelScrollLines;
        }

        void IScrollInfo.MouseWheelRight()
        {
            ScrollHandler.ScrollOffsetX += ScrollLineWidth * SystemParameters.WheelScrollLines;
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            ScrollHandler.ScrollOffsetX = offset;
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            ScrollHandler.ScrollOffsetY = offset;
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            return ScrollHandler.MakeVisible(visual, rectangle);
        }

        #endregion

        static DataElementPanel()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(DataElementPanel), new FrameworkPropertyMetadata(BooleanBoxes.True));
        }

        private DataView DataView
        {
            get { return TemplatedParent as DataView; }
        }

        private DataPresenter DataPresenter
        {
            get
            {
                var dataView = DataView;
                return dataView == null ? null : dataView.DataPresenter;
            }
        }

        private LayoutManager LayoutManager
        {
            get { return DataPresenter == null ? null : DataPresenter.LayoutManager; }
        }

        private IScrollHandler ScrollHandler
        {
            get
            {
                var result = LayoutManager as IScrollHandler;
                if (result == null)
                    throw new InvalidOperationException(Strings.DataElementPanel_NullScrollHandler);
                return result;
            }
        }

        internal IReadOnlyList<UIElement> Elements
        {
            get
            {
                var layoutManager = LayoutManager;
                if (layoutManager == null)
                    return Array<UIElement>.Empty;

                if (layoutManager.ElementCollection == null || layoutManager.ElementCollection.Parent != this)
                    layoutManager.SetElementsPanel(this);

                Debug.Assert(layoutManager.ElementCollection.Parent == this);
                return layoutManager.ElementCollection;
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
            return layoutManager == null ? base.MeasureOverride(availableSize) : layoutManager.Measure(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var layoutManager = LayoutManager;
            return layoutManager == null ? base.ArrangeOverride(finalSize) : layoutManager.Arrange(finalSize);
        }
    }
}
