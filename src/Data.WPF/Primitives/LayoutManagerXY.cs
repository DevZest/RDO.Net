using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class LayoutManagerXY : LayoutManager, IScrollHandler
    {
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

        public abstract double ViewportX { get; }

        public abstract double ViewportY { get; }

        protected double ViewportMain { get; private set; }

        protected double ViewportCross { get; private set; }

        private void RefreshViewport(double valueMain, double valueCross)
        {
            if (ViewportMain.IsClose(valueMain) && ViewportCross.IsClose(valueCross))
                return;
            ViewportMain = valueMain;
            ViewportCross = valueCross;
            InvalidateScrollInfo();
        }

        public abstract double ExtentY { get; }

        public abstract double ExtentX { get; }

        protected double ExtentMain { get; private set; }

        protected double ExtentCross { get; private set; }

        private void RefreshExtent(double valueMain, double valueCross)
        {
            if (ExtentMain.IsClose(valueMain) && ExtentCross.IsClose(valueCross))
                return;
            ExtentMain = valueMain;
            ExtentCross = valueCross;
            InvalidateScrollInfo();
        }

        public abstract double OffsetX { get; set; }

        public abstract double OffsetY { get; set; }

        private double _oldOffsetMain;
        protected double OffsetMain { get; set; }

        private double _oldOffsetCross;
        protected double OffsetCross { get; set; }

        private void RefreshOffset(double valueMain, double valueCross)
        {
            _oldOffsetMain = OffsetMain;
            _oldOffsetCross = OffsetCross;
            if (OffsetMain.IsClose(valueMain) && OffsetCross.IsClose(valueCross))
                return;
            OffsetMain = valueMain;
            OffsetCross = valueCross;
            InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        #endregion

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Derived classes are limited to X or Y, and the overrides do not rely on completion of its constructor.")]
        protected LayoutManagerXY(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
            VariantAutoLengthTracks = GridTracksMain.InitVariantAutoLengthTracks();
        }

        protected abstract IGridTrackCollection GridTracksMain { get; }
        protected abstract IGridTrackCollection GridTracksCross { get; }
        internal IReadOnlyList<GridTrack> VariantAutoLengthTracks { get; private set; }

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
            get { return GridTracksMain.BlockEnd.EndOffset - GridTracksMain.BlockStart.StartOffset; }
        }

        private double AvgBlockLength
        {
            get { return FixBlockLength + AvgVariantAutoLength; }
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

            var relativeOffsetSpan = GetRelativeOffsetSpan(gridTrack, blockOrdinal);
            var startOffset = GridTracksMain[MaxFrozenHead].StartOffset + GetBlocksLength(blockOrdinal);
            return new OffsetSpan(startOffset + relativeOffsetSpan.StartOffset, startOffset + relativeOffsetSpan.EndOffset);
        }

        private OffsetSpan GetRelativeOffsetSpan(GridTrack gridTrack, int blockOrdinal)
        {
            var startTrack = GridTracksMain.BlockStart;

            var startOffset = gridTrack.StartOffset - startTrack.StartOffset;
            var endOffset = gridTrack.EndOffset - startTrack.StartOffset;

            var variantAutoLengthTrack = LastVariantAutoLengthTrack(gridTrack);
            if (variantAutoLengthTrack != null)
            {
                var variantOffsetSpan = GetVariantOffsetSpan(variantAutoLengthTrack, blockOrdinal);
                startOffset += variantOffsetSpan.StartOffset;
                endOffset += variantOffsetSpan.EndOffset;
            }
            return new OffsetSpan(startOffset, endOffset);
        }

        private OffsetSpan GetVariantOffsetSpan(GridTrack gridTrack, int blockOrdinal)
        {
            Debug.Assert(gridTrack.IsVariantAutoLength);

            double startOffset, endOffset;

            var blockView = BlockViews.GetBlockView(blockOrdinal);
            if (blockView != null)
            {
                startOffset = blockView.GetMeasuredAutoLengthStartOffset(gridTrack);
                endOffset = blockView.GetMeasuredAutoLengthEndOffset(gridTrack);
            }
            else
            {
                endOffset = 0;
                for (int i = 0; i <= gridTrack.VariantAutoLengthIndex; i++)
                    endOffset += VariantAutoLengthTracks[i].AvgVariantAutoLength;
                startOffset = endOffset - gridTrack.AvgVariantAutoLength;
            }

            return new OffsetSpan(startOffset, endOffset);
        }

        private GridTrack LastVariantAutoLengthTrack(GridTrack gridTrack)
        {
            GridTrack result = null;
            foreach (var track in VariantAutoLengthTracks)
            {
                if (track.Ordinal > gridTrack.Ordinal)
                    break;

                result = track;
            }
            return result;
        }

        private double GetBlocksLength(int count)
        {
            Debug.Assert(count >= 0 && count <= MaxBlockCount);
            if (count == 0)
                return 0;

            var unrealized = BlockViews.Count == 0 ? 0 : BlockViews.First.Ordinal;
            if (count <= unrealized)
                return count * AvgBlockLength;

            var realized = BlockViews.Count == 0 ? 0 : BlockViews.Last.Ordinal - BlockViews.First.Ordinal + 1;
            if (count <= unrealized + realized)
                return unrealized * AvgBlockLength + GetRealizedBlocksLength(count - unrealized);

            return GetRealizedBlocksLength(realized) + (count - realized) * AvgBlockLength;
        }

        private double GetRealizedBlocksLength(int count)
        {
            Debug.Assert(count >= 0 && count <= BlockViews.Count);
            return count == 0 ? 0 : count * FixBlockLength + BlockViews[count - 1].EndMeasuredAutoLengthOffset;
        }

        private GridOffset TranslateGridOffset(int gridOffset)
        {
            Debug.Assert(gridOffset >= 0 && gridOffset <= MaxGridOffset);
            if (gridOffset < MaxFrozenHead)
                return GridOffset.New(GridTracksMain[gridOffset]);

            gridOffset -= MaxFrozenHead;
            var totalBlockGridTracks = TotalBlockGridTracks;
            if (gridOffset < totalBlockGridTracks)
                return GridOffset.New(GridTracksMain[MaxFrozenHead + gridOffset % BlockGridTracks], gridOffset / BlockGridTracks);

            gridOffset -= totalBlockGridTracks;
            Debug.Assert(gridOffset < MaxFrozenTail);
            return GridOffset.New(GridTracksMain[MaxFrozenHead + BlockGridTracks + gridOffset]);
        }

        private int MaxBlockCount
        {
            get { return BlockViews.MaxBlockCount; }
        }

        private int MaxFrozenHead
        {
            get { return GridTracksMain.MaxFrozenHead; }
        }

        private int BlockGridTracks
        {
            get { return GridTracksMain.BlockEnd.Ordinal - GridTracksMain.BlockStart.Ordinal + 1; }
        }

        private int TotalBlockGridTracks
        {
            get { return MaxBlockCount * BlockGridTracks; }
        }

        private int MaxFrozenTail
        {
            get { return GridTracksMain.MaxFrozenTail; }
        }

        private int MaxGridOffset
        {
            get { return MaxFrozenHead + TotalBlockGridTracks + MaxFrozenTail; }
        }

        private Vector BlockDimensionVector
        {
            get { return GridTracksMain.BlockDimensionVector; }
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
                gridTrack.SetAvgVariantAutoLength(BlockViews.Count == 0 ? 0 : totalVariantAutoLength / BlockViews.Count);
            }
        }

        protected override void OnSetState(DataPresenterState dataPresenterState)
        {
            base.OnSetState(dataPresenterState);
            if (dataPresenterState == DataPresenterState.Rows)
            {
                BlockViews.VirtualizeAll();
                InvalidateVariantAutoLengths();
            }
        }

        internal override Size Measure(Size availableSize)
        {
            return base.Measure(availableSize);
        }

        private void UpdateMainScrollOffset()
        {

        }

        protected sealed override void PrepareMeasureBlocks()
        {

            RefreshScrollInfo();
        }

        private void RefreshScrollInfo(bool invalidateMeasure = false)
        {
            RefreshExtentSize();
            RefreshViewport();  // Sequence matters here: ViewportSize relies on ExtentSize
            RefreshOffset();

            if (invalidateMeasure && ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateMeasure();
        }

        private void RefreshExtentSize()
        {
            throw new NotImplementedException();
        }

        private void RefreshViewport()
        {
            var valueMain = GridTracksMain.SizeToContent ? ExtentMain : GridTracksMain.AvailableLength;
            var valueCross = GridTracksCross.SizeToContent ? ExtentCross : GridTracksCross.AvailableLength;
            RefreshViewport(valueMain, valueCross);
        }

        private void RefreshOffset()
        {
            throw new NotImplementedException();
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
            get { return new Size(ViewportX, ViewportY); }
        }

        internal override Rect GetArrangeRect(BlockView blockView, BlockItem blockItem)
        {
            throw new NotImplementedException();
        }
    }
}
