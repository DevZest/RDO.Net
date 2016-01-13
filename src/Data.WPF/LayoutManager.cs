using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    internal abstract partial class LayoutManager
    {
        internal static LayoutManager Create(DataView view)
        {
            var orientation = view.Template.ListOrientation;

            if (orientation == ListOrientation.Z)
                return new LayoutZ(view);
            else if (orientation == ListOrientation.Y)
                return new LayoutY(view);
            else if (orientation == ListOrientation.XY)
                return new LayoutXY(view);
            else if (orientation == ListOrientation.X)
                return new LayoutX(view);
            else
            {
                Debug.Assert(orientation == ListOrientation.YX);
                return new LayoutYX(view);
            }
        }

        protected LayoutManager(DataView view)
        {
            Debug.Assert(view != null);
            _view = view;
        }

        private DataView _view;

        private IReadOnlyList<RowView> Rows
        {
            get { return _view; }
        }

        private GridTemplate Template
        {
            get { return _view.Template; }
        }

        public bool IsPinned
        {
            get { return Template.IsPinned; }
        }

        private bool IsVirtualizing
        {
            get { return _view.IsVirtualizing; }
        }

        private ListOrientation Orientation
        {
            get { return Template.ListOrientation; }
        }

        private IElementCollection _scrollableElements;
        public IReadOnlyList<UIElement> ScrollableElements
        {
            get
            {
                EnsureElementCollectionInitialized();
                return _scrollableElements;
            }
        }

        private IElementCollection _pinnedElements;
        public IReadOnlyList<UIElement> PinnedElements
        {
            get
            {
                EnsureElementCollectionInitialized();
                return _pinnedElements;
            }
        }

        private FrameworkElement _pinnedElementsParent;
        private FrameworkElement _scrollableElementsParent;

        public void SetElementsParent(FrameworkElement pinnedElementsParent, FrameworkElement scrollableElementsParent)
        {
            Debug.Assert(_pinnedElementsParent != pinnedElementsParent || _scrollableElementsParent != scrollableElementsParent);
            Debug.Assert(IsPinned || (!IsPinned && pinnedElementsParent == null), "pinnedElementsParent must be null when IsPinned==false");

            _pinnedElementsParent = pinnedElementsParent;
            _scrollableElementsParent = scrollableElementsParent;

            if (_pinnedElements != null)
            {
                _pinnedElements.Clear();
                _pinnedElements = null;
            }

            if (_scrollableElements != null)
            {
                _scrollableElements.Clear();
                _scrollableElements = null;
            }
        }

        private void EnsureElementCollectionInitialized()
        {
            if (_scrollableElements != null)
                return;

            _pinnedElements = IsPinned ? IElementCollectionFactory.Create(_pinnedElementsParent) : null;
            _scrollableElements = IElementCollectionFactory.Create(_scrollableElementsParent);
        }

        public double ViewportWidth { get; private set; }

        public double ViewportHeight { get; private set; }

        public double ExtentHeight { get; private set; }

        public double ExtentWidth { get; private set; }

        private Size ExtentSize
        {
            set
            {
                if (ExtentHeight.IsClose(value.Height) && ExtentWidth.IsClose(value.Width))
                    return;
                ExtentHeight = value.Height;
                ExtentWidth = value.Width;
                InvalidateScrollInfo();
            }
        }

        private double _horizontalOffset;
        public double HorizontalOffset
        {
            get { return _horizontalOffset; }
            set
            {
                if (_horizontalOffset.IsClose(value))
                    return;

                _horizontalOffset = value;
                InvalidateScrollInfo();
                InvalidateMeasure();
            }
        }

        private double _verticalOffset;
        public double VerticalOffset
        {
            get { return _verticalOffset; }
            set
            {
                if (_verticalOffset.IsClose(value))
                    return;

                _verticalOffset = value;
                InvalidateScrollInfo();
                InvalidateMeasure();
            }
        }

        public ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        public void OnRowAdded(int index)
        {
            _isMeasureDirty = true;
            InvalidateMeasure();
        }

        public void OnRowRemoved(int index, RowView row)
        {
            _isMeasureDirty = true;
            InvalidateMeasure();
        }

        private bool _isMeasureDirty;
        public event EventHandler Invalidated;

        private void InvalidateMeasure()
        {
            _isMeasureDirty = true;
            var invalidated = Invalidated;
            if (invalidated != null)
                invalidated(this, EventArgs.Empty);
        }

        private const double DEFAULT_WIDTH = 200;
        private const double DEFAULT_HEIGHT = 200;

        public Size Measure(Size availableSize)
        {
            double width = availableSize.Width;
            if (double.IsPositiveInfinity(width))
                width = DEFAULT_WIDTH;
            double height = availableSize.Height;
            if (double.IsPositiveInfinity(height))
                height = DEFAULT_HEIGHT;

            ViewportSize = new Size(width, height);
            if (_isMeasureDirty)
                return ViewportSize;

            _isMeasureDirty = false;
            return ViewportSize;
        }

        private Size ViewportSize
        {
            get { return new Size(ViewportWidth, ViewportHeight); }
            set
            {
                if (ViewportWidth.IsClose(value.Width) && ViewportHeight.IsClose(value.Height))
                    return;

                ViewportWidth = value.Width;
                ViewportHeight = value.Height;

                InvalidateScrollInfo();
                if (!_isMeasureDirty)
                    _isMeasureDirty = true;
            }
        }

        public Size Arrange(Size finalSize)
        {
            return ViewportSize;
        }
    }
}
