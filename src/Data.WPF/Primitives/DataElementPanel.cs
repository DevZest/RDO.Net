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
            get { return ScrollHandler.ExtentX; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return ScrollHandler.ExtentY; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return ScrollHandler.ViewportX; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return ScrollHandler.ViewportY; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return ScrollHandler.OffsetX; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return ScrollHandler.OffsetY; }
        }

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return ScrollHandler.ScrollOwner; }
            set { ScrollHandler.ScrollOwner = value; }
        }

        void IScrollInfo.LineUp()
        {
            ScrollHandler.OffsetY -= ScrollLineHeight;
        }

        void IScrollInfo.LineDown()
        {
            ScrollHandler.OffsetY += ScrollLineHeight;
        }

        void IScrollInfo.LineLeft()
        {
            ScrollHandler.OffsetX -= ScrollLineWidth;
        }

        void IScrollInfo.LineRight()
        {
            ScrollHandler.OffsetX += ScrollLineWidth;
        }

        void IScrollInfo.PageUp()
        {
            ScrollHandler.OffsetY -= ScrollHandler.ViewportY;
        }

        void IScrollInfo.PageDown()
        {
            ScrollHandler.OffsetY += ScrollHandler.ViewportY;
        }

        void IScrollInfo.PageLeft()
        {
            ScrollHandler.OffsetX -= ScrollHandler.ViewportX;
        }

        void IScrollInfo.PageRight()
        {
            ScrollHandler.OffsetX += ScrollHandler.ViewportX;
        }

        void IScrollInfo.MouseWheelUp()
        {
            ScrollHandler.OffsetY -= ScrollLineHeight * SystemParameters.WheelScrollLines;
        }

        void IScrollInfo.MouseWheelDown()
        {
            ScrollHandler.OffsetY += ScrollLineHeight * SystemParameters.WheelScrollLines;
        }

        void IScrollInfo.MouseWheelLeft()
        {
            ScrollHandler.OffsetX -= ScrollLineWidth * SystemParameters.WheelScrollLines;
        }

        void IScrollInfo.MouseWheelRight()
        {
            ScrollHandler.OffsetX += ScrollLineWidth * SystemParameters.WheelScrollLines;
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            ScrollHandler.OffsetX = offset;
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            ScrollHandler.OffsetY = offset;
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
