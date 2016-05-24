using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract partial class LayoutXYManager : LayoutManager, IScrollHandler
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Derived classes are limited to class LayoutXManager/LayoutYManager, and the overrides do not rely on completion of its constructor.")]
        protected LayoutXYManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
            _scrollStartMain = ScrollOriginMain;
        }

        internal abstract IGridTrackCollection GridTracksMain { get; }
        internal abstract IGridTrackCollection GridTracksCross { get; }

        internal GridSpan VariantByBlockGridSpan
        {
            get { return GridTracksMain.VariantByBlock ? GridTracksMain.GetGridSpan(Template.RowRange) : new GridSpan(); }
        }

        protected override void DisposeRow(RowPresenter row)
        {
            var gridSpan = VariantByBlockGridSpan;
            for (int i = 0; i < gridSpan.Count; i++)
                gridSpan[i].ClearAvailableLength(row);
            base.DisposeRow(row);
        }

        private bool _isBlockLengthsValid = true;
        internal void InvalidateBlockLengths()
        {
            _isBlockLengthsValid = false;
        }

        internal void RefreshBlockLengths()
        {
            if (_isBlockLengthsValid)
                return;

            _isBlockLengthsValid = true; // Avoid re-entrance
            for (int i = 1; i < BlockViews.Count; i++)
                BlockViews[i].StartOffset = BlockViews[i - 1].EndOffset;

            var gridSpan = VariantByBlockGridSpan;
            if (gridSpan.IsEmpty)
                return;

            for (int i = 0; i < gridSpan.Count; i++)
            {
                var gridTrack = gridSpan[i];
                double totalLength = 0;
                for (int j = 0; j < BlockViews.Count; j++)
                    totalLength += BlockViews[j].GetMeasuredLength(gridTrack);
                gridTrack.VariantByBlockAvgLength = BlockViews.Count == 0 ? 1 : totalLength / BlockViews.Count;
            }

            for (int i = 1; i < gridSpan.Count; i++)
                gridSpan[i].VariantByBlockStartOffset = gridSpan[i - 1].VariantByBlockEndOffset;
        }

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
            get { return new LogicalOffset(FrozenHeadMain); }
        }

        private LogicalOffset _scrollStartMain;
        private double ScrollStartMain
        {
            get { return GetOffset(_scrollStartMain); }
        }

        private void AdjustScrollStartMain(double delta)
        {
            _scrollStartMain = GetLogicalOffset(ScrollStartMain + delta);
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
            get { return _scrollOffsetMain - _oldScrollOffsetMain; }
        }

        private void ClearDeltaScrollOffset()
        {
            _oldScrollOffsetMain = _scrollOffsetMain;
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

        private Thickness ToThickness(Clip clipMain, Clip clipCross)
        {
            var vectorHead = ToVector(clipMain.Head, clipCross.Head);
            var vectorTail = ToVector(clipMain.Tail, clipCross.Tail);
            return new Thickness(vectorHead.X, vectorHead.Y, vectorTail.X, vectorTail.Y);
        }

        private int MaxBlockCount
        {
            get { return BlockViews.MaxBlockCount; }
        }

        private int FrozenHeadMain
        {
            get { return GridTracksMain.FrozenHead; }
        }

        private int FrozenHeadCross
        {
            get { return GridTracksCross.FrozenHead; }
        }

        private int MaxFrozenHeadMain
        {
            get { return GridTracksMain.MaxFrozenHead; }
        }

        private int BlockGridTracksMain
        {
            get { return GridTracksMain.BlockEnd.Ordinal - GridTracksMain.BlockStart.Ordinal + 1; }
        }

        private int TotalBlockGridTracksMain
        {
            get { return MaxBlockCount * BlockGridTracksMain; }
        }

        private int FrozenTailMain
        {
            get { return GridTracksMain.FrozenTail; }
        }

        private int FrozenTailCross
        {
            get { return GridTracksCross.FrozenTail; }
        }

        private int MaxFrozenTailMain
        {
            get { return GridTracksMain.MaxFrozenTail; }
        }

        private int MaxGridOffsetMain
        {
            get { return MaxFrozenHeadMain + TotalBlockGridTracksMain + MaxFrozenTailMain; }
        }

        private double MaxOffsetMain
        {
            get { return GetGridOffset(MaxGridOffsetMain - 1).Span.EndOffset; }
        }

        private double MaxOffsetCross
        {
            get
            {
                var result = GridTracksCross.GetMeasuredLength(Template.Range());
                if (BlockDimensions > 1)
                    result += BlockDimensionLength * (BlockDimensions - 1);
                return result;
            }
        }

        private double BlockDimensionLength
        {
            get { return GridTracksCross.GetMeasuredLength(Template.RowRange); }
        }

        private Vector BlockDimensionVector
        {
            get { return ToVector(0, BlockDimensionLength); }
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
            if (DeltaScrollOffset == 0 || (DeltaScrollOffset < 0 && Math.Abs(DeltaScrollOffset) <= ViewportMain))
                return;

            AdjustScrollStartMain(DeltaScrollOffset);
            ClearDeltaScrollOffset();
        }

        protected sealed override void PrepareMeasureBlocks()
        {
            InitBlocks();

            if (DeltaScrollOffset < 0 || _scrollStartMain.GridOffset >= MaxGridOffsetMain)
                MeasureBackward(-DeltaScrollOffset);
            else
                MeasureForward(GridTracksMain.AvailableLength);
            FillGap();
            RefreshScrollInfo();
        }

        private double HeadEnd
        {
            get { return GridTracksMain[MaxFrozenHeadMain].StartOffset; }
        }

        private double TailStart
        {
            get { return MaxFrozenTailMain == 0 ? MaxOffsetMain : GetGridOffset(MaxGridOffsetMain - MaxFrozenTailMain).Span.StartOffset; }
        }

        private double FrozenHeadLengthMain
        {
            get { return GridTracksMain[FrozenHeadMain].StartOffset; }
        }

        private double FrozenHeadLengthCross
        {
            get { return GridTracksCross[FrozenHeadCross].StartOffset; }
        }

        private double FrozenTailLengthMain
        {
            get
            {
                if (FrozenTailMain == 0)
                    return 0;

                var totalTracks = GridTracksMain.Count;
                var endOffset = GridTracksMain[totalTracks - 1].EndOffset;
                var startOffset = GridTracksMain[totalTracks - FrozenTailMain].StartOffset;
                return endOffset - startOffset;
            }
        }

        private double FrozenTailLengthCross
        {
            get { return GetTailLengthCross(FrozenTailCross); }
            //{
            //    if (FrozenTailCross == 0)
            //        return 0;

            //    var totalTracks = GridTracksCross.Count;
            //    var endOffset = GridTracksCross[totalTracks - 1].EndOffset;
            //    var startOffset = GridTracksCross[totalTracks - FrozenTailCross].StartOffset;
            //    return endOffset - startOffset;
            //}
        }

        private double GetTailLengthCross(int tracksCount)
        {
            if (tracksCount == 0)
                return 0;

            var totalTracks = GridTracksCross.Count;
            var endOffset = GridTracksCross[totalTracks - 1].EndOffset;
            var startOffset = GridTracksCross[totalTracks - tracksCount].StartOffset;
            return endOffset - startOffset;
        }

        private double TailLength
        {
            get { return MaxFrozenTailMain == 0 ? 0 : MaxOffsetMain - GetGridOffset(MaxGridOffsetMain - MaxFrozenTailMain).Span.StartOffset; }
        }

        private double Gap
        {
            get
            {
                var availableLength = GridTracksMain.AvailableLength;
                if (double.IsPositiveInfinity(availableLength))
                    return availableLength;

                var scrollable = availableLength - (FrozenHeadLengthMain + TailLength);
                var blockEndOffset = BlockViews.Count == 0 ? GridTracksMain[MaxFrozenHeadMain].StartOffset : GetEndOffset(BlockViews[BlockViews.Count - 1]);
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

        private void MeasureBackward(double availableLength)
        {
            Debug.Assert(availableLength >= 0);

            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            if (gridOffset.IsEof)
            {
                MeasureBackwardEof(availableLength);
                return;
            }
                

            var gridTrack = gridOffset.GridTrack;
            if (gridTrack.IsTail)
                MeasureBackwardTail(availableLength);
            else if (gridTrack.IsRepeat)
                MeasureBackwardRepeat(availableLength);
            else
            {
                Debug.Assert(gridTrack.IsHead);
                MeasureBackwardHead(availableLength);
            }
        }

        private void MeasureBackwardEof(double availableLength)
        {
            if (MaxFrozenTailMain > 0)
                MeasureBackwardTail(availableLength);
            else if (MaxBlockCount > 0)
                MeasureBackwardRepeat(availableLength);
            else if (MaxFrozenHeadMain > 0)
                MeasureBackwardHead(availableLength);
        }

        private void MeasureBackwardTail(double availableLength)
        {
            var measuredLength = Math.Min(availableLength, ScrollStartMain - TailStart);
            Debug.Assert(measuredLength >= 0);
            if (measuredLength > 0)
            {
                AdjustScrollStartMain(-measuredLength);
                availableLength -= measuredLength;
            }
            if (availableLength == 0)
                return;

            if (MaxBlockCount > 0)
                MeasureBackwardRepeat(availableLength);
            else if (MaxFrozenHeadMain > 0)
                MeasureBackwardHead(availableLength);
        }

        private void MeasureBackwardRepeat(double availableLength)
        {
            Debug.Assert(availableLength > 0);

            var block = BlockViews[0];
            var scrollLength = Math.Min(availableLength, ScrollStartMain - GetStartOffset(block));
            if (scrollLength > 0)
            {
                AdjustScrollStartMain(-scrollLength);
                availableLength -= scrollLength;
            }
            availableLength -= RealizeBackward(availableLength);
            if (availableLength > 0 && FrozenHeadMain > 0)
                MeasureBackwardHead(availableLength);
        }

        private double RealizeBackward(double availableLength)
        {
            if (BlockViews.Count == 0)
                return availableLength;

            Debug.Assert(BlockViews.Count > 0);

            for (int blockOrdinal = BlockViews.First.Ordinal - 1; blockOrdinal >= 0 && availableLength > 0; blockOrdinal--)
            {
                BlockViews.RealizePrev();
                var block = BlockViews.First;
                block.Measure(Size.Empty);
                var measuredLength = Math.Min(availableLength, GetBlockLengthMain(block));
                AdjustScrollStartMain(-measuredLength);
                availableLength -= measuredLength;
            }
            return availableLength;
        }

        private void MeasureBackwardHead(double availableLength)
        {
            Debug.Assert(availableLength >= 0);
            var measuredLength = Math.Min(availableLength, ScrollOffsetMain);
            if (measuredLength > 0)
               AdjustScrollStartMain(-measuredLength);
        }

        private void MeasureForward(double availableLength)
        {
            availableLength -= Math.Max(FrozenHeadLengthMain, HeadEnd - ScrollOffsetMain);
            if (availableLength <= 0)
                return;

            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            Debug.Assert(!gridOffset.IsEof);
            var gridTrack = gridOffset.GridTrack;
            var fraction = _scrollStartMain.FractionOffset;
            if (gridTrack.IsHead)
                MeasureForwardHead(gridTrack, fraction, availableLength);
            else if (gridTrack.IsRepeat)
                MeasureForwardRepeat(gridOffset, fraction, availableLength);
        }

        private void MeasureForwardHead(GridTrack gridTrack, double fraction, double availableLength)
        {
            Debug.Assert(gridTrack.IsHead);
            var measuredLength = HeadEnd - (gridTrack.StartOffset + gridTrack.MeasuredLength * fraction);
            Debug.Assert(measuredLength > 0);
            if (MaxBlockCount > 0)
                MeasureForwardRepeat(new GridOffset(GridTracksMain.BlockStart, 0), 0, availableLength - measuredLength);
        }

        private void MeasureForwardRepeat(GridOffset gridOffset, double fraction, double availableLength)
        {
            Debug.Assert(BlockViews.Count == 1);
            if (FrozenTailMain > 0)
                availableLength -= FrozenTailLengthMain;

            var gridTrack = gridOffset.GridTrack;
            Debug.Assert(gridTrack.IsRepeat);
            var block = BlockViews[0];
            Debug.Assert(block.Ordinal == gridOffset.BlockOrdinal);
            availableLength -= GetBlockLengthMain(block) - GetRelativeOffset(block, gridTrack, fraction);
            RealizeForward(availableLength);
        }

        private double GetRelativeOffset(BlockView block, GridTrack gridTrack, double fraction)
        {
            return gridTrack.GetRelativeSpan(block).StartOffset + GetMeasuredLength(block, gridTrack) * fraction;
        }

        private double RealizeForward(double availableLength)
        {
            if (BlockViews.Count == 0)
                return 0;

            Debug.Assert(BlockViews.Last != null);

            double result = 0;

            for (int blockOrdinal = BlockViews.Last.Ordinal + 1; blockOrdinal < MaxBlockCount && availableLength > 0; blockOrdinal++)
            {
                BlockViews.RealizeNext();
                var block = BlockViews.Last;
                block.Measure(Size.Empty);
                var measuredLength = GetBlockLengthMain(block);
                result += measuredLength;
                availableLength -= measuredLength;
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
            var valueMain = CoerceViewportMain();
            var valueCross = CoerceViewportCross();
            RefreshViewport(valueMain, valueCross);
        }

        private double CoerceViewportMain()
        {
            if (GridTracksMain.SizeToContent)
                return MaxOffsetMain;

            var result = GridTracksMain.AvailableLength;
            var frozenLength = FrozenHeadLengthMain + FrozenTailLengthMain;
            return Math.Max(frozenLength, result);
        }

        private double CoerceViewportCross()
        {
            if (GridTracksCross.SizeToContent)
                return MaxOffsetCross;

            var result = GridTracksCross.AvailableLength;
            var frozenLength = FrozenHeadLengthCross + FrozenTailLengthCross;
            return Math.Max(frozenLength, result);
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

        private bool IsFrozenHeadMain(TemplateItem templateItem)
        {
            return IsFrozenHeadMain(templateItem.GridRange);
        }

        private bool IsFrozenTailMain(TemplateItem templateItem)
        {
            return IsFrozenTailMain(templateItem.GridRange);
        }

        private bool IsFrozenHeadMain(GridRange gridRange)
        {
            return GridTracksMain.GetGridSpan(gridRange).StartTrack.IsFrozenHead;
        }

        private bool IsFrozenTailMain(GridRange gridRange)
        {
            return GridTracksMain.GetGridSpan(gridRange).EndTrack.IsFrozenTail;
        }

        private bool ShouldStretch(ScalarItem scalarItem)
        {
            return GridTracksMain.GetGridSpan(scalarItem.GridRange).StartTrack.Ordinal >= GridTracksMain.Count - Template.Stretches;
        }

        private bool IsFrozenHeadCross(TemplateItem templateItem)
        {
            return IsFrozenHeadCross(templateItem.GridRange);
        }

        private bool IsFrozenHeadCross(GridRange gridRange)
        {
            return GridTracksCross.GetGridSpan(gridRange).StartTrack.IsFrozenHead;
        }

        private bool IsFrozenTailCross(TemplateItem templateItem)
        {
            return GridTracksCross.GetGridSpan(templateItem.GridRange).EndTrack.IsFrozenTail;
        }

        private bool IsFrozenTailCross(GridRange gridRange)
        {
            return GridTracksCross.GetGridSpan(gridRange).EndTrack.IsFrozenTail;
        }

        protected sealed override Size GetScalarItemSize(ScalarItem scalarItem)
        {
            var valueMain = GetScalarItemLengthMain(scalarItem);
            var valueCross = GetScalarItemLengthCross(scalarItem);
            return ToSize(valueMain, valueCross);
        }

        private double GetScalarItemLengthMain(ScalarItem scalarItem)
        {
            var gridRange = scalarItem.GridRange;
            var startGridOffset = GetStartGridOffset(gridRange);
            var endGridOffset = GetEndGridOffset(gridRange);
            return startGridOffset == endGridOffset ? startGridOffset.Span.Length : endGridOffset.Span.EndOffset - startGridOffset.Span.StartOffset;
        }

        protected override Point GetScalarItemLocation(ScalarItem scalarItem, int blockDimension)
        {
            var valueMain = GetScalarItemStartMain(scalarItem);
            var valueCross = GetScalarItemStartCross(scalarItem, blockDimension);
            return ToPoint(valueMain, valueCross);
        }

        private double GetScalarItemStartMain(ScalarItem scalarItem)
        {
            var gridRange = scalarItem.GridRange;
            var startGridOffset = GetStartGridOffset(gridRange);
            var valueMain = startGridOffset.IsEof ? MaxOffsetMain : startGridOffset.Span.StartOffset;

            if (IsFrozenHeadMain(scalarItem))
                return valueMain;

            valueMain -= ScrollOffsetMain;

            if (IsFrozenTailMain(scalarItem))
            {
                double maxValueMain = ViewportMain - (MaxOffsetMain - startGridOffset.Span.StartOffset);
                if (ShouldStretch(scalarItem))
                    valueMain = maxValueMain;
                else if (valueMain > maxValueMain)
                    valueMain = maxValueMain;
            }

            return valueMain;
        }

        private double GetScalarItemStartCross(ScalarItem scalarItem, int blockDimension)
        {
            return GetStartOffsetCross(scalarItem.GridRange, blockDimension);
        }

        private double GetScalarItemLengthCross(ScalarItem scalarItem)
        {
            var gridRange = scalarItem.GridRange;
            var startOffset = GetStartOffsetCross(gridRange, 0);
            var endOffset = GetEndOffsetCross(gridRange, ShouldStretchCross(scalarItem) ? BlockDimensions - 1 : 0);
            return endOffset - startOffset;
        }

        private bool ShouldStretchCross(ScalarItem scalarItem)
        {
            if (BlockDimensions > 1 && !scalarItem.IsMultidimensional)
            {
                var rowSpan = GridTracksCross.GetGridSpan(Template.RowRange);
                var scalarItemSpan = GridTracksCross.GetGridSpan(scalarItem.GridRange);
                if (rowSpan.Contains(scalarItemSpan))
                    return true;
            }
            return false;
        }

        private Clip GetClipMain(double startOffset, double endOffset, TemplateItem templateItem)
        {
            return GetClipMain(startOffset, endOffset, templateItem.GridRange);
        }

        private Clip GetClipMain(double startOffset, double endOffset, GridRange gridRange)
        {
            double? minStart = FrozenHeadMain == 0 || IsFrozenHeadMain(gridRange) ? new double?() : FrozenHeadLengthMain;
            double? maxEnd = FrozenTailMain == 0 || IsFrozenTailMain(gridRange) ? new double?() : ViewportMain - FrozenTailLengthMain;
            return new Clip(startOffset, endOffset, minStart, maxEnd);
        }

        private Clip GetClipCross(double startOffset, double endOffset, TemplateItem templateItem)
        {
            return GetClipCross(startOffset, endOffset, templateItem.GridRange);
        }

        private Clip GetClipCross(double startOffset, double endOffset, GridRange gridRange)
        {
            double? minStart = FrozenHeadCross == 0 || IsFrozenHeadCross(gridRange) ? new double?() : FrozenHeadLengthCross;
            double? maxEnd = FrozenTailCross == 0 || IsFrozenTailCross(gridRange) ? new double?() : ViewportCross - FrozenTailLengthCross;
            return new Clip(startOffset, endOffset, minStart, maxEnd);
        }

        internal override Thickness GetScalarItemClip(ScalarItem scalarItem, int blockDimension)
        {
            var clipMain = GetScalarItemClipMain(scalarItem);
            var clipCross = GetScalarItemClipCross(scalarItem, blockDimension);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetScalarItemClipMain(ScalarItem scalarItem)
        {
            var startOffset = GetScalarItemStartMain(scalarItem);
            var endOffset = startOffset + GetScalarItemLengthMain(scalarItem);
            return GetClipMain(startOffset, endOffset, scalarItem);
        }

        private Clip GetScalarItemClipCross(ScalarItem scalarItem, int blockDimension)
        {
            var startOffset = GetScalarItemStartCross(scalarItem, blockDimension);
            var endOffset = startOffset + GetScalarItemLengthCross(scalarItem);
            return GetClipCross(startOffset, endOffset, scalarItem);
        }

        private double GetStartOffsetCross(GridRange gridRange, int blockDimension)
        {
            var gridTrack = GridTracksCross.GetGridSpan(gridRange).StartTrack;
            var result = gridTrack.StartOffset;

            if (blockDimension == 0 && gridTrack.IsFrozenHead)
                return result;

            result -= ScrollOffsetCross;
            if (blockDimension > 0)
                result += blockDimension * BlockDimensionLength;

            if (blockDimension == BlockDimensions - 1 && gridTrack.IsFrozenTail)
            {
                double maxValueCross = ViewportCross - (GridTracksCross.LastOf().EndOffset - gridTrack.StartOffset);
                if (result > maxValueCross)
                    result = maxValueCross;
            }

            return result;
        }

        private double GetEndOffsetCross(GridRange gridRange, int blockDimension)
        {
            var gridTrack = GridTracksCross.GetGridSpan(gridRange).EndTrack;
            var result = gridTrack.EndOffset;

            if (blockDimension == 0 && gridTrack.IsFrozenHead)
                return result;

            result -= ScrollOffsetCross;
            if (blockDimension > 0)
                result += blockDimension * BlockDimensionLength;

            if (blockDimension == BlockDimensions - 1 && gridTrack.IsFrozenTail)
            {
                double maxValueCross = ViewportCross - (GridTracksCross.LastOf().EndOffset - gridTrack.EndOffset);
                if (result > maxValueCross)
                    result = maxValueCross;
            }

            return result;
        }

        private double GetMeasuredLengthMain(BlockView block, GridRange gridRange)
        {
            var gridSpan = GridTracksMain.GetGridSpan(gridRange);
            var startTrack = gridSpan.StartTrack;
            var endTrack = gridSpan.EndTrack;
            return startTrack == endTrack ? startTrack.GetRelativeSpan(block).Length
                : endTrack.GetRelativeSpan(block).EndOffset - startTrack.GetRelativeSpan(block).StartOffset;
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

        private GridOffset GetStartGridOffset(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).StartTrack;
            if (!gridTrack.IsRepeat)
                return new GridOffset(gridTrack);

            if (MaxBlockCount > 0)
                return new GridOffset(gridTrack, 0);
            else
                return MaxFrozenTailMain > 0 ? new GridOffset(GridTracksMain.LastOf(MaxFrozenTailMain)) : GridOffset.Eof;
        }

        private GridOffset GetEndGridOffset(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).EndTrack;
            if (!gridTrack.IsRepeat)
                return new GridOffset(gridTrack);

            if (MaxBlockCount > 0)
                return new GridOffset(gridTrack, MaxBlockCount - 1);
            else
                return MaxFrozenTailMain > 0 ? new GridOffset(GridTracksMain.LastOf(MaxFrozenTailMain)) : GridOffset.Eof;
        }

        protected override Point GetBlockLocation(BlockView block)
        {
            var valueMain = GetBlockStartMain(block);
            var valueCross = GetBlockStartCross();
            return ToPoint(valueMain, valueCross);
        }

        private double GetBlockStartMain(BlockView block)
        {
            return GetStartOffset(block) - ScrollOffsetMain;
        }

        private double GetBlockStartCross()
        {
            return GetStartOffsetCross(Template.BlockRange, 0);
        }

        private double GetBlockEndCross()
        {
            return GetEndOffsetCross(Template.BlockRange, BlockDimensions - 1);
        }

        protected override Size GetBlockSize(BlockView block)
        {
            var valueMain = GetBlockLengthMain(block);
            var valueCross = GetBlockLengthCross();
            return ToSize(valueMain, valueCross);
        }

        private double GetBlockLengthMain(BlockView block)
        {
            return GetMeasuredLengthMain(block, Template.BlockRange);
        }

        private double GetBlockLengthCross()
        {
            return GetBlockEndCross() - GetBlockStartCross();
        }

        internal override Thickness GetBlockClip(BlockView block)
        {
            var clipMain = GetBlockClipMain(block);
            var clipCross = GetBlockClipCross();
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetBlockClipMain(BlockView block)
        {
            var startOffset = GetBlockStartMain(block);
            var endOffset = startOffset + GetBlockLengthMain(block);
            return GetClipMain(startOffset, endOffset, Template.BlockRange);
        }

        private Clip GetBlockClipCross()
        {
            var startOffset = GetBlockStartCross();
            var endOffset = GetBlockEndCross();
            return GetClipCross(startOffset, endOffset, Template.BlockRange);
        }

        private double GetStartOffset(BlockView block)
        {
            return new GridOffset(GridTracksMain.BlockStart, block).Span.StartOffset;
        }

        private double GetEndOffset(BlockView block)
        {
            return new GridOffset(GridTracksMain.BlockEnd, block).Span.EndOffset;
        }

        private double GetRelativeStartMain(BlockView block, GridRange gridRange)
        {
            Debug.Assert(Template.BlockRange.Contains(gridRange));

            var startTrackMain = GridTracksMain.GetGridSpan(gridRange).StartTrack;
            return startTrackMain.GetRelativeSpan(block).StartOffset;
        }

        protected override Point GetBlockItemLocation(BlockView block, BlockItem blockItem)
        {
            var valueMain = GetRelativeStartMain(block, blockItem.GridRange);
            var valueCross = GetBlockItemStartCross(block, blockItem) - GetBlockStartCross();
            return ToPoint(valueMain, valueCross);
        }

        protected override Size GetBlockItemSize(BlockView block, BlockItem blockItem)
        {
            var valueMain = GetMeasuredLengthMain(block, blockItem.GridRange);
            var valueCross = GetBlockItemEndCross(block, blockItem) - GetBlockItemStartCross(block, blockItem);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetBlockItemClip(BlockView block, BlockItem blockItem)
        {
            var clipMain = new Clip();
            var clipCross = GetBlockItemClipCross(block, blockItem);
            return ToThickness(clipMain, clipCross);
        }

        private bool IsHead(BlockItem blockItem)
        {
            return GridTracksCross.GetGridSpan(blockItem.GridRange).EndTrack.Ordinal < GridTracksCross.GetGridSpan(Template.RowRange).StartTrack.Ordinal;
        }

        private double GetBlockItemStartCross(BlockView block, BlockItem blockItem)
        {
            return GetStartOffsetCross(blockItem.GridRange, IsHead(blockItem) ? 0 : BlockDimensions - 1);
        }

        private double GetBlockItemEndCross(BlockView block, BlockItem blockItem)
        {
            return GetEndOffsetCross(blockItem.GridRange, IsHead(blockItem) ? 0 : BlockDimensions - 1);
        }

        private Clip GetBlockItemClipCross(BlockView block, BlockItem blockItem)
        {
            var startOffset = GetBlockItemStartCross(block, blockItem);
            var endOffset = GetBlockItemEndCross(block, blockItem);
            return GetClipCross(startOffset, endOffset, blockItem);
        }

        protected override Point GetRowLocation(BlockView block, int blockDimension)
        {
            var valueCross = GetRowStartCross(blockDimension) - GetBlockStartCross();
            return ToPoint(0, valueCross);
        }

        protected override Size GetRowSize(BlockView block, int blockDimension)
        {
            var valueMain = GetMeasuredLengthMain(block, Template.RowRange);
            var valueCross = GetRowEndCross(blockDimension) - GetRowStartCross(blockDimension);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetRowClip(int blockDimension)
        {
            var clipMain = new Clip();
            var clipCross = GetRowClipCross(blockDimension);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetRowClipCross(int blockDimension)
        {
            var startOffset = GetRowStartCross(blockDimension);
            var endOffset = GetRowEndCross(blockDimension);
            return GetClipCross(startOffset, endOffset, Template.RowRange);
        }

        private double GetRowStartCross(int blockDimension)
        {
            return GetStartOffsetCross(Template.RowRange, blockDimension);
        }

        private double GetRowEndCross(int blockDimension)
        {
            return GetEndOffsetCross(Template.RowRange, blockDimension);
        }

        protected override Point GetRowItemLocation(RowPresenter row, RowItem rowItem)
        {
            var valueCross = GetRowItemStartCross(row, rowItem) - GetRowStartCross(row.BlockOrdinal);
            return ToPoint(0, valueCross);
        }

        protected override Size GetRowItemSize(RowPresenter row, RowItem rowItem)
        {
            var valueMain = GetMeasuredLengthMain(BlockViews[row], rowItem.GridRange);
            var valueCross = GetRowItemEndCross(row, rowItem) - GetRowItemStartCross(row, rowItem);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetRowItemClip(RowPresenter row, RowItem rowItem)
        {
            var clipMain = new Clip();
            var clipCross = GetRowItemClipCross(row, rowItem);
            return ToThickness(clipMain, clipCross);
        }

        private double GetRowItemStartCross(RowPresenter row, RowItem rowItem)
        {
            return GetStartOffsetCross(rowItem.GridRange, row.BlockOrdinal);
        }

        private double GetRowItemEndCross(RowPresenter row, RowItem rowItem)
        {
            return GetEndOffsetCross(rowItem.GridRange, row.BlockOrdinal);
        }

        private Clip GetRowItemClipCross(RowPresenter row, RowItem rowItem)
        {
            var startOffset = GetRowItemStartCross(row, rowItem);
            var endOffset = GetRowItemEndCross(row, rowItem);
            return GetClipCross(startOffset, endOffset, rowItem);
        }

        protected sealed override Size MeasuredSize
        {
            get { return new Size(ViewportX, ViewportY); }
        }
    }
}
