using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract partial class LayoutManagerXY : LayoutManager, IScrollHandler
    {
        public static new LayoutManagerXY Create(Template template, DataSet dataSet)
        {
            if (template.Orientation.Value == Orientation.Horizontal)
                return new X(template, dataSet);
            else
                return new Y(template, dataSet);
        }

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

        public double ViewportWidth { get; private set; }

        public double ViewportHeight { get; private set; }

        protected Size ViewportSize
        {
            get { return new Size(ViewportWidth, ViewportHeight); }
            private set
            {
                if (ViewportWidth.IsClose(value.Width) && ViewportHeight.IsClose(value.Height))
                    return;
                ViewportWidth = value.Width;
                ViewportHeight = value.Height;
                InvalidateScrollInfo();
            }
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
            private set
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
            private set
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

        private LayoutManagerXY(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
        }

        private static IConcatList<T> CalcVariantAutoLengthTracks<T>(GridTrackCollection<T> gridTracks, T startTrack, T endTrack)
            where T : GridTrack, IConcatList<T>
        {
            var result = ConcatList<T>.Empty;
            if (startTrack == null)
                return result;

            for (int i = startTrack.Ordinal; i <= startTrack.Ordinal; i++)
            {
                var gridTrack = gridTracks[i];
                if (gridTrack.Length.IsAuto)
                {
                    result = result.Concat(gridTrack);
                    gridTrack.IsVariantAutoLength = true;
                }
            }
            return result;
        }

        protected abstract IReadOnlyList<GridTrack> VariantAutoLengthTracks { get; }

        protected sealed override double GetMeasuredLength(BlockView blockView, GridTrack gridTrack)
        {
            return blockView != null && gridTrack.IsVariantAutoLength ? blockView.GetMeasuredLength(gridTrack) : base.GetMeasuredLength(blockView, gridTrack);
        }

        protected override bool SetMeasuredAutoLength(BlockView blockView, GridTrack gridTrack, double value)
        {
            if (blockView != null && gridTrack.IsVariantAutoLength)
            {
                blockView.SetMeasuredLength(gridTrack, value);
                return false;
            }
            else
                return base.SetMeasuredAutoLength(blockView, gridTrack, value);
        }

        protected sealed override void PrepareMeasureBlocks()
        {
            if (VariantAutoLengthTracks.Count > 0)
            {
                for (int i = 0; i < BlockViews.Count; i++)
                    BlockViews[i].ClearMeasuredLengths();
            }

            RefreshExtentSize();
            RefreshViewportSize();
        }

        private void RefreshExtentSize()
        {
            throw new NotImplementedException();
        }

        private void RefreshViewportSize()
        {
            var width = Template.SizeToContentX ? ExtentWidth : Template.AvailableWidth;
            var height = Template.SizeToContentY ? ExtentHeight : Template.AvailableHeight;
            ViewportSize = new Size(width, height);
        }

        protected override Size GetMeasuredSize(BlockView blockView, GridRange gridRange)
        {
            throw new NotImplementedException();
        }

        protected override Point GetOffset(BlockView blockView, GridRange baseGridRange, GridRange gridRange)
        {
            throw new NotImplementedException();
        }

        protected override void FinalizeMeasureBlocks()
        {
            throw new NotImplementedException();
        }

        protected sealed override Size MeasuredSize
        {
            get { return ViewportSize; }
        }

        internal override Rect GetArrangeRect(BlockView blockView, BlockItem blockItem)
        {
            throw new NotImplementedException();
        }
    }
}
