﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    // ================================================
    // CONCEPT AND NAME CONVENTIONS:
    // ================================================
    // * Main(Axis): The axis where ContainerViews are arranged.
    // * Cross(Axis): The axis cross to the main axis.
    // * Extent(Coordinate): Coordinate before scrolled/frozen.
    // * Position(Coordinate): Coordinate after scrolled/frozen.
    // * Coordinate + Axis combination are used, for example: ExtentMain, ExtentCross, PositionMain, PositionCross
    // * Grid Point: Int value between 0 and number of grid tracks inclusive, to identify grid position in the template.
    // * Grid Extent: Int value between 0 and number of extented grid tracks inclusive, to identify grid position in the layout.
    // * LogicalMainTrack: the (GridTrack, ContainerView) pair to identify the grid track on the main axis,
    //   can be converted to/from an int Grid Extent value.
    // * LogicalCrossTrack: the (GridTrack, flowIndex) pair to identity the grid track on the cross axis,
    //   can be converted to/from an int Grid Extent value.
    internal abstract partial class ScrollableManager : LayoutManager, IScrollHandler, IScrollable
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Derived classes are limited to class LayoutXManager/LayoutYManager, and the overrides do not rely on completion of its constructor.")]
        protected ScrollableManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy, false)
        {
            _scrollStartMain = ScrollOriginMain;
        }

        internal abstract IGridTrackCollection GridTracksMain { get; }
        internal abstract IGridTrackCollection GridTracksCross { get; }

        private void InitContainerViews()
        {
            if (_variantLengthHandler != null)
                _variantLengthHandler.ClearMeasuredLengths();
            ContainerViewList.VirtualizeAll();

            var initialOrdinal = GetInitialOrdinal();
            if (initialOrdinal >= 0)
            {
                ContainerViewList.RealizeFirst(initialOrdinal);
                ContainerViewList[0].Measure(Size.Empty);
            }
        }

        private int GetInitialOrdinal()
        {
            if (MaxContainerCount == 0)
                return -1;

            var logicalMainTrack = GetLogicalMainTrack(_scrollStartMain.GridExtent);
            if (logicalMainTrack.IsEof)
                return MaxContainerCount - 1;

            var gridTrack = logicalMainTrack.GridTrack;
            if (gridTrack.IsHead)
                return 0;
            else if (gridTrack.IsRepeat)
                return logicalMainTrack.ContainerOrdinal;
            else
                return MaxContainerCount - 1;
        }

        private LogicalExtent ScrollOriginMain
        {
            get { return new LogicalExtent(FrozenHeadMain); }
        }

        private LogicalExtent _scrollStartMain;
        private double ScrollStartMain
        {
            get { return Translate(_scrollStartMain); }
        }

        private LogicalMainTrack ScrollEndOffsetMain
        {
            get { return MaxFrozenTailMain > 0 ? new LogicalMainTrack(GridTracksMain.LastOf(MaxFrozenTailMain)) : LogicalMainTrack.Eof; }
        }

        private void AdjustScrollStartMain(double delta)
        {
            _scrollStartMain = Translate(ScrollStartMain + delta);
            var scrollOriginMain = ScrollOriginMain;
            if (_scrollStartMain.Value < scrollOriginMain.Value)
                _scrollStartMain = scrollOriginMain;
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

        private int MaxContainerCount
        {
            get { return ContainerViewList.MaxCount; }
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

        private int ContainerGridTracksMain
        {
            get { return GridTracksMain.ContainerEnd.Ordinal - GridTracksMain.ContainerStart.Ordinal + 1; }
        }

        private int TotalContainerGridTracksMain
        {
            get { return MaxContainerCount * ContainerGridTracksMain; }
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

        private int MaxGridExtentMain
        {
            get { return MaxFrozenHeadMain + TotalContainerGridTracksMain + MaxFrozenTailMain; }
        }

        private double MaxExtentMain
        {
            get { return GetLogicalMainTrack(MaxGridExtentMain - 1).EndExtent; }
        }

        private double MaxExtentCross
        {
            get
            {
                var result = GridTracksCross.GetMeasuredLength(Template.Range());
                if (FlowCount > 1)
                    result += FlowLength * (FlowCount - 1);
                return result;
            }
        }

        private double FlowLength
        {
            get { return GridTracksCross.GetMeasuredLength(Template.RowRange); }
        }

        protected override void OnRowsChanged()
        {
            base.OnRowsChanged();
            InvalidateMeasure();
        }

        private void InitScroll()
        {
            if (DeltaScrollOffset == 0 || (DeltaScrollOffset < 0 && Math.Abs(DeltaScrollOffset) <= ViewportMain))
                return;

            AdjustScrollStartMain(DeltaScrollOffset);
            ClearDeltaScrollOffset();
        }

        protected sealed override void PrepareMeasureContainers()
        {
            InitScroll();
            InitContainerViews();

            if (DeltaScrollOffset < 0 || _scrollStartMain.GridExtent >= MaxGridExtentMain)
                MeasureBackward(-DeltaScrollOffset, true);
            else
                MeasureForward(GridTracksMain.AvailableLength);
            FillGap();
            RefreshScrollInfo();
        }

        private double HeadEndOffset
        {
            get { return MaxFrozenHeadMain == 0 ? 0 : GridTracksMain[MaxFrozenHeadMain - 1].EndOffset; }
        }

        private double TailStartOffset
        {
            get { return MaxFrozenTailMain == 0 ? MaxExtentMain : GetLogicalMainTrack(MaxGridExtentMain - MaxFrozenTailMain).StartExtent; }
        }

        private double FrozenHeadLengthMain
        {
            get { return FrozenHeadMain == 0 ? 0 : GridTracksMain[FrozenHeadMain - 1].EndOffset; }
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

        private double TailLengthMain
        {
            get { return MaxFrozenTailMain == 0 ? 0 : MaxExtentMain - GetLogicalMainTrack(MaxGridExtentMain - MaxFrozenTailMain).StartExtent; }
        }

        private double GapToFill
        {
            get
            {
                var availableLength = GridTracksMain.AvailableLength;
                if (double.IsPositiveInfinity(availableLength))
                    return availableLength;

                var scrollable = availableLength - (FrozenHeadLengthMain + FrozenTailLengthMain);
                var endOffset = ContainerViewList.Count == 0 ? GridTracksMain[MaxFrozenHeadMain].StartOffset : GetEndExtent(ContainerViewList[ContainerViewList.Count - 1]);
                return scrollable - (endOffset - ScrollStartMain);
            }
        }

        private void FillGap()
        {
            var gap = GapToFill;
            gap -= RealizeForward(gap) + TailLengthMain - FrozenTailLengthMain;
            if (gap > 0)
                MeasureBackward(gap, false);
        }

        private void MeasureBackward(double availableLength, bool flagScrollBack)
        {
            Debug.Assert(availableLength >= 0);

            var logicalMainTrack = GetLogicalMainTrack(_scrollStartMain.GridExtent);
            if (logicalMainTrack.IsEof)
            {
                MeasureBackwardEof(availableLength, flagScrollBack);
                return;
            }

            var gridTrack = logicalMainTrack.GridTrack;
            if (gridTrack.IsTail)
                MeasureBackwardTail(availableLength, flagScrollBack);
            else if (gridTrack.IsRepeat)
                MeasureBackwardRepeat(availableLength, flagScrollBack);
            else
            {
                Debug.Assert(gridTrack.IsHead);
                MeasureBackwardHead(availableLength, flagScrollBack);
            }
        }

        private void MeasureBackwardEof(double availableLength, bool flagScrollBack)
        {
            if (MaxFrozenTailMain > 0)
                MeasureBackwardTail(availableLength, flagScrollBack);
            else if (MaxContainerCount > 0)
                MeasureBackwardRepeat(availableLength, flagScrollBack);
            else if (MaxFrozenHeadMain > 0)
                MeasureBackwardHead(availableLength, flagScrollBack);
        }

        private void MeasureBackwardTail(double availableLength, bool flagScrollBack)
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

            if (MaxContainerCount > 0)
                MeasureBackwardRepeat(availableLength, flagScrollBack);
            else if (MaxFrozenHeadMain > 0)
                MeasureBackwardHead(availableLength, flagScrollBack);
        }

        private void MeasureBackwardRepeat(double availableLength, bool flagScrollBack)
        {
            if (availableLength <= 0)
                return;

            var containerView = ContainerViewList[0];
            var scrollLength = Math.Min(availableLength, ScrollStartMain - GetStartExtent(containerView));
            if (scrollLength > 0)
            {
                AdjustScrollStartMain(-scrollLength);
                availableLength -= scrollLength;
            }
            availableLength = RealizeBackward(availableLength);
            if (availableLength > 0 && FrozenHeadMain > 0)
                MeasureBackwardHead(availableLength, flagScrollBack);
        }

        private double RealizeBackward(double availableLength)
        {
            if (ContainerViewList.Count == 0)
                return availableLength;

            Debug.Assert(ContainerViewList.Count > 0);

            for (int ordinal = ContainerViewList.First.ContainerOrdinal - 1; ordinal >= 0 && availableLength > 0; ordinal--)
            {
                ContainerViewList.RealizePrev();
                var containerView = ContainerViewList.First;
                containerView.Measure(Size.Empty);
                var measuredLength = Math.Min(availableLength, GetLengthMain(containerView));
                AdjustScrollStartMain(-measuredLength);
                availableLength -= measuredLength;
            }
            return availableLength;
        }

        private void MeasureBackwardHead(double availableLength, bool flagScrollBack)
        {
            Debug.Assert(availableLength >= 0);
            var measuredLength = Math.Min(availableLength, flagScrollBack ? _oldScrollOffsetMain : ScrollOffsetMain);
            if (measuredLength > 0)
               AdjustScrollStartMain(-measuredLength);
        }

        private void MeasureForward(double availableLength)
        {
            availableLength -= Math.Max(FrozenHeadLengthMain, HeadEndOffset - ScrollOffsetMain);
            if (availableLength <= 0)
                return;

            var logicalMainTrack = GetLogicalMainTrack(_scrollStartMain.GridExtent);
            Debug.Assert(!logicalMainTrack.IsEof);
            var gridTrack = logicalMainTrack.GridTrack;
            var fraction = _scrollStartMain.Fraction;
            if (gridTrack.IsHead)
                MeasureForwardHead(gridTrack, fraction, availableLength);
            else if (gridTrack.IsRepeat)
                MeasureForwardRepeat(logicalMainTrack, fraction, availableLength);
        }

        private void MeasureForwardHead(GridTrack gridTrack, double fraction, double availableLength)
        {
            Debug.Assert(gridTrack.IsHead);
            var measuredLength = HeadEndOffset - (gridTrack.StartOffset + gridTrack.MeasuredLength * fraction);
            Debug.Assert(measuredLength >= 0);
            if (MaxContainerCount > 0)
                MeasureForwardRepeat(new LogicalMainTrack(GridTracksMain.ContainerStart, 0), 0, availableLength - measuredLength);
        }

        private void MeasureForwardRepeat(LogicalMainTrack logicalGridTrack, double fraction, double availableLength)
        {
            Debug.Assert(ContainerViewList.Count == 1);
            if (FrozenTailMain > 0)
                availableLength -= FrozenTailLengthMain;

            var gridTrack = logicalGridTrack.GridTrack;
            Debug.Assert(gridTrack.IsRepeat);
            var containerView = ContainerViewList[0];
            Debug.Assert(containerView.ContainerOrdinal == logicalGridTrack.ContainerOrdinal);
            availableLength -= GetLengthMain(containerView) - GetRelativeOffsetMain(containerView, gridTrack, fraction);
            RealizeForward(availableLength);
        }

        private double GetRelativeOffsetMain(ContainerView containerView, GridTrack gridTrack)
        {
            Debug.Assert(GridTracksMain.GetGridSpan(Template.BlockRange).Contains(gridTrack));
            return new LogicalMainTrack(gridTrack, containerView).StartPosition - GetStartPositionMain(containerView);
        }

        private double GetRelativeOffsetMain(ContainerView containerView, GridTrack gridTrack, double fraction)
        {
            return GetRelativeOffsetMain(containerView, gridTrack) + GetMeasuredLength(containerView, gridTrack) * fraction;
        }

        private double RealizeForward(double availableLength)
        {
            if (ContainerViewList.Count == 0)
                return 0;

            Debug.Assert(ContainerViewList.Last != null);

            double result = 0;

            for (int ordinal = ContainerViewList.Last.ContainerOrdinal + 1; ordinal < MaxContainerCount && availableLength > 0; ordinal++)
            {
                ContainerViewList.RealizeNext();
                var containerView = ContainerViewList.Last;
                containerView.Measure(Size.Empty);
                var measuredLength = GetLengthMain(containerView);
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
            var valueMain = MaxExtentMain;
            if (valueMain < ViewportMain)
                valueMain = ViewportMain;
            var valueCross = MaxExtentCross;
            if (valueCross < ViewportCross)
                valueCross = ViewportCross;
            RefreshExtent(valueMain, valueCross);
        }

        private void RefreshViewport()
        {
            var valueMain = CoerceViewport(GridTracksMain, MaxExtentMain, FrozenHeadLengthMain, FrozenTailLengthMain);
            var valueCross = CoerceViewport(GridTracksCross, MaxExtentCross, FrozenHeadLengthCross, FrozenTailLengthCross);
            RefreshViewport(valueMain, valueCross);
        }

        private static double CoerceViewport(IGridTrackCollection gridTracks, double maxOffset, double frozenHeadLength, double frozenTailLength)
        {
            if (gridTracks.SizeToContent)
                return maxOffset;

            var result = gridTracks.AvailableLength;
            var frozenLength = frozenHeadLength + frozenTailLength;
            return Math.Max(frozenLength, result);
        }

        private void RefreshScrollOffset()
        {
            var scrollOriginMain = Translate(ScrollOriginMain);
            var scrollStartMain = ScrollStartMain;
            Debug.Assert(scrollStartMain >= scrollOriginMain);
            var valueMain = scrollStartMain - scrollOriginMain;
            var valueCross = Math.Min(ScrollOffsetCross, ExtentCross - ViewportCross);
            RefreshScollOffset(valueMain, valueCross);
        }

        private bool IsFrozenHeadMain(GridSpan gridSpan)
        {
            var startTrack = gridSpan.StartTrack;
            Debug.Assert(startTrack.Owner == GridTracksMain);
            return startTrack.IsFrozenHead;
        }

        private bool IsFrozenTailMain(GridSpan gridSpan)
        {
            var endTrack = gridSpan.EndTrack;
            Debug.Assert(endTrack.Owner == GridTracksMain);
            return endTrack.IsFrozenTail;
        }

        private bool IsFrozenHeadCross(Binding binding)
        {
            return IsFrozenHeadCross(binding.GridRange);
        }

        private bool IsFrozenHeadCross(GridRange gridRange)
        {
            return GridTracksCross.GetGridSpan(gridRange).StartTrack.IsFrozenHead;
        }

        protected sealed override Size GetSize(ScalarBinding scalarBinding)
        {
            var valueMain = GetLengthMain(scalarBinding);
            var valueCross = GetLengthCross(scalarBinding);
            return ToSize(valueMain, valueCross);
        }

        private double GetLengthMain(ScalarBinding scalarBinding)
        {
            var gridRange = scalarBinding.GridRange;
            var startGridOffset = GetStartLogicalMainTrack(gridRange);
            var endGridOffset = GetEndLogicalMainTrack(gridRange);
            return startGridOffset == endGridOffset ? startGridOffset.Length : endGridOffset.EndExtent - startGridOffset.StartExtent;
        }

        protected override Point GetPosition(ScalarBinding scalarBinding, int flowIndex)
        {
            var valueMain = GetStartPositionMain(scalarBinding);
            var valueCross = GetStartPositionCross(scalarBinding, flowIndex);
            return ToPoint(valueMain, valueCross);
        }

        private double GetStartPositionMain(ScalarBinding scalarBinding)
        {
            return GetStartLogicalMainTrack(scalarBinding.GridRange).StartPosition;
        }

        private int MinStretchGridOrdinal
        {
            get { return GridTracksMain.Count - Template.Stretches; }
        }

        private double GetStartPositionCross(ScalarBinding scalarBinding, int flowIndex)
        {
            return GetStartPositionCross(scalarBinding.GridRange, flowIndex);
        }

        private double GetLengthCross(ScalarBinding scalarBinding)
        {
            var gridRange = scalarBinding.GridRange;
            var startPosition = GetStartPositionCross(gridRange, 0);
            var endPosition = GetEndPositionCross(gridRange, ShouldStretchCross(scalarBinding) ? FlowCount - 1 : 0);
            return endPosition - startPosition;
        }

        private bool ShouldStretchCross(ScalarBinding scalarBinding)
        {
            if (FlowCount > 1 && !scalarBinding.Flowable)
            {
                var rowSpan = GridTracksCross.GetGridSpan(Template.RowRange);
                var scalarBindingSpan = GridTracksCross.GetGridSpan(scalarBinding.GridRange);
                if (rowSpan.Contains(scalarBindingSpan))
                    return true;
            }
            return false;
        }

        private Clip GetClipMain(double startPosition, double endPosition, Binding binding)
        {
            return GetClipMain(startPosition, endPosition, binding.GridRange);
        }

        private Clip GetClipMain(double startPosition, double endPosition, GridRange gridRange)
        {
            var gridSpan = GridTracksMain.GetGridSpan(gridRange);
            return GetClipMain(startPosition, endPosition, gridSpan);
        }

        private Clip GetClipMain(double startPosition, double endPosition, GridSpan gridSpan)
        {
            double? minStart = GetMinClipMain(gridSpan.StartTrack);
            double? maxEnd = GetMaxClipMain(gridSpan.EndTrack);
            return new Clip(startPosition, endPosition, minStart, maxEnd);
        }

        private double? GetMinClipMain(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Owner == GridTracksMain);
            return FrozenHeadMain == 0 || gridTrack.IsFrozenHead ? new double?() : FrozenHeadLengthMain;
        }

        private double? GetMaxClipMain(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Owner == GridTracksMain);
            return FrozenTailMain == 0 || gridTrack.IsFrozenTail ? new double?() : ViewportMain - FrozenTailLengthMain;
        }

        private Clip GetClipCross(double startPosition, double endPosition, Binding binding, Clip containerClip)
        {
            return GetClipCross(startPosition, endPosition, binding.GridRange, containerClip);
        }

        private Clip GetClipCross(double startPosition, double endPosition, GridRange gridRange, Clip containerClip, int? flowIndex = null)
        {
            var gridSpan = GridTracksCross.GetGridSpan(gridRange);
            return GetClipCross(startPosition, endPosition, gridSpan, containerClip, flowIndex);
        }

        private Clip GetClipCross(double startPosition, double endPosition, GridSpan gridSpan, Clip containerClip = new Clip(), int? flowIndex = null)
        {
            double? minStart = GetMinClipCross(gridSpan.StartTrack, containerClip);
            double? maxEnd = GetMaxClipCross(gridSpan.EndTrack, flowIndex, containerClip);
            return new Clip(startPosition, endPosition, minStart, maxEnd);
        }

        private double? GetMinClipCross(GridTrack gridTrack, Clip containerClip = new Clip())
        {
            return FrozenHeadCross == 0 || containerClip.Head > 0 || gridTrack.IsFrozenHead ? new double?() : FrozenHeadLengthCross;
        }

        private double? GetMaxClipCross(GridTrack gridTrack, int? flowIndex = null, Clip containerClip = new Clip())
        {
            return FrozenTailCross == 0 || containerClip.Tail > 0 || IsFrozenTailCross(gridTrack, flowIndex)
                ? new double?() : ViewportCross - FrozenTailLengthCross;
        }

        private bool IsFrozenTailCross(GridTrack gridTrack, int? flowIndex)
        {
            Debug.Assert(gridTrack.Owner == GridTracksCross);
            return !flowIndex.HasValue ? gridTrack.IsFrozenTail : flowIndex.Value == FlowCount - 1 && gridTrack.IsFrozenTail;
        }

        internal override Thickness GetClip(ScalarBinding scalarBinding, int flowIndex)
        {
            var clipMain = GetClipMain(scalarBinding);
            var clipCross = GetClipCross(scalarBinding, flowIndex);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetClipMain(ScalarBinding scalarBinding)
        {
            var startPosition = GetStartPositionMain(scalarBinding);
            var endPosition = startPosition + GetLengthMain(scalarBinding);
            return GetClipMain(startPosition, endPosition, scalarBinding);
        }

        private Clip GetClipCross(ScalarBinding scalarBinding, int flowIndex)
        {
            var startPosition = GetStartPositionCross(scalarBinding, flowIndex);
            var endPosition = startPosition + GetLengthCross(scalarBinding);
            return GetClipCross(startPosition, endPosition, scalarBinding, new Clip());
        }

        private double GetMeasuredLengthMain(ContainerView containerView, GridRange gridRange)
        {
            var gridSpan = GridTracksMain.GetGridSpan(gridRange);
            var startTrack = gridSpan.StartTrack;
            var endTrack = gridSpan.EndTrack;
            return startTrack == endTrack ? new LogicalMainTrack(startTrack, containerView).Length
                : new LogicalMainTrack(endTrack, containerView).EndExtent - new LogicalMainTrack(startTrack, containerView).StartExtent;
        }

        protected override Point GetPosition(ContainerView containerView)
        {
            var valueMain = GetStartPositionMain(containerView);
            var valueCross = GetContainerStartPositionCross();
            return ToPoint(valueMain, valueCross);
        }

        private double GetStartPositionMain(ContainerView containerView)
        {
            var startTrack = GridTracksMain.GetGridSpan(Template.BlockRange).StartTrack;
            return new LogicalMainTrack(startTrack, containerView).StartPosition;
        }

        private double GetContainerStartPositionCross()
        {
            return GetStartPositionCross(Template.BlockRange, 0);
        }

        private double GetContainerEndPositionCross()
        {
            return GetEndPositionCross(Template.BlockRange, FlowCount - 1);
        }

        protected override Size GetSize(ContainerView containerView)
        {
            var valueMain = GetLengthMain(containerView);
            var valueCross = GetContainerLengthCross();
            return ToSize(valueMain, valueCross);
        }

        private double GetLengthMain(ContainerView containerView)
        {
            return GetMeasuredLengthMain(containerView, Template.BlockRange);
        }

        private double GetContainerLengthCross()
        {
            return GetContainerEndPositionCross() - GetContainerStartPositionCross();
        }

        internal override Thickness GetClip(ContainerView containerView)
        {
            var clipMain = GetClipMain(containerView);
            var clipCross = GetContainerClipCross();
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetClipMain(ContainerView containerView)
        {
            var startPosition = GetStartPositionMain(containerView);
            var endPosition = startPosition + GetLengthMain(containerView);
            return GetClipMain(startPosition, endPosition, Template.BlockRange);
        }

        private Clip GetContainerClipCross()
        {
            var startPosition = GetContainerStartPositionCross();
            var endPosition = GetContainerEndPositionCross();
            return GetClipCross(startPosition, endPosition, Template.BlockRange, new Clip());
        }

        private double GetStartExtent(ContainerView containerView)
        {
            return new LogicalMainTrack(GridTracksMain.ContainerStart, containerView).StartExtent;
        }

        private double GetEndExtent(ContainerView containerView)
        {
            return new LogicalMainTrack(GridTracksMain.ContainerEnd, containerView).EndExtent;
        }

        protected override Point GetPosition(BlockView blockView, BlockBinding blockBinding)
        {
            var valueMain = GetStartPositionMain(blockView, blockBinding);
            var valueCross = GetStartPositionCross(blockView, blockBinding) - GetContainerStartPositionCross();
            return ToPoint(valueMain, valueCross);
        }

        private double GetStartPositionMain(BlockView blockView, BlockBinding blockBinding)
        {
            var startTrack = GridTracksMain.GetGridSpan(blockBinding.GridRange).StartTrack;
            return GetRelativeOffsetMain(blockView, startTrack);
        }

        protected override Size GetSize(BlockView blockView, BlockBinding blockBinding)
        {
            var valueMain = GetMeasuredLengthMain(blockView, blockBinding.GridRange);
            var valueCross = GetEndPositionCross(blockView, blockBinding) - GetStartPositionCross(blockView, blockBinding);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetClip(BlockView blockView, BlockBinding blockBinding)
        {
            var clipMain = new Clip();
            var clipCross = GetClipCross(blockView, blockBinding);
            return ToThickness(clipMain, clipCross);
        }

        private bool IsHead(BlockBinding blockBinding)
        {
            return GridTracksCross.GetGridSpan(blockBinding.GridRange).EndTrack.Ordinal < GridTracksCross.GetGridSpan(Template.RowRange).StartTrack.Ordinal;
        }

        private double GetStartPositionCross(BlockView blockView, BlockBinding blockBinding)
        {
            return GetStartPositionCross(blockBinding.GridRange, IsHead(blockBinding) ? 0 : FlowCount - 1);
        }

        private double GetEndPositionCross(BlockView blockView, BlockBinding blockBinding)
        {
            return GetEndPositionCross(blockBinding.GridRange, IsHead(blockBinding) ? 0 : FlowCount - 1);
        }

        private Clip GetClipCross(BlockView blockView, BlockBinding blockBinding)
        {
            var startPosition = GetStartPositionCross(blockView, blockBinding);
            var endPosition = GetEndPositionCross(blockView, blockBinding);
            return GetClipCross(startPosition, endPosition, blockBinding, GetContainerClipCross());
        }

        private double GetRowViewStartPositionCross(int flowIndex)
        {
            var rowRange = Template.RowRange;
            var result = GetStartPositionCross(rowRange, flowIndex);
            if (flowIndex == FlowCount - 1 && GridTracksCross.GetGridSpan(rowRange).EndTrack.IsFrozenTail)
                result = Math.Min(ViewportCross - FrozenTailLengthCross, result);
            return result;
        }

        protected override Point GetPosition(BlockView blockView, int flowIndex)
        {
            var valueCross = GetRowViewStartPositionCross(flowIndex) - GetContainerStartPositionCross();
            return ToPoint(0, valueCross);
        }

        protected override Size GetSize(BlockView blockView, int flowIndex)
        {
            var valueMain = GetMeasuredLengthMain(blockView, Template.RowRange);
            var valueCross = GetEndPositionCross(Template.RowRange, flowIndex) - GetRowViewStartPositionCross(flowIndex);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetClip(int flowIndex)
        {
            var clipMain = new Clip();
            var clipCross = GetClipCross(flowIndex);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetClipCross(int flowIndex)
        {
            var startPosition = GetRowViewStartPositionCross(flowIndex);
            var endPosition = GetEndPositionCross(Template.RowRange, flowIndex);
            return GetClipCross(startPosition, endPosition, Template.RowRange, GetContainerClipCross(), flowIndex);
        }

        protected override Point GetPosition(RowView rowView, RowBinding rowBinding)
        {
            var valueMain = GetStartPositionMain(rowView, rowBinding);
            var valueCross = GetStartPositionCross(rowView, rowBinding) - GetRowViewStartPositionCross(rowView.FlowIndex);
            return ToPoint(valueMain, valueCross);
        }

        private double GetStartPositionMain(RowView rowView, RowBinding rowBinding)
        {
            var containerView = this[rowView];
            var startGridTrack = GridTracksMain.GetGridSpan(rowBinding.GridRange).StartTrack;
            return new LogicalMainTrack(startGridTrack, containerView).StartPosition - GetStartPositionMain(containerView);
        }

        protected override Size GetSize(RowView rowView, RowBinding rowBinding)
        {
            var valueMain = GetMeasuredLengthMain(this[rowView], rowBinding.GridRange);
            var valueCross = GetEndPositionCross(rowView, rowBinding) - GetStartPositionCross(rowView, rowBinding);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetClip(RowView rowView, RowBinding rowBinding)
        {
            var clipMain = new Clip();
            var clipCross = GetClipCross(rowView, rowBinding);
            return ToThickness(clipMain, clipCross);
        }

        private double GetStartPositionCross(RowView rowView, RowBinding rowBinding)
        {
            return GetStartPositionCross(rowBinding.GridRange, rowView.FlowIndex);
        }

        private double GetEndPositionCross(RowView rowView, RowBinding rowBinding)
        {
            return GetEndPositionCross(rowBinding.GridRange, rowView.FlowIndex);
        }

        private Clip GetClipCross(RowView rowView, RowBinding rowBinding)
        {
            var startPosition = GetStartPositionCross(rowView, rowBinding);
            var endPosition = GetEndPositionCross(rowView, rowBinding);
            var containerClip = GetContainerClipCross().Merge(GetClipCross(rowView.FlowIndex));
            return GetClipCross(startPosition, endPosition, rowBinding, containerClip);
        }

        protected sealed override Size MeasuredSize
        {
            get { return new Size(ViewportWidth, ViewportHeight); }
        }

        internal override IEnumerable<GridLineFigure> GridLineFigures
        {
            get
            {
                var gridLines = Template.GridLines;
                for (int i = 0; i < gridLines.Count; i++)
                {
                    var gridLine = gridLines[i];
                    foreach (var lineFigure in GetLineFigures(gridLine))
                        yield return new GridLineFigure(gridLine, lineFigure);
                }
            }
        }

        private IEnumerable<LineFigure> GetLineFigures(GridLine gridLine)
        {
            return gridLine.Orientation == Orientation.Horizontal
                ? GetLineFiguresX(gridLine.StartGridPoint.X, gridLine.EndGridPoint.X, gridLine.Placement, gridLine.StartGridPoint.Y)
                : GetLineFiguresY(gridLine.StartGridPoint.Y, gridLine.EndGridPoint.Y, gridLine.Placement, gridLine.StartGridPoint.X);
        }

        protected abstract IEnumerable<LineFigure> GetLineFiguresX(int startGridPointX, int endGridPointX, GridPointPlacement placement, int gridPointY);

        protected abstract IEnumerable<LineFigure> GetLineFiguresY(int startGridPointY, int endGridPointY, GridPointPlacement placement, int gridPointX);

        private static GridTrack GetPrevGridTrack(IReadOnlyList<GridTrack> gridTracks, int gridPoint, GridPointPlacement placement)
        {
            if ((placement & GridPointPlacement.PreviousTrack) != GridPointPlacement.PreviousTrack)
                return null;
            return gridPoint == 0 ? null : gridTracks[gridPoint - 1];
        }

        private static GridTrack GetNextGridTrack(IReadOnlyList<GridTrack> gridTracks, int gridPoint, GridPointPlacement placement)
        {
            if ((placement & GridPointPlacement.NextTrack) != GridPointPlacement.NextTrack)
                return null;
            return gridPoint == gridTracks.Count ? null : gridTracks[gridPoint];
        }

        protected IEnumerable<LineFigure> GetLineFiguresMain(int startGridPointMain, int endGridPointMain, GridPointPlacement placement, int gridPointCross)
        {
            var spansMain = GetLineFigureSpansMain(startGridPointMain, endGridPointMain);
            if (spansMain == null)
                yield break;

            foreach (var positionCross in GetLineFigurePositionsCross(gridPointCross, placement))
            {
                foreach (var spanMain in spansMain)
                    yield return new LineFigure(ToPoint(spanMain.Start, positionCross), ToPoint(spanMain.End, positionCross));
            }
        }

        private Span[] GetLineFigureSpansMain(int startGridPointMain, int endGridPointMain)
        {
            var startTrackMain = GridTracksMain[startGridPointMain];
            var endTrackMain = GridTracksMain[endGridPointMain - 1];
            var startPositionMain = GetStartLogicalMainTrack(startTrackMain).StartPosition;
            var endPositionMain = GetEndLogicalMainTrack(endTrackMain).EndPosition;
            if (endPositionMain <= startPositionMain)
                return null;

            var clip = GetClipMain(startPositionMain, endPositionMain, new GridSpan(startTrackMain, endTrackMain));
            if (double.IsPositiveInfinity(clip.Head) || double.IsPositiveInfinity(clip.Tail))
                return null;
            startPositionMain += clip.Head;
            endPositionMain -= clip.Tail;
            if (endPositionMain <= startPositionMain)
                return null;

            return new Span(startPositionMain, endPositionMain).Split(StretchGap);
        }

        private Span? StretchGap
        {
            get
            {
                const double Epsilon = 1e-8;

                if (Template.Stretches == 0)
                    return null;

                var minStretchGridOrdinal = MinStretchGridOrdinal;

                var startPosition = GetPrevTrackEndPositionMain(minStretchGridOrdinal);
                var endPosition = new LogicalMainTrack(GridTracksMain[minStretchGridOrdinal]).StartPosition;
                return startPosition + Epsilon < endPosition ? new Span(startPosition, endPosition) : default(Span?);
            }
        }

        private double GetPrevTrackEndPositionMain(int gridPoint)
        {
            var prevTrack = GridTracksMain[gridPoint - 1];
            if (!prevTrack.IsRepeat)
                return new LogicalMainTrack(prevTrack).EndPosition;

            if (ContainerViewList.Last != null)
                return new LogicalMainTrack(prevTrack, ContainerViewList.Last).EndPosition;

            prevTrack = MaxFrozenHeadMain == 0 ? null : GridTracksMain[MaxFrozenHeadMain - 1];
            return prevTrack == null ? -ScrollOffsetMain : new LogicalMainTrack(prevTrack).EndPosition;
        }

        private static void AnalyzeLineFigurePosition(IGridTrackCollection gridTracks, int gridPoint, GridPointPlacement placement,
            out GridTrack prevGridTrack, out GridTrack nextGridTrack, out bool isHead, out bool isRepeat, out bool isTail)
        {
            prevGridTrack = GetPrevGridTrack(gridTracks, gridPoint, placement);
            nextGridTrack = GetNextGridTrack(gridTracks, gridPoint, placement);

            var containerStart = gridTracks.ContainerStart.Ordinal;
            var containerEnd = gridTracks.ContainerEnd.Ordinal + 1;
            isHead = gridPoint <= containerStart;
            isRepeat = gridPoint >= containerStart && gridPoint <= containerEnd;
            isTail = gridPoint >= containerEnd;

            if (isHead && isRepeat)
            {
                if (prevGridTrack == null)
                    isHead = false;
                else if (nextGridTrack == null)
                    isRepeat = false;
            }

            if (isRepeat && isTail)
            {
                if (prevGridTrack == null)
                    isRepeat = false;
                else if (nextGridTrack == null)
                    isTail = false;
            }
        }

        private IEnumerable<double> GetLineFigurePositionsCross(int gridPointCross, GridPointPlacement placement)
        {
            GridTrack prevGridTrack, nextGridTrack;
            bool isHead, isRepeat, isTail;
            AnalyzeLineFigurePosition(GridTracksCross, gridPointCross, placement, out prevGridTrack, out nextGridTrack, out isHead, out isRepeat, out isTail);

            GridTrack gridTrack;
            if (isHead)
            {
                var value = GetHeadPositionCross(prevGridTrack, nextGridTrack, out gridTrack);
                if (!Clip.IsHeadClipped(value, GetMinClipCross(gridTrack)))
                    yield return value;
            }

            if (isRepeat)
            {
                var first = 0;
                if (isHead)
                    first += 1;
                var last = FlowCount - 1;
                if (isTail)
                    last -= 1;
                for (int i = first; i <= last; i++)
                {
                    var value = GetRepeatPositionCross(prevGridTrack, nextGridTrack, i, out gridTrack);
                    if (!Clip.IsClipped(value, GetMinClipCross(gridTrack), GetMaxClipCross(gridTrack, i)))
                        yield return value;
                }
            }

            if (isTail)
            {
                var value = GetTailPositionCross(prevGridTrack, nextGridTrack, out gridTrack);
                if (!Clip.IsTailClipped(value, GetMaxClipCross(gridTrack)))
                    yield return value;
            }
        }

        private double GetHeadPositionCross(GridTrack prevGridTrack, GridTrack nextGridTrack, out GridTrack gridTrack)
        {
            if (prevGridTrack != null)
            {
                gridTrack = prevGridTrack;
                return GetEndPositionCross(gridTrack, 0);
            }
            else
            {
                gridTrack = nextGridTrack;
                return GetStartPositionCross(gridTrack, 0);
            }
        }

        private double GetRepeatPositionCross(GridTrack prevGridTrack, GridTrack nextGridTrack, int flowIndex, out GridTrack gridTrack)
        {
            if (prevGridTrack != null && prevGridTrack.IsRepeat)
            {
                gridTrack = prevGridTrack;
                return GetEndPositionCross(gridTrack, flowIndex);
            }
            else
            {
                gridTrack = nextGridTrack;
                return GetStartPositionCross(gridTrack, flowIndex);
            }
        }

        private double GetTailPositionCross(GridTrack prevGridTrack, GridTrack nextGridTrack, out GridTrack gridTrack)
        {
            if (nextGridTrack != null)
            {
                gridTrack = nextGridTrack;
                return GetStartPositionCross(gridTrack, 0);
            }
            else
            {
                gridTrack = prevGridTrack;
                return GetEndPositionCross(gridTrack, gridTrack.IsRepeat ? FlowCount - 1 : 0);
            }
        }

        protected IEnumerable<LineFigure> GetLineFiguresCross(int startGridPointCross, int endGridPointCross, GridPointPlacement placement, int gridPointMain)
        {
            var spanCross = GetLineFigureSpanCross(startGridPointCross, endGridPointCross);
            if (spanCross.Length <= 0)
                yield break;

            foreach (var positionMain in GetLineFigurePositionsMain(gridPointMain, placement))
                yield return new LineFigure(ToPoint(positionMain, spanCross.Start), ToPoint(positionMain, spanCross.End));
        }

        private Span GetLineFigureSpanCross(int startGridPointCross, int endGridPointCross)
        {
            var startTrackCross = GridTracksCross[startGridPointCross];
            var endTrackCross = GridTracksCross[endGridPointCross - 1];
            var startPositionCross = GetStartPositionCross(startTrackCross, 0);
            var endPositionCross = GetEndPositionCross(endTrackCross, endTrackCross.IsRepeat ? FlowCount - 1 : 0);
            if (endPositionCross <= startPositionCross)
                return new Span();

            var clip = GetClipCross(startPositionCross, endPositionCross, new GridSpan(startTrackCross, endTrackCross));
            if (double.IsPositiveInfinity(clip.Head) || double.IsPositiveInfinity(clip.Tail))
                return new Span();
            startPositionCross += clip.Head;
            endPositionCross -= clip.Tail;
            if (endPositionCross <= startPositionCross)
                return new Span();

            return new Span(startPositionCross, endPositionCross);
        }

        private IEnumerable<double> GetLineFigurePositionsMain(int gridOrdinalMain, GridPointPlacement placement)
        {
            GridTrack prevGridTrack, nextGridTrack;
            bool isHead, isRepeat, isTail;
            AnalyzeLineFigurePosition(GridTracksMain, gridOrdinalMain, placement, out prevGridTrack, out nextGridTrack, out isHead, out isRepeat, out isTail);
            bool isStretch = gridOrdinalMain == MinStretchGridOrdinal && StretchGap.HasValue;

            GridTrack gridTrack;

            if (isHead)
            {
                var value = GetPositionMain(prevGridTrack, nextGridTrack, null, true, out gridTrack);
                if (!Clip.IsHeadClipped(value, GetMinClipMain(gridTrack)))
                    yield return value;
            }

            if (isRepeat)
            {
                var first = 0;
                if (isHead)
                    first += 1;
                var last = ContainerViewList.Count - 1;
                if (last == MaxContainerCount - 1 && isTail && !isStretch)
                    last -= 1;
                for (int i = first; i <= last; i++)
                {
                    var containerView = ContainerViewList[i];
                    var value = GetPositionMain(prevGridTrack, nextGridTrack, containerView, (prevGridTrack != null && prevGridTrack.IsRepeat), out gridTrack);
                    if (!Clip.IsClipped(value, GetMinClipMain(gridTrack), GetMaxClipMain(gridTrack)))
                        yield return value;
                }
            }

            if (isTail)
            {
                if (isStretch && !prevGridTrack.IsRepeat)
                {
                    var valuePrev = GetPositionMain(prevGridTrack, nextGridTrack, null, true, out gridTrack);
                    if (!Clip.IsTailClipped(valuePrev, GetMaxClipMain(gridTrack)))
                        yield return valuePrev;
                }

                var value = GetPositionMain(prevGridTrack, nextGridTrack, null, false, out gridTrack);
                if (!Clip.IsTailClipped(value, GetMaxClipMain(gridTrack)))
                    yield return value;
            }
        }

        private double GetPositionMain(GridTrack prevGridTrack, GridTrack nextGridTrack, ContainerView containerView, bool preferPrevGridTrack, out GridTrack gridTrack)
        {
            if (nextGridTrack == null || (preferPrevGridTrack && prevGridTrack != null))
            {
                gridTrack = prevGridTrack;
                var logicalMainTrack = containerView == null ? new LogicalMainTrack(gridTrack) : new LogicalMainTrack(gridTrack, containerView);
                return logicalMainTrack.EndPosition;
            }
            else
            {
                gridTrack = nextGridTrack;
                var logicalMainTrack = containerView == null ? new LogicalMainTrack(gridTrack) : new LogicalMainTrack(gridTrack, containerView);
                return logicalMainTrack.StartPosition;
            }
        }

        private VariantLengthHandler _variantLengthHandler;

        private VariantLengthHandler GetVariantLengthHandler()
        {
            Debug.Assert(GridTracksMain.VariantByContainer);
            if (_variantLengthHandler == null)
                _variantLengthHandler = new VariantLengthHandler(this);
            return _variantLengthHandler;
        }

        private bool IsVariantLength(ContainerView containerView, GridTrack gridTrack)
        {
            return containerView != null && gridTrack.VariantByContainer;
        }

        protected sealed override double GetMeasuredLength(ContainerView containerView, GridTrack gridTrack)
        {
            return IsVariantLength(containerView, gridTrack) 
                ? GetVariantLengthHandler().GetMeasuredLength(containerView, gridTrack)
                : base.GetMeasuredLength(containerView, gridTrack);
        }

        protected sealed override void SetMeasuredAutoLength(ContainerView containerView, GridTrack gridTrack, double value)
        {
            if (IsVariantLength(containerView, gridTrack))
                GetVariantLengthHandler().SetMeasuredLength(containerView, gridTrack, value);
            else
                base.SetMeasuredAutoLength(containerView, gridTrack, value);
        }

        private double GetEnsureVisibleOffsetMain(LogicalMainTrack startGridOffset, LogicalMainTrack endGridOffset)
        {
            if (startGridOffset.GridTrack.IsFrozenHead || endGridOffset.GridTrack.IsFrozenTail)
                return 0;

            var startExtent = startGridOffset.StartExtent;
            var scrollStartMain = ScrollStartMain;
            if (startExtent < scrollStartMain)
                return startExtent - scrollStartMain;

            var end = endGridOffset.EndExtent - ScrollOffsetMain;
            var scrollEnd = ViewportMain - FrozenTailLengthMain;
            if (end > scrollEnd)
                return end - scrollEnd;

            return 0;
        }

        private double GetEnsureVisibleOffsetCross(GridTrack startGridTrack, int startFlowIndex, GridTrack endGridTrack, int endFlowIndex)
        {
            if (startGridTrack.IsFrozenHead || endGridTrack.IsFrozenTail)
                return 0;

            var start = startGridTrack.StartOffset;
            if (startFlowIndex > 0)
                start += startFlowIndex * FlowLength;
            var scrollStart = ScrollOffsetCross + FrozenHeadLengthCross;
            if (start < scrollStart)
                return start - scrollStart;

            var end = endGridTrack.EndOffset - ScrollOffsetCross;
            if (endFlowIndex > 0)
                end += endFlowIndex * FlowLength;
            var scrollEnd = ViewportCross - FrozenTailLengthCross;
            if (end > scrollEnd)
                return end - scrollEnd;

            return 0;
        }

        public void EnsureVisible(DependencyObject visual)
        {
            for (; visual != null; visual = VisualTreeHelper.GetParent(visual))
            {
                var element = visual as UIElement;
                if (element == null)
                    continue;

                var binding = element.GetBinding();
                if (binding == null)
                {
                    var rowView = visual as RowView;
                    if (rowView != null)
                    {
                        EnsureVisible(rowView);
                        return;
                    }
                    continue;
                }

                if (binding.ParentBinding != null)
                    continue;

                var scalarBinding = binding as ScalarBinding;
                if (scalarBinding != null)
                {
                    for (int i = 0; i < scalarBinding.FlowCount; i++)
                    {
                        if (visual == scalarBinding[i])
                        {
                            EnsureVisible(scalarBinding, i);
                            return;
                        }
                    }
                }

                var rowBinding = binding as RowBinding;
                if (rowBinding != null)
                {
                    var rowView = element.GetRowPresenter().View;
                    EnsureVisible(rowView, rowBinding);
                    return;
                }

                var blockBinding = binding as BlockBinding;
                if (blockBinding != null)
                {
                    var blockView = element.GetBlockView();
                    EnsureVisible(blockView, blockBinding);
                    return;
                }
            }
        }

        private void EnsureVisible(ScalarBinding scalarBinding, int flowIndex)
        {
            SetScrollOffsetMain(ScrollOffsetMain + GetEnsureVisibleOffsetMain(scalarBinding), true);
            SetScrollOffsetCross(ScrollOffsetCross + GetEnsureVisibleOffsetCross(scalarBinding, flowIndex), true);
        }

        private double GetEnsureVisibleOffsetMain(ScalarBinding scalarBinding)
        {
            var gridRange = scalarBinding.GridRange;
            return GetEnsureVisibleOffsetMain(GetStartLogicalMainTrack(gridRange), GetEndLogicalMainTrack(gridRange));
        }

        private double GetEnsureVisibleOffsetCross(ScalarBinding scalarBinding, int flowIndex)
        {
            var gridSpan = GridTracksCross.GetGridSpan(scalarBinding.GridRange);
            var endFlowIndex = ShouldStretchCross(scalarBinding) ? FlowCount - 1 : flowIndex;
            return GetEnsureVisibleOffsetCross(gridSpan.StartTrack, flowIndex, gridSpan.EndTrack, endFlowIndex);
        }

        private void EnsureVisible(RowView rowView)
        {
            SetScrollOffsetMain(ScrollOffsetMain + GetEnsureVisibleOffsetMain(rowView), true);
            SetScrollOffsetCross(ScrollOffsetCross + GetEnsureVisibleOffsetCross(rowView), true);
        }

        private double GetEnsureVisibleOffsetMain(RowView rowView)
        {
            var gridSpan = GridTracksMain.GetGridSpan(Template.RowRange);
            var containerOrdinal = rowView.ContainerOrdinal;
            return GetEnsureVisibleOffsetMain(new LogicalMainTrack(gridSpan.StartTrack, containerOrdinal), new LogicalMainTrack(gridSpan.EndTrack, containerOrdinal));
        }

        private double GetEnsureVisibleOffsetCross(RowView rowView)
        {
            var gridSpan = GridTracksCross.GetGridSpan(Template.RowRange);
            var flowIndex = rowView.FlowIndex;
            return GetEnsureVisibleOffsetCross(gridSpan.StartTrack, flowIndex, gridSpan.EndTrack, flowIndex);
        }

        private void EnsureVisible(RowView rowView, RowBinding rowBinding)
        {
            SetScrollOffsetMain(ScrollOffsetMain + GetEnsureVisibleOffsetMain(rowView, rowBinding), true);
            SetScrollOffsetCross(ScrollOffsetCross + GetEnsureVisibleOffsetCross(rowView, rowBinding), true);
        }

        private double GetEnsureVisibleOffsetMain(RowView rowView, RowBinding rowBinding)
        {
            var gridSpan = GridTracksMain.GetGridSpan(rowBinding.GridRange);
            var containerOrdinal = rowView.ContainerOrdinal;
            return GetEnsureVisibleOffsetMain(new LogicalMainTrack(gridSpan.StartTrack, containerOrdinal), new LogicalMainTrack(gridSpan.EndTrack, containerOrdinal));
        }

        private double GetEnsureVisibleOffsetCross(RowView rowView, RowBinding rowBinding)
        {
            var gridSpan = GridTracksCross.GetGridSpan(rowBinding.GridRange);
            var flowIndex = rowView.FlowIndex;
            return GetEnsureVisibleOffsetCross(gridSpan.StartTrack, flowIndex, gridSpan.EndTrack, flowIndex);
        }

        private void EnsureVisible(BlockView blockView, BlockBinding blockBinding)
        {
            SetScrollOffsetMain(ScrollOffsetMain + GetEnsureVisibleOffsetMain(blockView, blockBinding), true);
            SetScrollOffsetCross(ScrollOffsetCross + GetEnsureVisibleOffsetCross(blockView, blockBinding), true);
        }

        private double GetEnsureVisibleOffsetMain(BlockView blockView, BlockBinding blockBinding)
        {
            var gridSpan = GridTracksMain.GetGridSpan(blockBinding.GridRange);
            var blockOrdinal = blockView.ContainerOrdinal;
            return GetEnsureVisibleOffsetMain(new LogicalMainTrack(gridSpan.StartTrack, blockOrdinal), new LogicalMainTrack(gridSpan.EndTrack, blockOrdinal));
        }

        private double GetEnsureVisibleOffsetCross(BlockView blockView, BlockBinding blockBinding)
        {
            var gridSpan = GridTracksCross.GetGridSpan(blockBinding.GridRange);
            var flowIndex = gridSpan.EndTrack.Ordinal > GridTracksCross.GetGridSpan(Template.RowRange).EndTrack.Ordinal ? FlowCount : 0;
            return GetEnsureVisibleOffsetCross(gridSpan.StartTrack, flowIndex, gridSpan.EndTrack, flowIndex);
        }
    }
}
