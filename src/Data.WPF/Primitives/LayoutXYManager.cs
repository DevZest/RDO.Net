using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract partial class LayoutXYManager : LayoutManager, IScrollHandler
    {
        private struct LogicalOffset
        {
            public LogicalOffset(double value)
            {
                _value = value;
            }

            public LogicalOffset(int gridOffset, double fractionOffset)
            {
                Debug.Assert(gridOffset >= 0);
                Debug.Assert(fractionOffset >= 0 && fractionOffset < 1);
                _value = gridOffset + fractionOffset;
            }

            private readonly double _value;
            public double Value
            {
                get { return _value; }
            }

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

            public GridOffset (GridTrack gridTrack)
            {
                Debug.Assert(!gridTrack.IsRepeat);
                GridTrack = gridTrack;
                _blockOrdinal = -1;
            }

            public GridOffset (GridTrack gridTrack, BlockView block)
            {
                Debug.Assert(gridTrack.IsRepeat && block != null);
                GridTrack = gridTrack;
                _blockOrdinal = block.Ordinal;
            }

            public GridOffset (GridTrack gridTrack, int blockOrdinal)
            {
                Debug.Assert(gridTrack.IsRepeat && blockOrdinal >= 0);
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

            public static bool operator ==(GridOffset x, GridOffset y)
            {
                return x.GridTrack == y.GridTrack && x.BlockOrdinal == y.BlockOrdinal;
            }

            public static bool operator !=(GridOffset x, GridOffset y)
            {
                return !(x == y);
            }

            public override int GetHashCode()
            {
                return IsEof ? 0 : GridTrack.GetHashCode() ^ BlockOrdinal;
            }

            public override bool Equals(object obj)
            {
                return obj is GridOffset ? (GridOffset)obj == this : false;
            }
        }

        #region IScrollHandler

        public ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        private void InvalidateMeasure()
        {
            if (ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateMeasure();
        }

        private void InvalidateArrange()
        {
            if (ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateArrange();
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

        public abstract double ScrollOffsetX { get; set; }

        public abstract double ScrollOffsetY { get; set; }

        private double _originalScrollOffsetMain;
        private double _scrollOffsetMain;
        protected double ScrollOffsetMain
        {
            get { return _scrollOffsetMain; }
            set { SetScrollOffsetMain(value, true); }
        }

        private void SetScrollOffsetMain(double value, bool invalidateMeasure)
        {
            if (_scrollOffsetMain.IsClose(value))
                return;
            _scrollOffsetMain = value;
            if (invalidateMeasure)
                InvalidateMeasure();
        }

        private double _scrollOffsetCross;
        protected double ScrollOffsetCross
        {
            get { return _scrollOffsetCross; }
            set { SetScrollOffsetCross(value, true); }
        }

        private void SetScrollOffsetCross(double value, bool invalidateArrange)
        {
            if (_scrollOffsetMain.IsClose(value))
                return;
            _scrollOffsetMain = value;
            if (invalidateArrange)
                InvalidateArrange();
        }

        private void RefreshScollOffset(double valueMain, double valueCross)
        {
            bool invalidateScrollInfo = !ScrollOffsetMain.IsClose(valueMain) || !ScrollOffsetCross.IsClose(valueCross);
            _originalScrollOffsetMain = valueMain;
            SetScrollOffsetMain(valueMain, false);
            SetScrollOffsetCross(valueCross, false);
            if (invalidateScrollInfo)
                InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        #endregion

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Derived classes are limited to class LayoutXManager/LayoutYManager, and the overrides do not rely on completion of its constructor.")]
        protected LayoutXYManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
            VariantAutoLengthTracks = GridTracksMain.InitVariantAutoLengthTracks();
            _scrollStartMain = ScrollOriginMain;
        }

        protected abstract IGridTrackCollection GridTracksMain { get; }
        protected abstract IGridTrackCollection GridTracksCross { get; }
        internal IReadOnlyList<GridTrack> VariantAutoLengthTracks { get; private set; }

        private bool _isBlocksDirty;
        private void InvalidateBlocks()
        {
            if (_isBlocksDirty)
                return;

            for (int i = 0; i < BlockViews.Count; i++)
                BlockViews[i].ClearElements();
            _isBlocksDirty = true;

            InvalidateMeasure();
        }

        private void InitBlocks()
        {
            BlockViews.VirtualizeAll();

            var initialBlockOrdinal = GetInitialBlockOrdinal();
            if (initialBlockOrdinal >= 0)
            {
                BlockViews.RealizeFirst(initialBlockOrdinal);
                BlockViews[0].Measure(Size.Empty);
            }

            _isBlocksDirty = false;
        }

        private int GetInitialBlockOrdinal()
        {
            if (MaxBlockCount == 0)
                return -1;

            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            if (gridOffset.IsEof)
                return MaxBlockCount - 1;

            var gridTrack = gridOffset.GridTrack;
            if (gridTrack.IsHead)
                return 0;
            else if (gridTrack.IsRepeat)
                return gridOffset.BlockOrdinal;
            else
                return MaxBlockCount - 1;
        }

        private LogicalOffset ScrollOriginMain
        {
            get { return new LogicalOffset(FrozenHead); }
        }

        private LogicalOffset _scrollStartMain;
        private double ScrollStartMain
        {
            get { return GetOffset(_scrollStartMain); }
        }

        private void ScrollBackward(double scrollLength)
        {
            _scrollStartMain = GetLogicalOffset(GetOffset(_scrollStartMain) - scrollLength);
            Debug.Assert(_scrollStartMain.Value >= ScrollOriginMain.Value);
        }

        private double ScrollOriginCross
        {
            get { return GridTracksCross[GridTracksCross.FrozenHead].StartOffset; }
        }

        private double ScrollStartCross
        {
            get { return ScrollOriginCross + ScrollOffsetCross; }
        }

        private double DeltaScrollOffset
        {
            get { return _scrollOffsetMain - _originalScrollOffsetMain; }
        }

        private void ClearDeltaScrollOffset()
        {
            _originalScrollOffsetMain = _scrollOffsetMain;
        }

        private Vector ToVector(double valueMain, double valueCross)
        {
            return GridTracksMain.ToVector(valueMain, valueCross);
        }

        private Size ToSize(double valueMain, double valueCross)
        {
            var vector = ToVector(valueMain, valueCross);
            return new Size(vector.X, vector.Y);
        }

        private Point ToPoint(double valueMain, double valueCross)
        {
            var vector = ToVector(valueMain, valueCross);
            return new Point(vector.X, vector.Y);
        }

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

        private double GetOffset(LogicalOffset logicalOffset)
        {
            return GetOffset(logicalOffset.GridOffset, logicalOffset.FractionOffset);
        }

        private double GetOffset(GridOffset gridOffset, double fraction)
        {
            var span = GetSpan(gridOffset);
            return span.StartOffset + span.Length * fraction;
        }

        private double GetOffset(int gridOffset, double fraction)
        {
            if (gridOffset >= MaxGridOffset)
                return GetSpan(MaxGridOffset - 1).EndOffset;
            else
            {
                var span = GetSpan(gridOffset);
                return span.StartOffset + span.Length * fraction;
            }
        }

        private LogicalOffset GetLogicalOffset(double offset)
        {
            // Binary search
            var min = 0;
            var max = MaxGridOffset - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                var offsetSpan = GetSpan(mid);
                if (offset < offsetSpan.StartOffset)
                    max = mid - 1;
                else if (offset >= offsetSpan.EndOffset)
                    min = mid + 1;
                else
                    return new LogicalOffset(mid, (offset - offsetSpan.StartOffset) / offsetSpan.Length);
            }

            return new LogicalOffset(MaxGridOffset);
        }

        private Span GetSpan(int gridOffset)
        {
            Debug.Assert(gridOffset >= 0 && gridOffset < MaxGridOffset);
            return GetSpan(GetGridOffset(gridOffset));
        }

        private Span GetSpan(GridOffset gridOffset)
        {
            Debug.Assert(!gridOffset.IsEof);

            var gridTrack = gridOffset.GridTrack;
            if (gridTrack.IsHead)
                return new Span(gridTrack.StartOffset, gridTrack.EndOffset);

            if (gridTrack.IsRepeat)
                return GetSpan(gridTrack, gridOffset.BlockOrdinal);

            Debug.Assert(gridTrack.IsTail);
            var delta = MaxBlockCount * AvgBlockLength;
            if (MaxBlockCount > 0)
                delta -= FixBlockLength;    // minus duplicated FixBlockLength
            return new Span(gridTrack.StartOffset + delta, gridTrack.EndOffset + delta);
        }

        private Span GetSpan(GridTrack gridTrack, int blockOrdinal)
        {
            Debug.Assert(gridTrack.IsRepeat && blockOrdinal >= 0);

            var relativeSpan = GetRelativeSpan(gridTrack, blockOrdinal);
            var startOffset = GridTracksMain[MaxFrozenHead].StartOffset + GetBlocksLength(blockOrdinal);
            return new Span(startOffset + relativeSpan.StartOffset, startOffset + relativeSpan.EndOffset);
        }

        private Span GetRelativeSpan(BlockView block, GridTrack gridTrack)
        {
            Debug.Assert(block != null && gridTrack.IsRepeat);
            return GetRelativeSpan(gridTrack, block.Ordinal);
        }

        private Span GetRelativeSpan(GridTrack gridTrack, int blockOrdinal)
        {
            Debug.Assert(gridTrack.IsRepeat && blockOrdinal >= 0);
            var startTrack = GridTracksMain.BlockStart;

            var startOffset = gridTrack.StartOffset - startTrack.StartOffset;
            var endOffset = gridTrack.EndOffset - startTrack.StartOffset;

            var variantAutoLengthTrack = LastVariantAutoLengthTrack(gridTrack);
            if (variantAutoLengthTrack != null)
            {
                var variantLengthSpan = GetVariantLengthSpan(variantAutoLengthTrack, blockOrdinal);
                startOffset += variantLengthSpan.StartOffset;
                endOffset += variantLengthSpan.EndOffset;
            }
            return new Span(startOffset, endOffset);
        }

        private Span GetVariantLengthSpan(GridTrack gridTrack, int blockOrdinal)
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

            return new Span(startOffset, endOffset);
        }

        private GridTrack LastVariantAutoLengthTrack(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.IsRepeat);

            // optimization: shortcuts for common cases
            if (VariantAutoLengthTracks.Count == 0)
                return null;
            if (gridTrack == GridTracksMain.BlockStart)
                return gridTrack.IsVariantAutoLength ? gridTrack : null;
            if (gridTrack == GridTracksMain.BlockEnd)
                return VariantAutoLengthTracks.LastOf(1);

            GridTrack result = null;
            for (int i = 0; i < VariantAutoLengthTracks.Count; i++)
            {
                var variantAuotLengthTrack = VariantAutoLengthTracks[i];
                if (variantAuotLengthTrack.Ordinal > gridTrack.Ordinal)
                    break;

                result = variantAuotLengthTrack;
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

        private GridOffset GetGridOffset(int gridOffset)
        {
            Debug.Assert(gridOffset >= 0);

            if (gridOffset >= MaxGridOffset)
                return GridOffset.Eof;

            if (gridOffset < MaxFrozenHead)
                return new GridOffset(GridTracksMain[gridOffset]);

            gridOffset -= MaxFrozenHead;
            var totalBlockGridTracks = TotalBlockGridTracks;
            if (gridOffset < totalBlockGridTracks)
                return new GridOffset(GridTracksMain[MaxFrozenHead + gridOffset % BlockGridTracks], gridOffset / BlockGridTracks);

            gridOffset -= totalBlockGridTracks;
            Debug.Assert(gridOffset < MaxFrozenTail);
            return new GridOffset(GridTracksMain[MaxFrozenHead + BlockGridTracks + gridOffset]);
        }

        private int MaxBlockCount
        {
            get { return BlockViews.MaxBlockCount; }
        }

        private int FrozenHead
        {
            get { return GridTracksMain.FrozenHead; }
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

        private int FrozenTail
        {
            get { return GridTracksMain.FrozenTail; }
        }

        private int MaxFrozenTail
        {
            get { return GridTracksMain.MaxFrozenTail; }
        }

        private int MaxGridOffset
        {
            get { return MaxFrozenHead + TotalBlockGridTracks + MaxFrozenTail; }
        }

        private double MaxOffsetMain
        {
            get { return GetSpan(MaxGridOffset - 1).EndOffset; }
        }

        private double MaxOffsetCross
        {
            get { return GridTracksCross.GetMeasuredLength(Template.Range()); }
        }

        private Vector BlockDimensionVector
        {
            get { return GridTracksMain.BlockDimensionVector; }
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
                InvalidateBlocks();
        }

        internal override Size Measure(Size availableSize)
        {
            InitScroll();
            return base.Measure(availableSize);
        }

        private void InitScroll()
        {
            if (DeltaScrollOffset < 0 && Math.Abs(DeltaScrollOffset) < ViewportMain)
                return;

            ScrollBackward(-DeltaScrollOffset);
            ClearDeltaScrollOffset();
        }

        protected sealed override void PrepareMeasureBlocks()
        {
            InitBlocks();

            if (DeltaScrollOffset < 0 || _scrollStartMain.GridOffset >= MaxGridOffset)
                MeasureBackward(-DeltaScrollOffset);
            else
                MeasureForward(GridTracksMain.AvailableLength);
            FillGap();
            RefreshScrollInfo();
        }

        private double FrozenHeadLength
        {
            get { return GridTracksMain[FrozenHead].StartOffset; }
        }

        private double FrozenTailStart
        {
            get { return FrozenTail == 0 ? MaxOffsetMain : GetSpan(MaxGridOffset - FrozenTail).StartOffset; }
        }

        private double TailLength
        {
            get { return MaxFrozenTail == 0 ? 0 : GridTracksMain.LastOf(1).EndOffset - GridTracksMain.LastOf(MaxFrozenTail).StartOffset; }
        }

        private double Gap
        {
            get
            {
                var availableLength = GridTracksMain.AvailableLength;
                if (double.IsPositiveInfinity(availableLength))
                    return availableLength;

                var scrollable = availableLength - (FrozenHeadLength + TailLength);
                var blockEndOffset = BlockViews.Count == 0 ? GridTracksMain[MaxFrozenHead].StartOffset : GetEndOffset(BlockViews[BlockViews.Count - 1]);
                return scrollable - (blockEndOffset - ScrollStartMain);
            }
        }

        private void FillGap()
        {
            var gap = Gap;
            gap -= RealizeForward(gap);
            if (gap > 0)
                MeasureBackward(gap);
        }

        private double MeasureBackward(double scrollDelta)
        {
            Debug.Assert(scrollDelta > 0);

            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            if (gridOffset.IsEof)
                return MeasureBackwardEof(scrollDelta);

            var gridTrack = gridOffset.GridTrack;
            if (gridTrack.IsTail)
                return MeasureBackwardTail(scrollDelta);
            else if (gridTrack.IsRepeat)
                return MeasureBackwardRepeat(scrollDelta);
            else
            {
                Debug.Assert(gridTrack.IsHead);
                return MeasureBackwardHead(scrollDelta);
            }
        }

        private double MeasureBackwardEof(double scrollDelta)
        {
            if (MaxFrozenTail > 0)
                return MeasureBackwardTail(scrollDelta);
            else if (MaxBlockCount > 0)
                return MeasureBackwardRepeat(scrollDelta);
            else if (MaxFrozenHead > 0)
                return MeasureBackwardHead(scrollDelta);
            return scrollDelta;
        }

        private double MeasureBackwardTail(double scrollDelta)
        {
            var scrollLength = Math.Min(scrollDelta, ScrollStartMain - FrozenTailStart);
            Debug.Assert(scrollLength >= 0);
            if (scrollLength > 0)
            {
                ScrollBackward(scrollLength);
                scrollDelta -= scrollLength;
            }
            if (scrollDelta == 0)
                return scrollDelta;

            if (MaxBlockCount > 0)
                return MeasureBackwardRepeat(scrollDelta);
            else if (MaxFrozenHead > 0)
                return MeasureBackwardHead(scrollDelta);
            return scrollDelta;
        }

        private double MeasureBackwardRepeat(double scrollDelta)
        {
            Debug.Assert(scrollDelta > 0);

            var block = BlockViews[0];
            var scrollLength = Math.Min(scrollDelta, ScrollStartMain - GetStartOffset(block));
            if (scrollLength > 0)
            {
                ScrollBackward(scrollLength);
                scrollDelta -= scrollLength;
            }
            scrollDelta = RealizeBackward(scrollDelta - scrollLength);
            return scrollDelta > 0 && FrozenHead > 0 ? MeasureBackwardHead(scrollDelta) : scrollDelta;
        }

        private double RealizeBackward(double scrollDelta)
        {
            Debug.Assert(BlockViews.Count > 0);

            while (scrollDelta > 0)
            {
                for (int blockOrdinal = BlockViews.Last.Ordinal + 1; blockOrdinal >= 0; blockOrdinal--)
                {
                    BlockViews.RealizePrev();
                    var block = BlockViews.First;
                    block.Measure(Size.Empty);
                    var scrollLength = Math.Min(scrollDelta, GetLength(block));
                    ScrollBackward(scrollLength);
                    scrollDelta -= scrollLength;
                }
            }
            return scrollDelta;
        }

        private double MeasureBackwardHead(double scrollDelta)
        {
            var scrollLength = Math.Min(scrollDelta, ScrollStartMain - FrozenHeadLength);
            if (scrollLength <= 0)
                return scrollDelta;

            ScrollBackward(scrollLength);
            return scrollDelta - scrollLength;
        }

        private void MeasureForward(double availableLength)
        {
            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            Debug.Assert(!gridOffset.IsEof);
            var gridTrack = gridOffset.GridTrack;
            var fraction = _scrollStartMain.FractionOffset;
            if (gridTrack.IsHead)
                MeasureForwardHead(gridTrack, fraction, availableLength);
            else if (gridTrack.IsRepeat)
                MeasureForwardRepeat(gridOffset, fraction, availableLength);
        }

        private double MeasureForwardHead(GridTrack gridTrack, double fraction, double availableLength)
        {
            Debug.Assert(gridTrack.IsHead);
            var measuredLength = FrozenHeadLength - (gridTrack.StartOffset + gridTrack.MeasuredLength * fraction);
            if (MaxBlockCount == 0)
                return measuredLength;

            availableLength -= measuredLength;
            return measuredLength + MeasureForwardRepeat(new GridOffset(GridTracksMain.BlockStart, 0), 0, availableLength);
        }

        private double MeasureForwardRepeat(GridOffset gridOffset, double fraction, double availableLength)
        {
            Debug.Assert(BlockViews.Count == 1);
            double result = 0;
            if (MaxFrozenTail > 0)
            {
                result = MeasureForwardTail(GridTracksMain.LastOf(MaxFrozenTail), 0);
                availableLength -= result;
            }

            var gridTrack = gridOffset.GridTrack;
            Debug.Assert(gridTrack.IsRepeat);
            var block = BlockViews[0];
            Debug.Assert(block.Ordinal == gridOffset.BlockOrdinal);
            var measuredLength = GetLength(block) - GetRelativeOffset(block, gridTrack, fraction);
            result += measuredLength;
            availableLength -= measuredLength;

            return result + RealizeForward(availableLength);
        }

        private double GetRelativeOffset(BlockView block, GridTrack gridTrack, double fraction)
        {
            return GetRelativeSpan(block, gridTrack).StartOffset + GetMeasuredLength(block, gridTrack) * fraction;
        }

        private double MeasureForwardTail(GridTrack gridTrack, double fraction)
        {
            Debug.Assert(gridTrack.IsTail);
            return GridTracksMain.LastOf(1).EndOffset - (gridTrack.StartOffset + gridTrack.MeasuredLength * fraction);
        }

        private double RealizeForward(double availableLength)
        {
            Debug.Assert(BlockViews.Last != null);

            double result = 0;

            while (availableLength > 0)
            {
                for (int blockOrdinal = BlockViews.Last.Ordinal + 1; blockOrdinal < MaxBlockCount; blockOrdinal++)
                {
                    BlockViews.RealizeNext();
                    var block = BlockViews.Last;
                    block.Measure(Size.Empty);
                    var measuredLength = GetLength(block);
                    result += measuredLength;
                    availableLength -= measuredLength;
                }
            }
            return result;
        }

        private void RefreshScrollInfo()
        {
            RefreshViewport();
            RefreshExtent();  // Exec order matters: RefreshExtent relies on RefreshViewport
            RefreshScrollOffset();  // Exec order matters: RefreshScrollOffset relies on RefreshViewport and RefreshExtent
        }

        private void RefreshExtent()
        {
            var valueMain = MaxOffsetMain;
            if (valueMain < ViewportMain)
                valueMain = ViewportMain;
            var valueCross = MaxOffsetCross;
            if (valueCross < ViewportCross)
                valueCross = ViewportCross;
            RefreshExtent(valueMain, valueCross);
        }

        private void RefreshViewport()
        {
            var valueMain = GridTracksMain.SizeToContent ? MaxOffsetMain : GridTracksMain.AvailableLength;
            var valueCross = GridTracksCross.SizeToContent ? MaxOffsetCross : GridTracksCross.AvailableLength;
            RefreshViewport(valueMain, valueCross);
        }

        private void RefreshScrollOffset()
        {
            var scrollOriginMain = GetOffset(ScrollOriginMain);
            var scrollStartMain = GetOffset(_scrollStartMain);
            Debug.Assert(scrollStartMain >= scrollOriginMain);
            var valueMain = scrollStartMain - scrollOriginMain;
            var valueCross = CoerceScrollOffsetCross();
            RefreshScollOffset(valueMain, valueCross);
        }

        private double CoerceScrollOffsetCross()
        {
            var result = ScrollOffsetCross;
            if (result < 0)
                result = 0;
            if (result > ExtentCross - ViewportCross)
                result = ExtentCross - ViewportCross;
            return result;
        }

        protected sealed override Size GetMeasuredSize(ScalarItem scalarItem)
        {
            var gridRange = scalarItem.GridRange;
            var valueMain = GetLength(gridRange);
            var valueCross = GridTracksCross.GetMeasuredLength(gridRange);

            var result = ToSize(valueMain, valueCross);
            if (!scalarItem.IsMultidimensional && BlockDimensions > 1)
            {
                var delta = BlockDimensionVector * (BlockDimensions - 1);
                result = new Size(result.Width + delta.X, result.Height + delta.Y);
            }
            return result;
        }

        private double GetLength(GridRange gridRange)
        {
            var startGridOffset = GetStartGridOffset(gridRange);
            var endGridOffset = GetEndGridOffset(gridRange);
            return startGridOffset == endGridOffset ? GetSpan(startGridOffset).Length : GetSpan(endGridOffset).EndOffset - GetSpan(startGridOffset).StartOffset;
        }

        protected override Size GetMeasuredSize(BlockView block)
        {
            var result = GetMeasuredSize(block, Template.BlockRange, true);
            if (BlockDimensions > 1)
            {
                var vector = BlockDimensionVector * (BlockDimensions - 1);
                result = new Size(result.Width + vector.X, result.Height + vector.Y);
            }
            return result;
        }

        protected override Size GetMeasuredSize(BlockView block, GridRange gridRange, bool clipScrollCross)
        {
            Debug.Assert(!gridRange.IsEmpty && Template.BlockRange.Contains(gridRange));

            var valueMain = GetLength(block, gridRange);
            var valueCross = GridTracksCross.GetMeasuredLength(gridRange);
            if (clipScrollCross)
                valueCross -= GetScrollCrossClip(gridRange);
            return ToSize(valueMain, valueCross);
        }

        private double GetLength(BlockView block)
        {
            return GetLength(block, Template.BlockRange);
        }

        private double GetLength(BlockView block, GridRange gridRange)
        {
            var gridSpan = GridTracksMain.GetGridSpan(gridRange);
            var startTrack = gridSpan.StartTrack;
            var endTrack = gridSpan.EndTrack;
            return startTrack == endTrack ? GetRelativeSpan(block, startTrack).Length
                : GetRelativeSpan(block, endTrack).EndOffset - GetRelativeSpan(block, startTrack).StartOffset;
        }

        private double GetScrollCrossClip(GridRange gridRange)
        {
            var span = GridTracksCross.GetGridSpan(gridRange);
            var startOffset = span.StartTrack.StartOffset;
            if (ScrollOriginCross <= startOffset)
                return 0;
            var endOffset = span.EndTrack.EndOffset;
            var scrollStart = ScrollStartCross;
            return scrollStart > startOffset && scrollStart < endOffset ? scrollStart - Math.Max(startOffset, ScrollOriginCross) : 0;
        }

        protected override Point GetScalarItemLocation(ScalarItem scalarItem, int blockDimension)
        {
            var gridRange = scalarItem.GridRange;
            var startGridOffset = GetStartGridOffset(gridRange);
            var valueMain = startGridOffset.IsEof ? MaxOffsetMain : GetSpan(startGridOffset).StartOffset;
            var valueCross = GridTracksCross.GetGridSpan(gridRange).StartTrack.StartOffset;
            var result = ToPoint(valueMain, valueCross);
            if (blockDimension > 0)
                result += blockDimension * BlockDimensionVector;
            result.Offset(-ScrollOffsetX, -ScrollOffsetY);
            return result;
        }

        private GridOffset GetStartGridOffset(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).StartTrack;
            if (!gridTrack.IsRepeat)
                return new GridOffset(gridTrack);

            if (MaxBlockCount > 0)
                return new GridOffset(gridTrack, 0);
            else
                return MaxFrozenTail > 0 ? new GridOffset(GridTracksMain.LastOf(MaxFrozenTail)) : GridOffset.Eof;
        }

        private GridOffset GetEndGridOffset(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).EndTrack;
            if (!gridTrack.IsRepeat)
                return new GridOffset(gridTrack);

            if (MaxBlockCount > 0)
                return new GridOffset(gridTrack, MaxBlockCount - 1);
            else
                return MaxFrozenTail > 0 ? new GridOffset(GridTracksMain.LastOf(MaxFrozenTail)) : GridOffset.Eof;
        }

        protected override Point GetBlockLocation(BlockView block)
        {
            var valueMain = GetStartOffset(block);
            var valueCross = GridTracksCross.BlockStart.StartOffset;
            var result = ToPoint(valueMain, valueCross);
            result.Offset(-ScrollOffsetX, -ScrollOffsetY);
            return result;
        }

        private double GetStartOffset(BlockView block)
        {
            return GetSpan(new GridOffset(GridTracksMain.BlockStart, block)).StartOffset;
        }

        private double GetEndOffset(BlockView block)
        {
            return GetSpan(new GridOffset(GridTracksMain.BlockEnd, block)).EndOffset;
        }

        private Point GetRelativeLocation(BlockView block, GridRange gridRange)
        {
            Debug.Assert(Template.BlockRange.Contains(gridRange));

            var startTrackMain = GridTracksMain.GetGridSpan(gridRange).StartTrack;
            var valueMain = GetRelativeSpan(block, startTrackMain).StartOffset;
            var valueCross = GridTracksCross.GetGridSpan(gridRange).StartTrack.StartOffset - GridTracksCross.BlockStart.StartOffset;
            return ToPoint(valueMain, valueCross);
        }

        protected override Point GetBlockItemLocation(BlockView block, BlockItem blockItem)
        {
            return GetRelativeLocation(block, blockItem.GridRange);
        }

        protected override Point GetRowLocation(BlockView block, int blockDimension)
        {
            var result = GetRelativeLocation(block, Template.RowRange);
            if (blockDimension > 0)
                result += blockDimension * BlockDimensionVector;
            return result;
        }

        protected override Point GetRowItemLocation(BlockView block, RowItem rowItem)
        {
            var rowLocation = GetRelativeLocation(block, Template.RowRange);
            var result = GetRelativeLocation(block, rowItem.GridRange);
            result.Offset(-rowLocation.X, -rowLocation.Y);
            return result;
        }

        protected sealed override Size MeasuredSize
        {
            get { return new Size(ViewportX, ViewportY); }
        }
    }
}
