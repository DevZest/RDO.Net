using System;
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

            ScrollableElements = new ObservableCollection<UIElement>();
            PinnedElements = IsPinned ? new ObservableCollection<UIElement>() : null;
            Invalidate();
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

        internal ObservableCollection<UIElement> ScrollableElements { get; private set; }

        internal ObservableCollection<UIElement> PinnedElements { get; private set; }

        private Size _availableSize;
        internal Size AvailableSize
        {
            get { return _availableSize; }
            set
            {
                if (_availableSize.IsClose(value))
                    return;

                _availableSize = value;
                Invalidate();
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
                Invalidate();
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
                Invalidate();
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
                Invalidate();
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

        private void Invalidate()
        {
            var dataSetView = Presenter.View;
            if (dataSetView != null)
            {
                dataSetView.InvalidateMeasure();
                dataSetView.InvalidateVisual();
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
