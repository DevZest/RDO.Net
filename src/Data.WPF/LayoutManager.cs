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
        internal LayoutManager(DataSetManager dataSetManager)
        {
            Debug.Assert(dataSetManager != null);
            DataSetManager = dataSetManager;

            ScrollableElements = new ObservableCollection<UIElement>();
            PinnedElements = IsPinned ? new ObservableCollection<UIElement>() : null;
            Invalidate();
        }

        internal DataSetManager DataSetManager { get; private set; }

        internal DataSetControl DataSetControl
        {
            get { return DataSetManager == null ? null : DataSetManager.DataSetControl; }
        }

        internal GridTemplate Template
        {
            get { return DataSetManager.Template; }
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
            var dataSetControl = DataSetManager.DataSetControl;
            if (dataSetControl != null)
            {
                dataSetControl.InvalidateMeasure();
                dataSetControl.InvalidateVisual();
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
