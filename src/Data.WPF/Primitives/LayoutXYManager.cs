using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    // ================================================
    // COORDINATES SYSTEM CONCEPT AND NAME CONVENTIONS:
    // ================================================
    // * Main(axis): the axis where ContainerViews are arranged.
    // * Cross(axis): the axis cross to the main axis.
    // * Offset(coordinate): coordinate before scrolled/frozen.
    // * Location(coordinate): coordinate after scrolled/frozen.
    // - Coordinate + Axis combination are used, for example: OffsetMain, OffsetCross, LocationMain, LocationCross
    // * GridOffset: the (GridTrack, ContainerView) pair to uniquely identify the grid on the main axis, can be converted to/from an int index value.
    internal abstract partial class LayoutXYManager : LayoutManager, IScrollHandler
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Derived classes are limited to class LayoutXManager/LayoutYManager, and the overrides do not rely on completion of its constructor.")]
        protected LayoutXYManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, Func<IEnumerable<ValidationMessage<Scalar>>> validateScalars)
            : base(template, dataSet, where, orderBy, false, validateScalars)
        {
            _scrollStartMain = ScrollOriginMain;
        }

        internal abstract IGridTrackCollection GridTracksMain { get; }
        internal abstract IGridTrackCollection GridTracksCross { get; }

        internal GridSpan VariantByContainerGridSpan
        {
            get { return GridTracksMain.VariantByContainer ? GridTracksMain.GetGridSpan(Template.RowRange) : new GridSpan(); }
        }

        private bool _isContainerLengthsValid = true;
        internal void InvalidateContainerLengths()
        {
            _isContainerLengthsValid = false;
        }

        internal void RefreshContainerLengths()
        {
            if (_isContainerLengthsValid)
                return;

            _isContainerLengthsValid = true; // Avoid re-entrance
            for (int i = 1; i < ContainerViewList.Count; i++)
                ContainerViewList[i].StartOffset = ContainerViewList[i - 1].EndOffset;

            var gridSpan = VariantByContainerGridSpan;
            if (gridSpan.IsEmpty)
                return;

            for (int i = 0; i < gridSpan.Count; i++)
            {
                var gridTrack = gridSpan[i];
                double totalLength = 0;
                for (int j = 0; j < ContainerViewList.Count; j++)
                    totalLength += ContainerViewList[j].GetMeasuredLength(gridTrack);
                gridTrack.VariantByContainerAvgLength = ContainerViewList.Count == 0 ? 1 : totalLength / ContainerViewList.Count;
            }

            for (int i = 1; i < gridSpan.Count; i++)
                gridSpan[i].VariantByContainerStartOffset = gridSpan[i - 1].VariantByContainerEndOffset;
        }

        private void InitContainerViews()
        {
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

            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            if (gridOffset.IsEof)
                return MaxContainerCount - 1;

            var gridTrack = gridOffset.GridTrack;
            if (gridTrack.IsHead)
                return 0;
            else if (gridTrack.IsRepeat)
                return gridOffset.Ordinal;
            else
                return MaxContainerCount - 1;
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
            var scrollOriginMain = ScrollOriginMain;
            if (_scrollStartMain.Value < scrollOriginMain.Value)
                _scrollStartMain = scrollOriginMain;
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

        private int MaxGridOffsetMain
        {
            get { return MaxFrozenHeadMain + TotalContainerGridTracksMain + MaxFrozenTailMain; }
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

        protected override void OnRowsChanged()
        {
            base.OnRowsChanged();
            InvalidateMeasure();
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

        protected sealed override void PrepareMeasureContainers()
        {
            InitContainerViews();

            if (DeltaScrollOffset < 0 || _scrollStartMain.GridOffset >= MaxGridOffsetMain)
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
            get { return MaxFrozenTailMain == 0 ? MaxOffsetMain : GetGridOffset(MaxGridOffsetMain - MaxFrozenTailMain).Span.Start; }
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
            get { return MaxFrozenTailMain == 0 ? 0 : MaxOffsetMain - GetGridOffset(MaxGridOffsetMain - MaxFrozenTailMain).Span.Start; }
        }

        private double GapToFill
        {
            get
            {
                var availableLength = GridTracksMain.AvailableLength;
                if (double.IsPositiveInfinity(availableLength))
                    return availableLength;

                var scrollable = availableLength - (FrozenHeadLengthMain + FrozenTailLengthMain);
                var endOffset = ContainerViewList.Count == 0 ? GridTracksMain[MaxFrozenHeadMain].StartOffset : GetEndOffset(ContainerViewList[ContainerViewList.Count - 1]);
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

            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            if (gridOffset.IsEof)
            {
                MeasureBackwardEof(availableLength, flagScrollBack);
                return;
            }

            var gridTrack = gridOffset.GridTrack;
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
            var scrollLength = Math.Min(availableLength, ScrollStartMain - GetStartOffset(containerView));
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
            Debug.Assert(measuredLength >= 0);
            if (MaxContainerCount > 0)
                MeasureForwardRepeat(new GridOffset(GridTracksMain.ContainerStart, 0), 0, availableLength - measuredLength);
        }

        private void MeasureForwardRepeat(GridOffset gridOffset, double fraction, double availableLength)
        {
            Debug.Assert(ContainerViewList.Count == 1);
            if (FrozenTailMain > 0)
                availableLength -= FrozenTailLengthMain;

            var gridTrack = gridOffset.GridTrack;
            Debug.Assert(gridTrack.IsRepeat);
            var containerView = ContainerViewList[0];
            Debug.Assert(containerView.ContainerOrdinal == gridOffset.Ordinal);
            availableLength -= GetLengthMain(containerView) - GetRelativeOffsetMain(containerView, gridTrack, fraction);
            RealizeForward(availableLength);
        }

        private double GetRelativeOffsetMain(ContainerView containerView, GridTrack gridTrack)
        {
            Debug.Assert(GridTracksMain.GetGridSpan(Template.BlockRange).Contains(gridTrack));
            return GetStartLocationMain(new GridOffset(gridTrack, containerView)) - GetStartLocationMain(containerView);
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
            var startGridOffset = GetStartGridOffset(gridRange);
            var endGridOffset = GetEndGridOffset(gridRange);
            return startGridOffset == endGridOffset ? startGridOffset.Span.Length : endGridOffset.Span.End - startGridOffset.Span.Start;
        }

        protected override Point GetLocation(ScalarBinding scalarBinding, int blockDimension)
        {
            var valueMain = GetStartLocationMain(scalarBinding);
            var valueCross = GetStartLocationCross(scalarBinding, blockDimension);
            return ToPoint(valueMain, valueCross);
        }

        private double GetStartLocationMain(ScalarBinding scalarBinding)
        {
            var startGridOffset = GetStartGridOffset(scalarBinding.GridRange);
            return GetStartLocationMain(startGridOffset);
        }

        private int MinStretchGridOrdinal
        {
            get { return GridTracksMain.Count - Template.Stretches; }
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
                if (gridTrack.Ordinal >= MinStretchGridOrdinal)
                    result = maxValueMain;
                else if (result > maxValueMain)
                    result = maxValueMain;
            }

            return result;
        }

        private double GetEndLocationMain(GridOffset gridOffset)
        {
            return gridOffset.IsEof ? GetStartLocationMain(gridOffset) : GetStartLocationMain(gridOffset) + gridOffset.Span.Length;
        }

        private double GetStartLocationCross(ScalarBinding scalarBinding, int blockDimension)
        {
            return GetStartLocationCross(scalarBinding.GridRange, blockDimension);
        }

        private double GetLengthCross(ScalarBinding scalarBinding)
        {
            var gridRange = scalarBinding.GridRange;
            var startLocation = GetStartLocationCross(gridRange, 0);
            var endLocation = GetEndLocationCross(gridRange, ShouldStretchCross(scalarBinding) ? BlockDimensions - 1 : 0);
            return endLocation - startLocation;
        }

        private bool ShouldStretchCross(ScalarBinding scalarBinding)
        {
            if (BlockDimensions > 1 && !scalarBinding.IsMultidimensional)
            {
                var rowSpan = GridTracksCross.GetGridSpan(Template.RowRange);
                var scalarBindingSpan = GridTracksCross.GetGridSpan(scalarBinding.GridRange);
                if (rowSpan.Contains(scalarBindingSpan))
                    return true;
            }
            return false;
        }

        private Clip GetClipMain(double startLocation, double endLocation, Binding binding)
        {
            return GetClipMain(startLocation, endLocation, binding.GridRange);
        }

        private Clip GetClipMain(double startLocation, double endLocation, GridRange gridRange)
        {
            var gridSpan = GridTracksMain.GetGridSpan(gridRange);
            return GetClipMain(startLocation, endLocation, gridSpan);
        }

        private Clip GetClipMain(double startLocation, double endLocation, GridSpan gridSpan)
        {
            double? minStart = GetMinClipMain(gridSpan.StartTrack);
            double? maxEnd = GetMaxClipMain(gridSpan.EndTrack);
            return new Clip(startLocation, endLocation, minStart, maxEnd);
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

        private Clip GetClipCross(double startLocation, double endLocation, Binding binding, Clip containerClip)
        {
            return GetClipCross(startLocation, endLocation, binding.GridRange, containerClip);
        }

        private Clip GetClipCross(double startLocation, double endLocation, GridRange gridRange, Clip containerClip, int? blockDimension = null)
        {
            var gridSpan = GridTracksCross.GetGridSpan(gridRange);
            return GetClipCross(startLocation, endLocation, gridSpan, containerClip, blockDimension);
        }

        private Clip GetClipCross(double startLocation, double endLocation, GridSpan gridSpan, Clip containerClip = new Clip(), int? blockDimension = null)
        {
            double? minStart = GetMinClipCross(gridSpan.StartTrack, containerClip);
            double? maxEnd = GetMaxClipCross(gridSpan.EndTrack, blockDimension, containerClip);
            return new Clip(startLocation, endLocation, minStart, maxEnd);
        }

        private double? GetMinClipCross(GridTrack gridTrack, Clip containerClip = new Clip())
        {
            return FrozenHeadCross == 0 || containerClip.Head > 0 || gridTrack.IsFrozenHead ? new double?() : FrozenHeadLengthCross;
        }

        private double? GetMaxClipCross(GridTrack gridTrack, int? blockDimension = null, Clip containerClip = new Clip())
        {
            return FrozenTailCross == 0 || containerClip.Tail > 0 || IsFrozenTailCross(gridTrack, blockDimension)
                ? new double?() : ViewportCross - FrozenTailLengthCross;
        }

        private bool IsFrozenTailCross(GridTrack gridTrack, int? blockDimension)
        {
            Debug.Assert(gridTrack.Owner == GridTracksCross);
            return !blockDimension.HasValue ? gridTrack.IsFrozenTail : blockDimension.Value == BlockDimensions - 1 && gridTrack.IsFrozenTail;
        }

        internal override Thickness GetClip(ScalarBinding scalarBinding, int blockDimension)
        {
            var clipMain = GetClipMain(scalarBinding);
            var clipCross = GetClipCross(scalarBinding, blockDimension);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetClipMain(ScalarBinding scalarBinding)
        {
            var startLocation = GetStartLocationMain(scalarBinding);
            var endLocation = startLocation + GetLengthMain(scalarBinding);
            return GetClipMain(startLocation, endLocation, scalarBinding);
        }

        private Clip GetClipCross(ScalarBinding scalarBinding, int blockDimension)
        {
            var startLocation = GetStartLocationCross(scalarBinding, blockDimension);
            var endLocation = startLocation + GetLengthCross(scalarBinding);
            return GetClipCross(startLocation, endLocation, scalarBinding, new Clip());
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

        private double GetMeasuredLengthMain(ContainerView containerView, GridRange gridRange)
        {
            var gridSpan = GridTracksMain.GetGridSpan(gridRange);
            var startTrack = gridSpan.StartTrack;
            var endTrack = gridSpan.EndTrack;
            return startTrack == endTrack ? new GridOffset(startTrack, containerView).Span.Length
                : new GridOffset(endTrack, containerView).Span.End - new GridOffset(startTrack, containerView).Span.Start;
        }

        private GridOffset GetStartGridOffset(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).StartTrack;
            return GetStartGridOffset(gridTrack);
        }

        private GridOffset GetStartGridOffset(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Owner == GridTracksMain);
            if (!gridTrack.IsRepeat)
                return new GridOffset(gridTrack);

            if (MaxContainerCount > 0)
                return new GridOffset(gridTrack, 0);
            else
                return MaxFrozenTailMain > 0 ? new GridOffset(GridTracksMain.LastOf(MaxFrozenTailMain)) : GridOffset.Eof;
        }

        private GridOffset GetEndGridOffset(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).EndTrack;
            return GetEndGridOffset(gridTrack);
        }

        private GridOffset GetEndGridOffset(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Owner == GridTracksMain);
            if (!gridTrack.IsRepeat)
                return new GridOffset(gridTrack);

            if (MaxContainerCount > 0)
                return new GridOffset(gridTrack, MaxContainerCount - 1);
            else
                return MaxFrozenTailMain > 0 ? new GridOffset(GridTracksMain.LastOf(MaxFrozenTailMain)) : GridOffset.Eof;
        }

        protected override Point GetLocation(ContainerView containerView)
        {
            var valueMain = GetStartLocationMain(containerView);
            var valueCross = GetContainerStartLocationCross();
            return ToPoint(valueMain, valueCross);
        }

        private double GetStartLocationMain(ContainerView containerView)
        {
            var startTrack = GridTracksMain.GetGridSpan(Template.BlockRange).StartTrack;
            return GetStartLocationMain(new GridOffset(startTrack, containerView));
        }

        private double GetContainerStartLocationCross()
        {
            return GetStartLocationCross(Template.BlockRange, 0);
        }

        private double GetContainerEndLocationCross()
        {
            return GetEndLocationCross(Template.BlockRange, BlockDimensions - 1);
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
            return GetContainerEndLocationCross() - GetContainerStartLocationCross();
        }

        internal override Thickness GetClip(ContainerView containerView)
        {
            var clipMain = GetClipMain(containerView);
            var clipCross = GetContainerClipCross();
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetClipMain(ContainerView containerView)
        {
            var startLocation = GetStartLocationMain(containerView);
            var endLocation = startLocation + GetLengthMain(containerView);
            return GetClipMain(startLocation, endLocation, Template.BlockRange);
        }

        private Clip GetContainerClipCross()
        {
            var startLocation = GetContainerStartLocationCross();
            var endLocation = GetContainerEndLocationCross();
            return GetClipCross(startLocation, endLocation, Template.BlockRange, new Clip());
        }

        private double GetStartOffset(ContainerView containerView)
        {
            return new GridOffset(GridTracksMain.ContainerStart, containerView).Span.Start;
        }

        private double GetEndOffset(ContainerView containerView)
        {
            return new GridOffset(GridTracksMain.ContainerEnd, containerView).Span.End;
        }

        protected override Point GetLocation(BlockView blockView, BlockBinding blockBinding)
        {
            var valueMain = GetStartLocationMain(blockView, blockBinding);
            var valueCross = GetStartLocationCross(blockView, blockBinding) - GetContainerStartLocationCross();
            return ToPoint(valueMain, valueCross);
        }

        private double GetStartLocationMain(BlockView blockView, BlockBinding blockBinding)
        {
            var startTrack = GridTracksMain.GetGridSpan(blockBinding.GridRange).StartTrack;
            return GetRelativeOffsetMain(blockView, startTrack);
        }

        protected override Size GetSize(BlockView blockView, BlockBinding blockBinding)
        {
            var valueMain = GetMeasuredLengthMain(blockView, blockBinding.GridRange);
            var valueCross = GetEndLocationCross(blockView, blockBinding) - GetStartLocationCross(blockView, blockBinding);
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

        private double GetStartLocationCross(BlockView blockView, BlockBinding blockBinding)
        {
            return GetStartLocationCross(blockBinding.GridRange, IsHead(blockBinding) ? 0 : BlockDimensions - 1);
        }

        private double GetEndLocationCross(BlockView blockView, BlockBinding blockBinding)
        {
            return GetEndLocationCross(blockBinding.GridRange, IsHead(blockBinding) ? 0 : BlockDimensions - 1);
        }

        private Clip GetClipCross(BlockView blockView, BlockBinding blockBinding)
        {
            var startLocation = GetStartLocationCross(blockView, blockBinding);
            var endLocation = GetEndLocationCross(blockView, blockBinding);
            return GetClipCross(startLocation, endLocation, blockBinding, GetContainerClipCross());
        }

        protected override Point GetLocation(BlockView blockView, int blockDimension)
        {
            var valueCross = GetStartLocationCross(blockDimension) - GetContainerStartLocationCross();
            return ToPoint(0, valueCross);
        }

        protected override Size GetSize(BlockView blockView, int blockDimension)
        {
            var valueMain = GetMeasuredLengthMain(blockView, Template.RowRange);
            var valueCross = GetEndLocationCross(blockDimension) - GetStartLocationCross(blockDimension);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetClip(int blockDimension)
        {
            var clipMain = new Clip();
            var clipCross = GetClipCross(blockDimension);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetClipCross(int blockDimension)
        {
            var startLocation = GetStartLocationCross(blockDimension);
            var endLocation = GetEndLocationCross(blockDimension);
            return GetClipCross(startLocation, endLocation, Template.RowRange, GetContainerClipCross(), blockDimension);
        }

        private double GetStartLocationCross(int blockDimension)
        {
            var rowRange = Template.RowRange;
            var result = GetStartLocationCross(rowRange, blockDimension);
            if (blockDimension == BlockDimensions - 1 && GridTracksCross.GetGridSpan(rowRange).EndTrack.IsFrozenTail)
                result = Math.Min(ViewportCross - FrozenTailLengthCross, result);
            return result;
        }

        private double GetEndLocationCross(int blockDimension)
        {
            return GetEndLocationCross(Template.RowRange, blockDimension);
        }

        protected override Point GetLocation(RowView rowView, RowBinding rowBinding)
        {
            var valueMain = GetStartLocationMain(rowView, rowBinding);
            var valueCross = GetStartLocationCross(rowView, rowBinding) - GetStartLocationCross(rowView.BlockDimension);
            return ToPoint(valueMain, valueCross);
        }

        private double GetStartLocationMain(RowView rowView, RowBinding rowBinding)
        {
            var containerView = this[rowView];
            var startGridTrack = GridTracksMain.GetGridSpan(rowBinding.GridRange).StartTrack;
            return GetStartLocationMain(new GridOffset(startGridTrack, containerView)) - GetStartLocationMain(containerView);
        }

        protected override Size GetSize(RowView rowView, RowBinding rowBinding)
        {
            var valueMain = GetMeasuredLengthMain(this[rowView], rowBinding.GridRange);
            var valueCross = GetEndLocationCross(rowView, rowBinding) - GetStartLocationCross(rowView, rowBinding);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetClip(RowView rowView, RowBinding rowBinding)
        {
            var clipMain = new Clip();
            var clipCross = GetClipCross(rowView, rowBinding);
            return ToThickness(clipMain, clipCross);
        }

        private double GetStartLocationCross(RowView rowView, RowBinding rowBinding)
        {
            return GetStartLocationCross(rowBinding.GridRange, rowView.BlockDimension);
        }

        private double GetEndLocationCross(RowView rowView, RowBinding rowBinding)
        {
            return GetEndLocationCross(rowBinding.GridRange, rowView.BlockDimension);
        }

        private Clip GetClipCross(RowView rowView, RowBinding rowBinding)
        {
            var startLocation = GetStartLocationCross(rowView, rowBinding);
            var endLocation = GetEndLocationCross(rowView, rowBinding);
            var containerClip = GetContainerClipCross().Merge(GetClipCross(rowView.BlockDimension));
            return GetClipCross(startLocation, endLocation, rowBinding, containerClip);
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
                    foreach (var lineFigure in GetLineFigures(gridLine))
                        yield return new GridLineFigure(gridLine, lineFigure);
                }
            }
        }

        private IEnumerable<LineFigure> GetLineFigures(GridLine gridLine)
        {
            return gridLine.Orientation == Orientation.Horizontal
                ? GetLineFiguresX(gridLine.StartGridPoint.X, gridLine.EndGridPoint.X, gridLine.Position, gridLine.StartGridPoint.Y)
                : GetLineFiguresY(gridLine.StartGridPoint.Y, gridLine.EndGridPoint.Y, gridLine.Position, gridLine.StartGridPoint.X);
        }

        protected abstract IEnumerable<LineFigure> GetLineFiguresX(int startGridOrdinalX, int endGridOrdinalX, GridLinePosition position, int gridOrdinalY);

        protected abstract IEnumerable<LineFigure> GetLineFiguresY(int startGridOrdinalY, int endGridOrdinalY, GridLinePosition position, int gridOrdinalX);

        private static GridTrack GetPrevGridTrack(IReadOnlyList<GridTrack> gridTracks, int gridOrdinal, GridLinePosition position)
        {
            if ((position & GridLinePosition.PreviousTrack) != GridLinePosition.PreviousTrack)
                return null;
            return gridOrdinal == 0 ? null : gridTracks[gridOrdinal - 1];
        }

        private static GridTrack GetNextGridTrack(IReadOnlyList<GridTrack> gridTracks, int gridOrdinal, GridLinePosition position)
        {
            if ((position & GridLinePosition.NextTrack) != GridLinePosition.NextTrack)
                return null;
            return gridOrdinal == gridTracks.Count ? null : gridTracks[gridOrdinal];
        }

        protected IEnumerable<LineFigure> GetLineFiguresMain(int startGridOrdinalMain, int endGridOrdinalMain, GridLinePosition position, int gridOrdinalCross)
        {
            var spansMain = GetLineFigureSpansMain(startGridOrdinalMain, endGridOrdinalMain);
            if (spansMain == null)
                yield break;

            foreach (var locationCross in GetLineFigureLocationsCross(gridOrdinalCross, position))
            {
                foreach (var spanMain in spansMain)
                    yield return new LineFigure(ToPoint(spanMain.Start, locationCross), ToPoint(spanMain.End, locationCross));
            }
        }

        private Span[] GetLineFigureSpansMain(int startGridOrdinalMain, int endGridOrdinalMain)
        {
            var startTrackMain = GridTracksMain[startGridOrdinalMain];
            var endTrackMain = GridTracksMain[endGridOrdinalMain - 1];
            var startLocationMain = GetStartLocationMain(GetStartGridOffset(startTrackMain));
            var endLocationMain = GetEndLocationMain(GetEndGridOffset(endTrackMain));
            if (endLocationMain <= startLocationMain)
                return null;

            var clip = GetClipMain(startLocationMain, endLocationMain, new GridSpan(startTrackMain, endTrackMain));
            if (double.IsPositiveInfinity(clip.Head) || double.IsPositiveInfinity(clip.Tail))
                return null;
            startLocationMain += clip.Head;
            endLocationMain -= clip.Tail;
            if (endLocationMain <= startLocationMain)
                return null;

            return new Span(startLocationMain, endLocationMain).Split(StretchGap);
        }

        private Span? StretchGap
        {
            get
            {
                const double Epsilon = 1e-8;

                if (Template.Stretches == 0)
                    return null;

                var minStretchGridOrdinal = MinStretchGridOrdinal;

                var startLocation = GetPrevTrackEndLocationMain(minStretchGridOrdinal);
                var endLocation = GetStartLocationMain(new GridOffset(GridTracksMain[minStretchGridOrdinal]));
                return startLocation + Epsilon < endLocation ? new Span(startLocation, endLocation) : default(Span?);
            }
        }

        private double GetPrevTrackEndLocationMain(int gridOrdinal)
        {
            var prevTrack = GridTracksMain[gridOrdinal - 1];
            if (!prevTrack.IsRepeat)
                return GetEndLocationMain(new GridOffset(prevTrack));

            if (ContainerViewList.Last != null)
                return GetEndLocationMain(new GridOffset(prevTrack, ContainerViewList.Last));

            prevTrack = MaxFrozenHeadMain == 0 ? null : GridTracksMain[MaxFrozenHeadMain - 1];
            return prevTrack == null ? -ScrollOffsetMain : GetEndLocationMain(new GridOffset(prevTrack));
        }

        private static void AnalyzeLineFigureLocation(IGridTrackCollection gridTracks, int gridOrdinal, GridLinePosition position,
            out GridTrack prevGridTrack, out GridTrack nextGridTrack, out bool isHead, out bool isRepeat, out bool isTail)
        {
            prevGridTrack = GetPrevGridTrack(gridTracks, gridOrdinal, position);
            nextGridTrack = GetNextGridTrack(gridTracks, gridOrdinal, position);

            var containerStart = gridTracks.ContainerStart.Ordinal;
            var containerEnd = gridTracks.ContainerEnd.Ordinal + 1;
            isHead = gridOrdinal <= containerStart;
            isRepeat = gridOrdinal >= containerStart && gridOrdinal <= containerEnd;
            isTail = gridOrdinal >= containerEnd;

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

        private IEnumerable<double> GetLineFigureLocationsCross(int gridOrdinalCross, GridLinePosition position)
        {
            GridTrack prevGridTrack, nextGridTrack;
            bool isHead, isRepeat, isTail;
            AnalyzeLineFigureLocation(GridTracksCross, gridOrdinalCross, position, out prevGridTrack, out nextGridTrack, out isHead, out isRepeat, out isTail);

            GridTrack gridTrack;
            if (isHead)
            {
                var value = GetLocationCross(prevGridTrack, nextGridTrack, 0, true, out gridTrack);
                if (!Clip.IsHeadClipped(value, GetMinClipCross(gridTrack)))
                    yield return value;
            }

            if (isRepeat)
            {
                var first = 0;
                if (isHead)
                    first += 1;
                var last = BlockDimensions - 1;
                if (isTail)
                    last -= 1;
                for (int i = first; i <= last; i++)
                {
                    var value = GetLocationCross(prevGridTrack, nextGridTrack, i, gridOrdinalCross <= FrozenHeadCross, out gridTrack);
                    if (!Clip.IsClipped(value, GetMinClipCross(gridTrack), GetMaxClipCross(gridTrack, i)))
                        yield return value;
                }
            }

            if (isTail)
            {
                var value = GetLocationCross(prevGridTrack, nextGridTrack, BlockDimensions - 1, false, out gridTrack);
                if (!Clip.IsTailClipped(value, GetMaxClipCross(gridTrack)))
                    yield return value;
            }
        }

        private double GetLocationCross(GridTrack prevGridTrack, GridTrack nextGridTrack, int blockDimension, bool preferPrevGridTrack, out GridTrack gridTrack)
        {
            if (nextGridTrack == null || (preferPrevGridTrack && prevGridTrack != null))
            {
                gridTrack = prevGridTrack;
                return GetEndLocationCross(prevGridTrack, blockDimension);
            }
            else
            {
                gridTrack = nextGridTrack;
                return GetStartLocationCross(nextGridTrack, blockDimension);
            }
        }

        protected IEnumerable<LineFigure> GetLineFiguresCross(int startGridOffsetCross, int endGridOffsetCross, GridLinePosition position, int gridOffsetMain)
        {
            var spanCross = GetLineFigureSpanCross(startGridOffsetCross, endGridOffsetCross);
            if (spanCross.Length <= 0)
                yield break;

            foreach (var locationMain in GetLineFigureLocationsMain(gridOffsetMain, position))
                yield return new LineFigure(ToPoint(locationMain, spanCross.Start), ToPoint(locationMain, spanCross.End));
        }

        private Span GetLineFigureSpanCross(int startGridOffsetCross, int endGridOffsetCross)
        {
            var startTrackCross = GridTracksCross[startGridOffsetCross];
            var endTrackCross = GridTracksCross[endGridOffsetCross - 1];
            var startLocationCross = GetStartLocationCross(startTrackCross, 0);
            var endLocationCross = GetEndLocationCross(endTrackCross, endTrackCross.IsHead ? 0 : BlockDimensions - 1);
            if (endLocationCross <= startLocationCross)
                return new Span();

            var clip = GetClipCross(startLocationCross, endLocationCross, new GridSpan(startTrackCross, endTrackCross));
            if (double.IsPositiveInfinity(clip.Head) || double.IsPositiveInfinity(clip.Tail))
                return new Span();
            startLocationCross += clip.Head;
            endLocationCross -= clip.Tail;
            if (endLocationCross <= startLocationCross)
                return new Span();

            return new Span(startLocationCross, endLocationCross);
        }

        private IEnumerable<double> GetLineFigureLocationsMain(int gridOrdinalMain, GridLinePosition position)
        {
            GridTrack prevGridTrack, nextGridTrack;
            bool isHead, isRepeat, isTail;
            AnalyzeLineFigureLocation(GridTracksMain, gridOrdinalMain, position, out prevGridTrack, out nextGridTrack, out isHead, out isRepeat, out isTail);
            bool isStretch = gridOrdinalMain == MinStretchGridOrdinal && StretchGap.HasValue;

            GridTrack gridTrack;

            if (isHead)
            {
                var value = GetLocationMain(prevGridTrack, nextGridTrack, null, true, out gridTrack);
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
                    var value = GetLocationMain(prevGridTrack, nextGridTrack, containerView, (prevGridTrack != null && prevGridTrack.IsRepeat), out gridTrack);
                    if (!Clip.IsClipped(value, GetMinClipMain(gridTrack), GetMaxClipMain(gridTrack)))
                        yield return value;
                }
            }

            if (isTail)
            {
                if (isStretch && !prevGridTrack.IsRepeat)
                {
                    var valuePrev = GetLocationMain(prevGridTrack, nextGridTrack, null, true, out gridTrack);
                    if (!Clip.IsTailClipped(valuePrev, GetMaxClipMain(gridTrack)))
                        yield return valuePrev;
                }

                var value = GetLocationMain(prevGridTrack, nextGridTrack, null, false, out gridTrack);
                if (!Clip.IsTailClipped(value, GetMaxClipMain(gridTrack)))
                    yield return value;
            }
        }

        private double GetLocationMain(GridTrack prevGridTrack, GridTrack nextGridTrack, ContainerView containerView, bool preferPrevGridTrack, out GridTrack gridTrack)
        {
            if (nextGridTrack == null || (preferPrevGridTrack && prevGridTrack != null))
            {
                gridTrack = prevGridTrack;
                var gridOffset = containerView == null ? new GridOffset(gridTrack) : new GridOffset(gridTrack, containerView);
                return GetEndLocationMain(gridOffset);
            }
            else
            {
                gridTrack = nextGridTrack;
                var gridOffset = containerView == null ? new GridOffset(gridTrack) : new GridOffset(gridTrack, containerView);
                return GetStartLocationMain(gridOffset);
            }
        }
    }
}
