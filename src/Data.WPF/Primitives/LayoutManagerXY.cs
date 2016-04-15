using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutManagerXY : LayoutManager, IScrollHandler
    {
        private struct GridTrackPoint
        {
            private double _value;

            public GridTrackPoint(int index, double proportion)
            {
                _value = index + proportion;
            }

            public int Index
            {
                get { return (int)_value; }
            }

            public double Proportion
            {
                get { return _value - Index; }
            }
        }

        #region IScrollHandler

        public ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        public double ViewportWidth
        {
            get { return Template.AvailableWidth; }
        }

        public double ViewportHeight
        {
            get { return Template.AvailableHeight; }
        }

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
            }
        }

        private double _deltaHorizontalOffset;
        public double DeltaHorizontalOffset
        {
            get { return _deltaHorizontalOffset; }
            set
            {
                if (_deltaHorizontalOffset.IsClose(value))
                    return;
                _deltaHorizontalOffset = value;
                InvalidateScrollInfo();
            }
        }

        private double _deltaVerticalOffset;
        public double DeltaVerticalOffset
        {
            get { return _deltaVerticalOffset; }
            set
            {
                if (_deltaVerticalOffset.IsClose(value))
                    return;
                _deltaVerticalOffset = value;
                InvalidateScrollInfo();
            }
        }

        public void SetHorizontalOffset(double offset)
        {
            throw new NotImplementedException();
        }

        public void SetVerticalOffset(double offset)
        {
            throw new NotImplementedException();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        #endregion

        public LayoutManagerXY(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
        }

        private Orientation Orientation
        {
            get
            {
                Debug.Assert(Template.Orientation.HasValue);
                return Template.Orientation.GetValueOrDefault();
            }
        }

        protected override void PrepareMeasureBlocks()
        {
            throw new NotImplementedException();
        }

        internal override Size GetMeasuredSize(DataItem dataItem, int blockDimension)
        {
            throw new NotImplementedException();
        }

        internal override Rect GetArrangeRect(DataItem dataItem, int blockDimension)
        {
            throw new NotImplementedException();
        }

        internal override Size GetMeasuredSize(BlockView blockView, GridRange gridRange)
        {
            throw new NotImplementedException();
        }

        protected override void FinalizeMeasureBlocks()
        {
            throw new NotImplementedException();
        }

        protected override Size MeasuredSize
        {
            get { return Template.AvailableSize; }
        }
    }
}
