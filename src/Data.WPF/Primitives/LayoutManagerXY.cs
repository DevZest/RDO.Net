using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

        private struct OffsetSpan
        {
            public readonly double StartOffset;
            public readonly double EndOffset;

            public OffsetSpan(double startOffset, double endOffset)
            {
                StartOffset = startOffset;
                EndOffset = endOffset;
            }

            public double Length
            {
                get { return EndOffset - StartOffset; }
            }
        }

        private struct RelativeOffset
        {
            public RelativeOffset(double value)
            {
                _value = value;
            }

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

            private GridOffset(GridTrack gridTrack, int blockOrdinal)
            {
                GridTrack = gridTrack;
                _blockOrdinal = blockOrdinal;
            }

            public readonly GridTrack GridTrack;
            private readonly int _blockOrdinal;
            public int BlockOrdinal
            {
                get { return GridTrack == null || !GridTrack.IsRepeat ? -1 : _blockOrdinal; }
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

        private void SetExtentSize(Vector value)
        {
            if (ExtentWidth.IsClose(value.X) && ExtentHeight.IsClose(value.Y))
                return;
            ExtentWidth = value.X;
            ExtentHeight = value.Y;
            InvalidateScrollInfo();
        }

        public double HorizontalOffset { get; private set; }

        public double VerticalOffset { get; private set; }

        private void SetScrollOffset(Vector value)
        {
            if (HorizontalOffset.IsClose(value.X) && VerticalOffset.IsClose(value.Y))
                return;
            HorizontalOffset = value.X;
            VerticalOffset = value.Y;
            InvalidateScrollInfo();
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

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Derived classes are limited to X or Y, and the overrides do not rely on completion of its constructor.")]
        private LayoutManagerXY(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
            VariantAutoLengthTracks = MainAxisGridTracks.InitVariantAutoLengthTracks();
        }

        protected abstract IGridTrackCollection MainAxisGridTracks { get; }
        protected abstract IGridTrackCollection CrossAxisGridTracks { get; }
        internal IReadOnlyList<GridTrack> VariantAutoLengthTracks { get; private set; }
        private RelativeOffset MainScrollOffset;
        private double CrossScrollOffset;

        private double TotalVariantAutoLength
        {
            get
            {
                Debug.Assert(BlockViews.Count > 0);
                return BlockViews.Last.EndMeasuredAutoLengthOffset - BlockViews.First.StartMeasuredAutoLengthOffset;
            }
        }

        private double AvgVariantAutoLength
        {
            get { return BlockViews.Count == 0 ? 0 : TotalVariantAutoLength / BlockViews.Count; }
        }

        private double FixBlockLength
        {
            get { return MainAxisGridTracks.BlockEnd.EndOffset - MainAxisGridTracks.BlockStart.StartOffset; }
        }

        private double AvgBlockLength
        {
            get { return FixBlockLength + AvgVariantAutoLength; }
        }

        private Vector ToVector(double mainLength, double crossLength)
        {
            return MainAxisGridTracks.ToVector(mainLength, crossLength);
        }

        private double TranslateRelativeOffset(RelativeOffset relativeOffset)
        {
            var offsetSpan = GetOffsetSpan(relativeOffset.GridOffset);
            return offsetSpan.StartOffset + offsetSpan.Length * relativeOffset.FractionOffset;
        }

        private RelativeOffset TranslateRelativeOffset(double offset)
        {
            // Binary search
            var min = 0;
            var max = MaxGridOffset - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                var offsetSpan = GetOffsetSpan(mid);
                if (offset < offsetSpan.StartOffset)
                    max = mid - 1;
                else if (offset >= offsetSpan.EndOffset)
                    min = mid + 1;
                else
                    return new RelativeOffset(mid, (offset - offsetSpan.StartOffset) / offsetSpan.Length);
            }

            return new RelativeOffset(MaxGridOffset);
        }

        private OffsetSpan GetOffsetSpan(int gridOffset)
        {
            Debug.Assert(gridOffset >= 0 && gridOffset < MaxGridOffset);
            return GetOffsetSpan(TranslateGridOffset(gridOffset));
        }

        private OffsetSpan GetOffsetSpan(GridOffset gridOffset)
        {
            Debug.Assert(!gridOffset.IsEof);

            var gridTrack = gridOffset.GridTrack;
            if (gridTrack.IsHead)
                return new OffsetSpan(gridTrack.StartOffset, gridTrack.EndOffset);

            if (gridTrack.IsRepeat)
                return GetOffsetSpan(gridTrack, gridOffset.BlockOrdinal);

            Debug.Assert(gridTrack.IsTail);
            var delta = MaxBlockCount * AvgBlockLength;
            if (MaxBlockCount > 0)
                delta -= FixBlockLength;    // minus duplicated FixBlockLength
            return new OffsetSpan(gridTrack.StartOffset + delta, gridTrack.EndOffset + delta);
        }

        private OffsetSpan GetOffsetSpan(GridTrack gridTrack, int blockOrdinal)
        {
            Debug.Assert(blockOrdinal >= 0);

            var blockStartOffset = MainAxisGridTracks[MaxFrozenHead].StartOffset;
            var avgBlockLength = AvgBlockLength;
            if (BlockViews.Count == 0 || blockOrdinal < BlockViews.First.Ordinal || blockOrdinal > BlockViews.Last.Ordinal)
            {
                var prevBlockEndOffset = blockOrdinal == 0 ? blockStartOffset : blockStartOffset + (blockOrdinal - 1) * avgBlockLength;
                //gridTrack.IsVariantAutoLength
            }
            throw new NotImplementedException();
        }

        private GridOffset TranslateGridOffset(int gridOffset)
        {
            Debug.Assert(gridOffset >= 0 && gridOffset <= MaxGridOffset);
            if (gridOffset < MaxFrozenHead)
                return GridOffset.New(MainAxisGridTracks[gridOffset]);

            gridOffset -= MaxFrozenHead;
            var totalBlockGridTracks = TotalBlockGridTracks;
            if (gridOffset < totalBlockGridTracks)
                return GridOffset.New(MainAxisGridTracks[MaxFrozenHead + gridOffset % BlockGridTracks], gridOffset / BlockGridTracks);

            gridOffset -= totalBlockGridTracks;
            Debug.Assert(gridOffset < MaxFrozenTail);
            return GridOffset.New(MainAxisGridTracks[MaxFrozenHead + BlockGridTracks + gridOffset]);
        }

        private int MaxBlockCount
        {
            get { return BlockViews.MaxBlockCount; }
        }

        private int MaxFrozenHead
        {
            get { return MainAxisGridTracks.MaxFrozenHead; }
        }

        private int BlockGridTracks
        {
            get { return MainAxisGridTracks.BlockEnd.Ordinal - MainAxisGridTracks.BlockStart.Ordinal + 1; }
        }

        private int TotalBlockGridTracks
        {
            get { return MaxBlockCount * BlockGridTracks; }
        }

        private int MaxFrozenTail
        {
            get { return MainAxisGridTracks.MaxFrozenTail; }
        }

        private int MaxGridOffset
        {
            get { return MaxFrozenHead + TotalBlockGridTracks + MaxFrozenTail; }
        }

        private Vector BlockDimensionVector
        {
            get { return MainAxisGridTracks.BlockDimensionVector; }
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

        private bool _isVariantAutoLengthsValid = true;
        internal void InvalidateVariantAutoLengths()
        {
            _isVariantAutoLengthsValid = false;
        }

        internal void RefreshVariantAutoLengths()
        {
            if (_isVariantAutoLengthsValid)
                return;

            _isVariantAutoLengthsValid = true; // Avoid re-entrance
            for (int i = 1; i < BlockViews.Count; i++)
                BlockViews[i].StartMeasuredAutoLengthOffset = BlockViews[i - 1].EndMeasuredAutoLengthOffset;

            foreach (var gridTrack in VariantAutoLengthTracks)
            {
                double totalVariantAutoLength = 0;
                for (int i = 0; i < BlockViews.Count; i++)
                    totalVariantAutoLength += BlockViews[i].GetMeasuredAutoLength(gridTrack);
                gridTrack.SetTotalVariantAutoLength(totalVariantAutoLength);
            }
        }

        protected sealed override void PrepareMeasureBlocks()
        {

            RefreshScrollInfo();
        }

        private void ClearMeasuredAutoLengths()
        {
            if (VariantAutoLengthTracks.Count > 0)
            {
                for (int i = 0; i < BlockViews.Count; i++)
                    BlockViews[i].ClearMeasuredAutoLengths();
            }
        }

        private void RefreshScrollInfo(bool invalidateMeasure = false)
        {
            RefreshExtentSize();
            RefreshViewportSize();  // Sequence matters here: ViewportSize relies on ExtentSize
            RefreshScrollOffset();

            if (invalidateMeasure && ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateMeasure();
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

        private void RefreshScrollOffset()
        {
            SetScrollOffset(ToVector(TranslateRelativeOffset(MainScrollOffset), CrossScrollOffset));
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
