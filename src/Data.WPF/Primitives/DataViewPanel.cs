using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
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
            get { return ScrollHandler.HorizontalOffset; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return ScrollHandler.VerticalOffset; }
        }

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return ScrollHandler.ScrollOwner; }
            set { ScrollHandler.ScrollOwner = value; }
        }

        void IScrollInfo.LineUp()
        {
            ScrollHandler.ScrollBy(0, -ScrollLineHeight);
        }

        void IScrollInfo.LineDown()
        {
            ScrollHandler.ScrollBy(0, ScrollLineHeight);
        }

        void IScrollInfo.LineLeft()
        {
            ScrollHandler.ScrollBy(-ScrollLineWidth, 0);
        }

        void IScrollInfo.LineRight()
        {
            ScrollHandler.ScrollBy(ScrollLineWidth, 0);
        }

        void IScrollInfo.PageUp()
        {
            ScrollHandler.ScrollBy(0, -ScrollHandler.ViewportHeight);
        }

        void IScrollInfo.PageDown()
        {
            ScrollHandler.ScrollBy(0, ScrollHandler.ViewportHeight);
        }

        void IScrollInfo.PageLeft()
        {
            ScrollHandler.ScrollBy(-ScrollHandler.ViewportWidth, 0);
        }

        void IScrollInfo.PageRight()
        {
            ScrollHandler.ScrollBy(ScrollHandler.ViewportWidth, 0);
        }

        void IScrollInfo.MouseWheelUp()
        {
            ScrollHandler.ScrollBy(0,  -ScrollLineHeight * SystemParameters.WheelScrollLines);
        }

        void IScrollInfo.MouseWheelDown()
        {
            ScrollHandler.ScrollBy(0, ScrollLineHeight * SystemParameters.WheelScrollLines);
        }

        void IScrollInfo.MouseWheelLeft()
        {
            ScrollHandler.ScrollBy(-ScrollLineWidth * SystemParameters.WheelScrollLines, 0);
        }

        void IScrollInfo.MouseWheelRight()
        {
            ScrollHandler.ScrollBy(ScrollLineWidth * SystemParameters.WheelScrollLines, 0);
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            ScrollHandler.ScrollTo(offset, ScrollHandler.VerticalOffset);
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            ScrollHandler.ScrollTo(ScrollHandler.HorizontalOffset, offset);
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            return ScrollHandler.MakeVisible(visual, rectangle);
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
                if (layoutManager == null || layoutManager.ElementCollection == null)
                    return Array<UIElement>.Empty;

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
