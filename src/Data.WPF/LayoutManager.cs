using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    internal abstract partial class LayoutManager
    {
        internal static LayoutManager Create(DataSetPresenter presenter)
        {
            var orientation = presenter.Template.Orientation;

            if (orientation == GridOrientation.Z)
                return new LayoutZ(presenter);
            else if (orientation == GridOrientation.Y)
                return new LayoutY(presenter);
            else if (orientation == GridOrientation.XY)
                return new LayoutXY(presenter);
            else if (orientation == GridOrientation.X)
                return new LayoutX(presenter);
            else
            {
                Debug.Assert(orientation == GridOrientation.YX);
                return new LayoutYX(presenter);
            }
        }

        protected LayoutManager(DataSetPresenter presenter)
        {
            Debug.Assert(presenter != null);
            Presenter = presenter;
        }

        internal DataSetPresenter Presenter { get; private set; }

        internal DataSetView View
        {
            get { return Presenter == null ? null : Presenter.View; }
        }

        internal GridTemplate Template
        {
            get { return Presenter.Template; }
        }

        internal bool IsPinned
        {
            get { return Template.IsPinned; }
        }

        private IElementCollection _scrollableElements;
        internal IReadOnlyList<UIElement> ScrollableElements
        {
            get
            {
                EnsureElementCollectionInitialized();
                return _scrollableElements;
            }
        }

        private IElementCollection _pinnedElements;
        internal IReadOnlyList<UIElement> PinnedElements
        {
            get
            {
                EnsureElementCollectionInitialized();
                return _pinnedElements;
            }
        }


        private FrameworkElement _pinnedElementsParent;
        private FrameworkElement _scrollableElementsParent;

        internal void SetElementsParent(FrameworkElement pinnedElementsParent, FrameworkElement scrollableElementsParent)
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

            if (_scrollableElements == null)
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

        private Size _availableSize;
        internal Size AvailableSize
        {
            get { return _availableSize; }
            set
            {
                if (_availableSize.IsClose(value))
                    return;

                _availableSize = value;
                InvalidateMeasure();
            }
        }

        private Size _viewportSize;
        internal Size ViewportSize
        {
            get { return _viewportSize; }
            set
            {
                if (_viewportSize.IsClose(value))
                    return;

                _viewportSize = value;
                InvalidateScrollInfo();
                InvalidateMeasure();
            }
        }

        private Size _extentSize;
        internal Size ExtentSize
        {
            get { return _extentSize; }
            private set
            {
                if (_extentSize.IsClose(value))
                    return;
                _extentSize = value;
                InvalidateScrollInfo();
            }
        }

        private double _horizontalOffset;
        internal double HorizontalOffset
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
        internal double VerticalOffset
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

        internal ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        internal Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        private void InvalidateMeasure()
        {
            var dataSetView = Presenter.View;
            if (dataSetView != null)
            {
                dataSetView.InvalidateMeasure();
            }
        }

        internal void Measure(Size availableSize)
        {
        }

        private bool IsVirtualizing
        {
            get { return Template.IsVirtualizing; }
        }

        private GridOrientation Orientation
        {
            get { return Template.Orientation; }
        }

        //protected abstract int RepeatXCount { get; }

        //protected abstract int RepeatYCount { get; }

        //protected abstract Orientation MainOrientation { get; }

    }
}
