using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    // ================================================
    // COORDINATES SYSTEM CONCEPT AND NAME CONVENTIONS:
    // ================================================
    // * Main(axis): the axis where blocks are arranged.
    // * Cross(axis): the axis cross to the main axis.
    // * Offset(coordinate): coordinate before scrolled/frozen.
    // * Location(coordinate): coordinate after scrolled/frozen.
    // - Coordinate + Axis combination are used, for example: OffsetMain, OffsetCross, LocationMain, LocationCross
    // * GridOffset: the (GridTrack, Block) pair to uniquely identify the grid on the main axis, can be converted to/from an int index value.
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
            get { return GetGridOffset(MaxGridOffsetMain - 1).Span.End; }
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

        private double HeadEndOffset
        {
            get { return GridTracksMain[MaxFrozenHeadMain].StartOffset; }
        }

        private double TailStartOffset
        {
            get { return MaxFrozenTailMain == 0 ? MaxOffsetMain : GetGridOffset(MaxGridOffsetMain - MaxFrozenTailMain).Span.Start; }
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
            get { return MaxFrozenTailMain == 0 ? 0 : MaxOffsetMain - GetGridOffset(MaxGridOffsetMain - MaxFrozenTailMain).Span.Start; }
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
            var measuredLength = Math.Min(availableLength, ScrollStartMain - TailStartOffset);
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
            if (availableLength <= 0)
                return;

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
            availableLength -= Math.Max(FrozenHeadLengthMain, HeadEndOffset - ScrollOffsetMain);
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
            var measuredLength = HeadEndOffset - (gridTrack.StartOffset + gridTrack.MeasuredLength * fraction);
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
            return gridTrack.GetRelativeSpan(block).Start + GetMeasuredLength(block, gridTrack) * fraction;
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
            var valueCross = ScrollOffsetCross;
            RefreshScollOffset(valueMain, valueCross);
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
            return startGridOffset == endGridOffset ? startGridOffset.Span.Length : endGridOffset.Span.End - startGridOffset.Span.Start;
        }

        protected override Point GetScalarItemLocation(ScalarItem scalarItem, int blockDimension)
        {
            var valueMain = GetScalarItemStartLocationMain(scalarItem);
            var valueCross = GetScalarItemStartLocationCross(scalarItem, blockDimension);
            return ToPoint(valueMain, valueCross);
        }

        private double GetScalarItemStartLocationMain(ScalarItem scalarItem)
        {
            var startGridOffset = GetStartGridOffset(scalarItem.GridRange);
            return GetStartLocationMain(startGridOffset);
        }

        private double GetStartLocationMain(GridOffset gridOffset)
        {
            var result = gridOffset.IsEof ? MaxOffsetMain : gridOffset.Span.Start;
            var gridTrack = gridOffset.GridTrack;

            if (gridTrack != null && gridTrack.IsFrozenHead)
                return result;

            result -= ScrollOffsetMain;

            if (gridTrack != null && gridTrack.IsFrozenTail)
            {
                double maxValueMain = ViewportMain - (MaxOffsetMain - gridOffset.Span.Start);
                if (gridTrack.Ordinal >= GridTracksMain.Count - Template.Stretches)
                    result = maxValueMain;
                else if (result > maxValueMain)
                    result = maxValueMain;
            }

            return result;
        }

        private double GetEndLocationMain(GridOffset gridOffset)
        {
            Debug.Assert(gridOffset.IsEof || gridOffset.BlockOrdinal == -1);
            return gridOffset.IsEof ? GetStartLocationMain(gridOffset) : GetStartLocationMain(gridOffset) + gridOffset.Span.Length;
        }

        private double GetStartLocationMainRepeat(GridOffset gridOffset)
        {
            Debug.Assert(gridOffset.IsRepeat);

            throw new NotImplementedException();
        }

        private double GetScalarItemStartLocationCross(ScalarItem scalarItem, int blockDimension)
        {
            return GetStartLocationCross(scalarItem.GridRange, blockDimension);
        }

        private double GetScalarItemLengthCross(ScalarItem scalarItem)
        {
            var gridRange = scalarItem.GridRange;
            var startLocation = GetStartLocationCross(gridRange, 0);
            var endLocation = GetEndLocationCross(gridRange, ShouldStretchCross(scalarItem) ? BlockDimensions - 1 : 0);
            return endLocation - startLocation;
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

        private Clip GetClipMain(double startLocation, double endLocation, TemplateItem templateItem)
        {
            return GetClipMain(startLocation, endLocation, templateItem.GridRange);
        }

        private Clip GetClipMain(double startLocation, double endLocation, GridRange gridRange)
        {
            double? minStart = FrozenHeadMain == 0 || IsFrozenHeadMain(gridRange) ? new double?() : FrozenHeadLengthMain;
            double? maxEnd = FrozenTailMain == 0 || IsFrozenTailMain(gridRange) ? new double?() : ViewportMain - FrozenTailLengthMain;
            return new Clip(startLocation, endLocation, minStart, maxEnd);
        }

        private Clip GetClipCross(double startLocation, double endLocation, TemplateItem templateItem, Clip containerClip)
        {
            return GetClipCross(startLocation, endLocation, templateItem.GridRange, containerClip);
        }

        private Clip GetClipCross(double startLocation, double endLocation, GridRange gridRange, Clip containerClip, int? blockDimension = null)
        {
            double? minStart = FrozenHeadCross == 0 || containerClip.Head > 0 || IsFrozenHeadCross(gridRange) ? new double?() : FrozenHeadLengthCross;
            double? maxEnd = FrozenTailCross == 0 || containerClip.Tail > 0 || IsFrozenTailCross(gridRange, blockDimension)
                ? new double?() : ViewportCross - FrozenTailLengthCross;
            return new Clip(startLocation, endLocation, minStart, maxEnd);
        }

        private bool IsFrozenTailCross(GridRange gridRange, int? blockDimension)
        {
            return !blockDimension.HasValue ? IsFrozenTailCross(gridRange) : blockDimension.Value == BlockDimensions - 1 && IsFrozenTailCross(gridRange);
        }

        internal override Thickness GetScalarItemClip(ScalarItem scalarItem, int blockDimension)
        {
            var clipMain = GetScalarItemClipMain(scalarItem);
            var clipCross = GetScalarItemClipCross(scalarItem, blockDimension);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetScalarItemClipMain(ScalarItem scalarItem)
        {
            var startLocation = GetScalarItemStartLocationMain(scalarItem);
            var endLocation = startLocation + GetScalarItemLengthMain(scalarItem);
            return GetClipMain(startLocation, endLocation, scalarItem);
        }

        private Clip GetScalarItemClipCross(ScalarItem scalarItem, int blockDimension)
        {
            var startLocation = GetScalarItemStartLocationCross(scalarItem, blockDimension);
            var endLocation = startLocation + GetScalarItemLengthCross(scalarItem);
            return GetClipCross(startLocation, endLocation, scalarItem, new Clip());
        }

        private double GetStartLocationCross(GridRange gridRange, int blockDimension)
        {
            var gridTrack = GridTracksCross.GetGridSpan(gridRange).StartTrack;
            return GetStartLocationCross(gridTrack, blockDimension);
        }

        private double GetStartLocationCross(GridTrack gridTrack, int blockDimension)
        {
            Debug.Assert(gridTrack.Owner == GridTracksCross);
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

        private double GetEndLocationCross(GridRange gridRange, int blockDimension)
        {
            var gridTrack = GridTracksCross.GetGridSpan(gridRange).EndTrack;
            return GetEndLocationCross(gridTrack, blockDimension);
        }

        private double GetEndLocationCross(GridTrack gridTrack, int blockDimension)
        {
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
                : endTrack.GetRelativeSpan(block).End - startTrack.GetRelativeSpan(block).Start;
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
            var valueMain = GetBlockStartLocationMain(block);
            var valueCross = GetBlockStartLocationCross();
            return ToPoint(valueMain, valueCross);
        }

        private double GetBlockStartLocationMain(BlockView block)
        {
            return GetStartOffset(block) - ScrollOffsetMain;
        }

        private double GetBlockStartLocationCross()
        {
            return GetStartLocationCross(Template.BlockRange, 0);
        }

        private double GetBlockEndLocationCross()
        {
            return GetEndLocationCross(Template.BlockRange, BlockDimensions - 1);
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
            return GetBlockEndLocationCross() - GetBlockStartLocationCross();
        }

        internal override Thickness GetBlockClip(BlockView block)
        {
            var clipMain = GetBlockClipMain(block);
            var clipCross = GetBlockClipCross();
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetBlockClipMain(BlockView block)
        {
            var startLocation = GetBlockStartLocationMain(block);
            var endLocation = startLocation + GetBlockLengthMain(block);
            return GetClipMain(startLocation, endLocation, Template.BlockRange);
        }

        private Clip GetBlockClipCross()
        {
            var startLocation = GetBlockStartLocationCross();
            var endLocation = GetBlockEndLocationCross();
            return GetClipCross(startLocation, endLocation, Template.BlockRange, new Clip());
        }

        private double GetStartOffset(BlockView block)
        {
            return new GridOffset(GridTracksMain.BlockStart, block).Span.Start;
        }

        private double GetEndOffset(BlockView block)
        {
            return new GridOffset(GridTracksMain.BlockEnd, block).Span.End;
        }

        private double GetRelativeStartMain(BlockView block, GridRange gridRange)
        {
            Debug.Assert(Template.BlockRange.Contains(gridRange));

            var startTrackMain = GridTracksMain.GetGridSpan(gridRange).StartTrack;
            return startTrackMain.GetRelativeSpan(block).Start;
        }

        protected override Point GetBlockItemLocation(BlockView block, BlockItem blockItem)
        {
            var valueMain = GetRelativeStartMain(block, blockItem.GridRange);
            var valueCross = GetBlockItemStartLocationCross(block, blockItem) - GetBlockStartLocationCross();
            return ToPoint(valueMain, valueCross);
        }

        protected override Size GetBlockItemSize(BlockView block, BlockItem blockItem)
        {
            var valueMain = GetMeasuredLengthMain(block, blockItem.GridRange);
            var valueCross = GetBlockItemEndLocationCross(block, blockItem) - GetBlockItemStartLocationCross(block, blockItem);
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

        private double GetBlockItemStartLocationCross(BlockView block, BlockItem blockItem)
        {
            return GetStartLocationCross(blockItem.GridRange, IsHead(blockItem) ? 0 : BlockDimensions - 1);
        }

        private double GetBlockItemEndLocationCross(BlockView block, BlockItem blockItem)
        {
            return GetEndLocationCross(blockItem.GridRange, IsHead(blockItem) ? 0 : BlockDimensions - 1);
        }

        private Clip GetBlockItemClipCross(BlockView block, BlockItem blockItem)
        {
            var startLocation = GetBlockItemStartLocationCross(block, blockItem);
            var endLocation = GetBlockItemEndLocationCross(block, blockItem);
            return GetClipCross(startLocation, endLocation, blockItem, GetBlockClipCross());
        }

        protected override Point GetRowLocation(BlockView block, int blockDimension)
        {
            var valueCross = GetRowStartLocationCross(blockDimension) - GetBlockStartLocationCross();
            return ToPoint(0, valueCross);
        }

        protected override Size GetRowSize(BlockView block, int blockDimension)
        {
            var valueMain = GetMeasuredLengthMain(block, Template.RowRange);
            var valueCross = GetRowEndLocationCross(blockDimension) - GetRowStartLocationCross(blockDimension);
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
            var startLocation = GetRowStartLocationCross(blockDimension);
            var endLocation = GetRowEndLocationCross(blockDimension);
            return GetClipCross(startLocation, endLocation, Template.RowRange, GetBlockClipCross(), blockDimension);
        }

        private double GetRowStartLocationCross(int blockDimension)
        {
            var rowRange = Template.RowRange;
            var result = GetStartLocationCross(rowRange, blockDimension);
            if (blockDimension == BlockDimensions - 1 && GridTracksCross.GetGridSpan(rowRange).EndTrack.IsFrozenTail)
                result = Math.Min(ViewportCross - FrozenTailLengthCross, result);
            return result;
        }

        private double GetRowEndLocationCross(int blockDimension)
        {
            return GetEndLocationCross(Template.RowRange, blockDimension);
        }

        protected override Point GetRowItemLocation(RowPresenter row, RowItem rowItem)
        {
            var valueCross = GetRowItemStartLocationCross(row, rowItem) - GetRowStartLocationCross(row.BlockDimension);
            return ToPoint(0, valueCross);
        }

        protected override Size GetRowItemSize(RowPresenter row, RowItem rowItem)
        {
            var valueMain = GetMeasuredLengthMain(BlockViews[row], rowItem.GridRange);
            var valueCross = GetRowItemEndLocationCross(row, rowItem) - GetRowItemStartLocationCross(row, rowItem);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetRowItemClip(RowPresenter row, RowItem rowItem)
        {
            var clipMain = new Clip();
            var clipCross = GetRowItemClipCross(row, rowItem);
            return ToThickness(clipMain, clipCross);
        }

        private double GetRowItemStartLocationCross(RowPresenter row, RowItem rowItem)
        {
            return GetStartLocationCross(rowItem.GridRange, row.BlockDimension);
        }

        private double GetRowItemEndLocationCross(RowPresenter row, RowItem rowItem)
        {
            return GetEndLocationCross(rowItem.GridRange, row.BlockDimension);
        }

        private Clip GetRowItemClipCross(RowPresenter row, RowItem rowItem)
        {
            var startLocation = GetRowItemStartLocationCross(row, rowItem);
            var endLocation = GetRowItemEndLocationCross(row, rowItem);
            var containerClip = GetBlockClipCross().Merge(GetRowClipCross(row.BlockDimension));
            return GetClipCross(startLocation, endLocation, rowItem, containerClip);
        }

        protected sealed override Size MeasuredSize
        {
            get { return new Size(ViewportX, ViewportY); }
        }

        internal override IEnumerable<GridLineFigure> GridLineFigures
        {
            get
            {
                foreach (var gridLine in Template.GridLines)
                {
                    var position = CoerceGridLinePosition(gridLine);
                    if (!position.HasValue)
                        continue;

                    var positionValue = position.GetValueOrDefault();
                    if ((positionValue & GridLinePosition.PreviousTrack) == GridLinePosition.PreviousTrack)
                        yield return GetPrevTrackGridLineFigure(gridLine);
                    else if ((positionValue & GridLinePosition.NextTrack) == GridLinePosition.NextTrack)
                        yield return GetNextTrackGridLineFigure(gridLine);
                }
            }
        }

        private GridLinePosition? CoerceGridLinePosition(GridLine gridLine)
        {
            throw new NotImplementedException();
        }

        private GridLineFigure GetPrevTrackGridLineFigure(GridLine gridLine)
        {
            throw new NotImplementedException();
        }

        private GridLineFigure GetNextTrackGridLineFigure(GridLine gridLine)
        {
            throw new NotImplementedException();
        }
    }
}
