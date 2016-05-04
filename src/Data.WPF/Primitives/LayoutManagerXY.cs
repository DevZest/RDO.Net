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

        private struct RelativeOffset
        {
            public RelativeOffset(int gridOffset, double fractionOffset)
            {
                Debug.Assert(gridOffset >= 0);
                Debug.Assert(fractionOffset >= 0 && fractionOffset < 1);
                _value = gridOffset + fractionOffset;
            }

            private readonly double _value;
            public int GridOffset
            {
                get { return (int)_value; }
            }

            public double FractionOffset
            {
                get { return _value - GridOffset; }
            }
        }

        private struct GridOffset
        {
            public static GridOffset Eof
            {
                get { return new GridOffset(); }
            }

            public static GridOffset New(GridTrack gridTrack)
            {
                Debug.Assert(!gridTrack.IsRepeat);
                return new GridOffset(gridTrack, -1);
            }

            public static GridOffset New(GridTrack gridTrack, int blockOffset)
            {
                Debug.Assert(gridTrack.IsRepeat);
                return new GridOffset(gridTrack, blockOffset);
            }

            private GridOffset(GridTrack gridTrack, int blockOffset)
            {
                GridTrack = gridTrack;
                _blockOffset = blockOffset;
            }

            public readonly GridTrack GridTrack;
            private readonly int _blockOffset;
            public int BlockOffset
            {
                get { return GridTrack == null || !GridTrack.IsRepeat ? -1 : _blockOffset; }
            }

            public bool IsEof
            {
                get { return GridTrack == null; }
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

        public double HorizontalOffset { get; private set; }

        public double VerticalOffset { get; private set; }

        private Vector ScrollOffset
        {
            set
            {
                if (HorizontalOffset.IsClose(value.X) && VerticalOffset.IsClose(value.Y))
                    return;
                HorizontalOffset = value.X;
                VerticalOffset = value.Y;
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

        private LayoutManagerXY(IGridTrackCollection mainAxisGridTracks, DataSet dataSet)
            : base(mainAxisGridTracks.Template, dataSet)
        {
            _mainAxisGridTracks = mainAxisGridTracks;
            VariantAutoLengthTracks = mainAxisGridTracks.InitVariantAutoLengthTracks();
        }

        private readonly IGridTrackCollection _mainAxisGridTracks;
        private RelativeOffset _mainScrollOffset;
        private double _crossScrollOffset;
        private double _avgVariantAutoLength;

        private double TranslateRelativeOffset(RelativeOffset relativeOffset)
        {
            throw new NotImplementedException();
        }

        private RelativeOffset TranslateRelativeOffset(double offset)
        {
            throw new NotImplementedException();
        }

        private GridOffset TranslateGridOffset(int gridOffset)
        {
            Debug.Assert(gridOffset >= 0 && gridOffset < MaxFrozenHead + TotalBlockGridTracks + MaxFrozenTail);
            if (gridOffset < MaxFrozenHead)
                return GridOffset.New(_mainAxisGridTracks[gridOffset]);

            gridOffset -= MaxFrozenHead;
            var totalBlockGridTracks = TotalBlockGridTracks;
            if (gridOffset < totalBlockGridTracks)
                return GridOffset.New(_mainAxisGridTracks[MaxFrozenHead + gridOffset % BlockGridTracks], gridOffset / BlockGridTracks);

            gridOffset -= totalBlockGridTracks;
            Debug.Assert(gridOffset < MaxFrozenTail);
            return GridOffset.New(_mainAxisGridTracks[MaxFrozenHead + totalBlockGridTracks + gridOffset]);
        }

        private int MaxFrozenHead
        {
            get { return _mainAxisGridTracks.MaxFrozenHead; }
        }

        private int BlockGridTracks
        {
            get { return _mainAxisGridTracks.BlockEnd.Ordinal - _mainAxisGridTracks.BlockStart.Ordinal + 1; }
        }

        private int TotalBlockGridTracks
        {
            get { return BlockViews.MaxBlockCount * BlockGridTracks; }
        }

        private int MaxFrozenTail
        {
            get { return _mainAxisGridTracks.MaxFrozenTail; }
        }

        internal IReadOnlyList<GridTrack> VariantAutoLengthTracks { get; private set; }

        private Vector BlockDimensionVector
        {
            get { return _mainAxisGridTracks.BlockDimensionVector; }
        }

        protected sealed override Point Offset(Point point, int blockDimension)
        {
            return point + BlockDimensionVector * blockDimension;
        }

        protected sealed override Size GetMeasuredSize(ScalarItem scalarItem)
        {
            var size = scalarItem.GridRange.MeasuredSize;
            if (!scalarItem.IsMultidimensional && BlockDimensions > 1)
            {
                var delta = BlockDimensionVector * (BlockDimensions - 1);
                size = new Size(size.Width + delta.X, size.Height + delta.Y);
            }
            return size;
        }

        protected sealed override double GetMeasuredLength(BlockView blockView, GridTrack gridTrack)
        {
            return blockView != null && gridTrack.IsVariantAutoLength ? blockView.GetMeasuredAutoLength(gridTrack) : base.GetMeasuredLength(blockView, gridTrack);
        }

        protected override bool SetMeasuredAutoLength(BlockView blockView, GridTrack gridTrack, double value)
        {
            if (blockView != null && gridTrack.IsVariantAutoLength)
            {
                blockView.SetMeasuredAutoLength(gridTrack, value);
                return false;
            }
            else
                return base.SetMeasuredAutoLength(blockView, gridTrack, value);
        }

        private bool _isVariantAutoLengthOffsetValid = true;
        internal void InvalidateVariantAutoLengthOffset()
        {
            _isVariantAutoLengthOffsetValid = false;
        }

        internal void RefreshVariantAutoLengthOffset()
        {
            if (_isVariantAutoLengthOffsetValid)
                return;

            _isVariantAutoLengthOffsetValid = true; // Avoid re-entrance
            for (int i = 1; i < BlockViews.Count; i++)
                BlockViews[i].StartMeasuredAutoLengthOffset = BlockViews[i - 1].EndMeasuredAutoLengthOffset;
        }

        protected sealed override void PrepareMeasureBlocks()
        {

            RefreshExtentSize();
            RefreshViewportSize();
        }

        private void ClearMeasuredAutoLengths()
        {
            if (VariantAutoLengthTracks.Count > 0)
            {
                for (int i = 0; i < BlockViews.Count; i++)
                    BlockViews[i].ClearMeasuredAutoLengths();
            }
        }

        private void RefreshScrollOfffset()
        {
            ScrollOffset = _mainAxisGridTracks.ToVector(TranslateRelativeOffset(_mainScrollOffset), _crossScrollOffset);
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
