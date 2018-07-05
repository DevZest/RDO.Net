using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Views.Primitives
{
    public sealed class DataViewPanel : FrameworkElement, IScrollInfo
    {
        private class GridLineLayer : UIElement
        {
            public GridLineLayer(DataViewPanel dataElementPanel)
            {
                _dataElementPanel = dataElementPanel;
            }

            private DataViewPanel _dataElementPanel;

            private LayoutManager LayoutManager
            {
                get { return _dataElementPanel.LayoutManager; }
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                var layoutManager = LayoutManager;
                if (layoutManager == null)
                    return;

                foreach (var gridLineFigure in layoutManager.GridLineFigures)
                {
                    var pen = gridLineFigure.GridLine.Pen;
                    var startPoint = gridLineFigure.StartPoint;
                    var endPoint = gridLineFigure.EndPoint;
                    drawingContext.DrawLine(pen, startPoint, endPoint);
                }
            }
        }

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
            get
            {
                var scrollHandler = ScrollHandler;
                return scrollHandler != null ? scrollHandler.ExtentWidth : 0;
            }
        }

        double IScrollInfo.ExtentHeight
        {
            get
            {
                var scrollHandler = ScrollHandler;
                return scrollHandler != null ? scrollHandler.ExtentHeight : 0;
            }
        }

        double IScrollInfo.ViewportWidth
        {
            get
            {
                var scrollHandler = ScrollHandler;
                return scrollHandler != null ? scrollHandler.ViewportWidth : 0;
            }
        }

        double IScrollInfo.ViewportHeight
        {
            get
            {
                var scrollHandler = ScrollHandler;
                return scrollHandler != null ? scrollHandler.ViewportHeight : 0;
            }
        }

        double IScrollInfo.HorizontalOffset
        {
            get
            {
                var scrollHandler = ScrollHandler;
                return scrollHandler != null ? scrollHandler.HorizontalOffset : 0;
            }
        }

        double IScrollInfo.VerticalOffset
        {
            get
            {
                var scrollHandler = ScrollHandler;
                return scrollHandler != null ? scrollHandler.VerticalOffset : 0;
            }
        }

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get
            {
                var scrollHandler = ScrollHandler;
                return scrollHandler != null ? scrollHandler.ScrollOwner : null;
            }
            set
            {
                var scrollHandler = ScrollHandler;
                if (scrollHandler != null)
                    scrollHandler.ScrollOwner = value;
            }
        }

        void IScrollInfo.LineUp()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(0, -ScrollLineHeight);
        }

        void IScrollInfo.LineDown()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(0, ScrollLineHeight);
        }

        void IScrollInfo.LineLeft()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(-ScrollLineWidth, 0);
        }

        void IScrollInfo.LineRight()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(ScrollLineWidth, 0);
        }

        void IScrollInfo.PageUp()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(0, -scrollHandler.ScrollableHeight.IfZero(ScrollLineHeight));
        }

        void IScrollInfo.PageDown()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(0, scrollHandler.ScrollableHeight.IfZero(ScrollLineHeight));
        }

        void IScrollInfo.PageLeft()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(-scrollHandler.ScrollableWidth.IfZero(ScrollLineWidth), 0);
        }

        void IScrollInfo.PageRight()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(scrollHandler.ScrollableWidth.IfZero(ScrollLineWidth), 0);
        }

        void IScrollInfo.MouseWheelUp()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(0,  -ScrollLineHeight * SystemParameters.WheelScrollLines);
        }

        void IScrollInfo.MouseWheelDown()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(0, ScrollLineHeight * SystemParameters.WheelScrollLines);
        }

        void IScrollInfo.MouseWheelLeft()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(-ScrollLineWidth * SystemParameters.WheelScrollLines, 0);
        }

        void IScrollInfo.MouseWheelRight()
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollBy(ScrollLineWidth * SystemParameters.WheelScrollLines, 0);
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollTo(offset, scrollHandler.VerticalOffset);
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                scrollHandler.ScrollTo(scrollHandler.HorizontalOffset, offset);
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            var scrollHandler = ScrollHandler;
            if (scrollHandler != null)
                return scrollHandler.MakeVisible(visual, rectangle);
            else
                return new Rect();
        }

        #endregion

        static DataViewPanel()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(DataViewPanel), new FrameworkPropertyMetadata(BooleanBoxes.True));
        }

        public DataViewPanel()
        {
            _gridLineLayer = new GridLineLayer(this);
            AddLogicalChild(_gridLineLayer);
            AddVisualChild(_gridLineLayer);
        }

        private GridLineLayer _gridLineLayer;

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
            get { return LayoutManager as IScrollHandler; }
        }

        internal IReadOnlyList<UIElement> Elements
        {
            get
            {
                var layoutManager = LayoutManager;
                if (layoutManager == null || layoutManager.ElementCollection == null)
                    return Array.Empty<UIElement>();

                Debug.Assert(layoutManager.ElementCollection.Parent == this);
                return layoutManager.ElementCollection;
            }
        }

        protected override int VisualChildrenCount
        {
            get { return Elements.Count + 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return index == VisualChildrenCount - 1 ? _gridLineLayer : Elements[index];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var layoutManager = LayoutManager;
            var result = layoutManager == null ? base.MeasureOverride(availableSize) : layoutManager.Measure(availableSize);
            _gridLineLayer.InvalidateVisual();
            _gridLineLayer.Measure(result);
            return result;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var layoutManager = LayoutManager;
            var result = layoutManager == null ? base.ArrangeOverride(finalSize) : layoutManager.Arrange(finalSize);
            _gridLineLayer.Arrange(new Rect(result));
            return result;
        }
    }
}
