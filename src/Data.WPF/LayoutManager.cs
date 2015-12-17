using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    internal class LayoutManager
    {
        internal LayoutManager(DataSetPresenter presenter)
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

        private bool _isDirty;

        private void Invalidate()
        {
            _isDirty = true;
            var dataSetView = Presenter.View;
            if (dataSetView != null)
            {
                dataSetView.InvalidateMeasure();
                dataSetView.InvalidateVisual();
            }
        }

        internal void Refresh()
        {
            if (!_isDirty)
                return;

            _isDirty = false;
        }
    }
}
