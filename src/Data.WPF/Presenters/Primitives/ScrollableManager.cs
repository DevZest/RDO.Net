using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Presenters.Primitives
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
        protected ScrollableManager(Template template, DataSet dataSet, IReadOnlyList<Column> rowMatchColumns, Predicate<DataRow> where, IComparer<DataRow> orderBy)
            : base(template, dataSet, rowMatchColumns, where, orderBy, false)
        {
            _scrollToMain = MinScrollToMain;
            _scrollToMainPlacement = GridPlacement.Head;
        }

        internal abstract IGridTrackCollection GridTracksMain { get; }
        internal abstract IGridTrackCollection GridTracksCross { get; }

        private LogicalExtent MinScrollToMain
        {
            get { return new LogicalExtent(FrozenHeadTracksCountMain); }
        }

        private LogicalExtent MaxScrollToMain
        {
            get { return new LogicalExtent(MaxGridExtentMain - FrozenTailTracksCountMain); }
        }

        private double ScrollToMainExtent
        {
            get { return Translate(_scrollToMain); }
        }

        private LogicalMainTrack TailStartLogicalMainTrack
        {
            get { return TailTracksCountMain > 0 ? new LogicalMainTrack(GridTracksMain.LastOf(TailTracksCountMain)) : LogicalMainTrack.Eof; }
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

        private int FrozenHeadTracksCountMain
        {
            get { return GridTracksMain.FrozenHeadTracksCount; }
        }

        private int FrozenTailTracksCountMain
        {
            get { return GridTracksMain.FrozenTailTracksCount; }
        }

        private int FrozenHeadTracksCountCross
        {
            get { return GridTracksCross.FrozenHeadTracksCount; }
        }

        private int HeadTracksCountMain
        {
            get { return GridTracksMain.HeadTracksCount; }
        }

        private LogicalMainTrack LastHeadTrackMain
        {
            get { return new LogicalMainTrack(GridTracksMain[HeadTracksCountMain - 1]); }
        }

        private LogicalMainTrack FirstTailTrackMain
        {
            get { return new LogicalMainTrack(GridTracksMain[HeadTracksCountMain + ContainerTracksCountMain]); }
        }

        private int HeadTracksCountCross
        {
            get { return GridTracksCross.HeadTracksCount; }
        }

        private int ContainerTracksCountCross
        {
            get { return GridTracksCross.ContainerTracksCount; }
        }

        private int RowTracksCountCross
        {
            get { return GridTracksCross.RowTracksCount; }
        }

        private int TailTracksCountCross
        {
            get { return GridTracksCross.TailTracksCount; }
        }

        private int ContainerTracksCountMain
        {
            get { return GridTracksMain.ContainerEnd.Ordinal - GridTracksMain.ContainerStart.Ordinal + 1; }
        }

        private int TotalContainerGridTracksMain
        {
            get { return MaxContainerCount * ContainerTracksCountMain; }
        }

        private int FrozenTailTracksCountCross
        {
            get { return GridTracksCross.FrozenTailTracksCount; }
        }

        private int TailTracksCountMain
        {
            get { return GridTracksMain.TailTracksCount; }
        }

        private double MaxExtentMain
        {
            get { return MaxGridExtentMain == 0 ? 0 : GetLogicalMainTrack(MaxGridExtentMain - 1).EndExtent; }
        }

        private double MaxExtentCross
        {
            get
            {
                var result = GridTracksCross.GetMeasuredLength(Template.Range());
                if (FlowRepeatCount > 1)
                    result += FlowLength * (FlowRepeatCount - 1);
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

        protected double FrozenHeadLengthMain
        {
            get { return FrozenHeadTracksCountMain == 0 ? 0 : GridTracksMain[FrozenHeadTracksCountMain - 1].EndOffset; }
        }

        protected double FrozenHeadLengthCross
        {
            get { return GridTracksCross[FrozenHeadTracksCountCross].StartOffset; }
        }

        protected double FrozenTailLengthMain
        {
            get { return GetTailLengthMain(FrozenTailTracksCountMain); }
        }

        private double GetTailLengthMain(int count)
        {
            if (count == 0)
                return 0;

            var totalTracks = GridTracksMain.Count;
            var endOffset = GridTracksMain[totalTracks - 1].EndOffset;
            var startOffset = GridTracksMain[totalTracks - count].StartOffset;
            return endOffset - startOffset;
        }

        protected double FrozenTailLengthCross
        {
            get { return GetTailLengthCross(FrozenTailTracksCountCross); }
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
            get { return TailTracksCountMain == 0 ? 0 : MaxExtentMain - GetLogicalMainTrack(MaxGridExtentMain - TailTracksCountMain).StartExtent; }
        }

        private double GetRelativePositionMain(ContainerView containerView, GridTrack gridTrack)
        {
            Debug.Assert(GridTracksMain.GetGridSpan(Template.ContainerRange).Contains(gridTrack));
            return new LogicalMainTrack(gridTrack, containerView).StartPosition - GetStartPositionMain(containerView);
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
            var start = GetStartLogicalMainTrack(gridRange);
            var end = GetEndLogicalMainTrack(gridRange);
            return start == end ? start.Length : end.EndExtent - start.StartExtent;
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
            var endPosition = GetEndPositionCross(gridRange, ShouldStretchCross(scalarBinding) ? FlowRepeatCount - 1 : 0);
            return endPosition - startPosition;
        }

        private bool ShouldStretchCross(ScalarBinding scalarBinding)
        {
            if (FlowRepeatCount > 1 && !scalarBinding.FlowRepeatable)
            {
                var rowSpan = GridTracksCross.GetGridSpan(Template.RowRange);
                var scalarBindingSpan = GridTracksCross.GetGridSpan(scalarBinding.GridRange);
                if (rowSpan.Contains(scalarBindingSpan))
                    return true;
            }
            return false;
        }

        private Clip GetFrozenClipMain(double startPosition, double endPosition, Binding binding)
        {
            return GetFrozenClipMain(startPosition, endPosition, binding.GridRange);
        }

        private Clip GetFrozenClipMain(double startPosition, double endPosition, GridRange gridRange)
        {
            var gridSpan = GridTracksMain.GetGridSpan(gridRange);
            return GetFrozenClipMain(startPosition, endPosition, gridSpan);
        }

        private Clip GetFrozenClipMain(double startPosition, double endPosition, GridSpan gridSpan)
        {
            double? minStart = GetMinClipMain(gridSpan.StartTrack);
            double? maxEnd = GetMaxClipMain(gridSpan.EndTrack);
            return new Clip(startPosition, endPosition, minStart, maxEnd);
        }

        private double? GetMinClipMain(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Owner == GridTracksMain);
            return FrozenHeadTracksCountMain == 0 || gridTrack.IsFrozenHead ? new double?() : FrozenHeadLengthMain;
        }

        private double? GetMaxClipMain(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Owner == GridTracksMain);
            return FrozenTailTracksCountMain == 0 || gridTrack.IsFrozenTail ? new double?() : ViewportMain - FrozenTailLengthMain;
        }

        private Clip GetFrozenClipCross(double startPosition, double endPosition, Binding binding)
        {
            return GetFrozenClipCross(startPosition, endPosition, binding.GridRange);
        }

        private Clip GetFrozenClipCross(double startPosition, double endPosition, GridRange gridRange, int? flowIndex = null)
        {
            var gridSpan = GridTracksCross.GetGridSpan(gridRange);
            return GetFrozenClipCross(startPosition, endPosition, gridSpan, flowIndex);
        }

        private Clip GetFrozenClipCross(double startPosition, double endPosition, GridSpan gridSpan, int? flowIndex = null)
        {
            double? minStart = GetMinFrozenClipCross(gridSpan.StartTrack);
            double? maxEnd = GetMaxFrozenClipCross(gridSpan.EndTrack, flowIndex);
            return new Clip(startPosition, endPosition, minStart, maxEnd);
        }

        private double? GetMinFrozenClipCross(GridTrack gridTrack)
        {
            return FrozenHeadTracksCountCross == 0 || gridTrack.IsFrozenHead ? new double?() : FrozenHeadLengthCross;
        }

        private double? GetMaxFrozenClipCross(GridTrack gridTrack, int? flowIndex = null)
        {
            return FrozenTailTracksCountCross == 0 || IsFrozenTailCross(gridTrack, flowIndex) ? new double?() : ViewportCross - FrozenTailLengthCross;
        }

        private bool IsFrozenTailCross(GridTrack gridTrack, int? flowIndex)
        {
            Debug.Assert(gridTrack.Owner == GridTracksCross);
            return !flowIndex.HasValue ? gridTrack.IsFrozenTail : flowIndex.Value == FlowRepeatCount - 1 && gridTrack.IsFrozenTail;
        }

        internal override Thickness GetFrozenClip(ScalarBinding scalarBinding, int flowIndex)
        {
            var clipMain = GetFrozenClipMain(scalarBinding);
            var clipCross = GetFrozenClipCross(scalarBinding, flowIndex);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetFrozenClipMain(ScalarBinding scalarBinding)
        {
            var startPosition = GetStartPositionMain(scalarBinding);
            var endPosition = startPosition + GetLengthMain(scalarBinding);
            return GetFrozenClipMain(startPosition, endPosition, scalarBinding);
        }

        private Clip GetFrozenClipCross(ScalarBinding scalarBinding, int flowIndex)
        {
            var startPosition = GetStartPositionCross(scalarBinding, flowIndex);
            var endPosition = startPosition + GetLengthCross(scalarBinding);
            return GetFrozenClipCross(startPosition, endPosition, scalarBinding);
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
            var startTrack = GridTracksMain.GetGridSpan(Template.ContainerRange).StartTrack;
            return new LogicalMainTrack(startTrack, containerView).StartPosition;
        }

        private double GetEndPositionMain(ContainerView containerView)
        {
            var endTrack = GridTracksMain.GetGridSpan(Template.ContainerRange).EndTrack;
            return new LogicalMainTrack(endTrack, containerView).EndPosition;
        }

        private double GetContainerStartPositionCross()
        {
            return GetStartPositionCross(Template.ContainerRange, 0);
        }

        private double GetContainerEndPositionCross()
        {
            return GetEndPositionCross(Template.ContainerRange, FlowRepeatCount - 1);
        }

        protected override Size GetSize(ContainerView containerView)
        {
            var valueMain = GetLengthMain(containerView);
            var valueCross = GetContainerLengthCross();
            return ToSize(valueMain, valueCross);
        }

        private double GetLengthMain(ContainerView containerView)
        {
            return GetMeasuredLengthMain(containerView, Template.ContainerRange);
        }

        private double GetContainerLengthCross()
        {
            return GetContainerEndPositionCross() - GetContainerStartPositionCross();
        }

        internal override Thickness GetFrozenClip(ContainerView containerView)
        {
            var clipMain = GetFrozenClipMain(containerView);
            var clipCross = GetContainerFrozenClipCross();
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetFrozenClipMain(ContainerView containerView)
        {
            var startPosition = GetStartPositionMain(containerView);
            var endPosition = startPosition + GetLengthMain(containerView);
            return GetFrozenClipMain(startPosition, endPosition, Template.ContainerRange);
        }

        private Clip GetContainerFrozenClipCross()
        {
            var startPosition = GetContainerStartPositionCross();
            var endPosition = GetContainerEndPositionCross();
            return GetFrozenClipCross(startPosition, endPosition, Template.ContainerRange);
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
            return GetRelativePositionMain(blockView, startTrack);
        }

        protected override Size GetSize(BlockView blockView, BlockBinding blockBinding)
        {
            var valueMain = GetMeasuredLengthMain(blockView, blockBinding.GridRange);
            var valueCross = GetEndPositionCross(blockView, blockBinding) - GetStartPositionCross(blockView, blockBinding);
            return ToSize(valueMain, valueCross);
        }

        internal override Thickness GetFrozenClip(BlockView blockView, BlockBinding blockBinding)
        {
            var clipMain = GetFrozenClipMain(blockView, blockBinding);
            var clipCross = GetFrozenClipCross(blockView, blockBinding);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetFrozenClipMain(BlockView blockView, BlockBinding blockBinding)
        {
            var startPosition = GetStartPositionMain(blockView, blockBinding);
            var endPosition = startPosition + GetMeasuredLengthMain(blockView, blockBinding.GridRange);
            return GetFrozenClipMain(startPosition, endPosition, blockBinding);
        }

        private bool IsHead(BlockBinding blockBinding)
        {
            return GridTracksCross.GetGridSpan(blockBinding.GridRange).EndTrack.Ordinal < GridTracksCross.GetGridSpan(Template.RowRange).StartTrack.Ordinal;
        }

        private double GetStartPositionCross(BlockView blockView, BlockBinding blockBinding)
        {
            return GetStartPositionCross(blockBinding.GridRange, IsHead(blockBinding) ? 0 : FlowRepeatCount - 1);
        }

        private double GetEndPositionCross(BlockView blockView, BlockBinding blockBinding)
        {
            return GetEndPositionCross(blockBinding.GridRange, IsHead(blockBinding) ? 0 : FlowRepeatCount - 1);
        }

        private Clip GetFrozenClipCross(BlockView blockView, BlockBinding blockBinding)
        {
            var startPosition = GetStartPositionCross(blockView, blockBinding);
            var endPosition = GetEndPositionCross(blockView, blockBinding);
            return GetFrozenClipCross(startPosition, endPosition, blockBinding);
        }

        private double GetRowViewStartPositionCross(int flowIndex)
        {
            var rowRange = Template.RowRange;
            var result = GetStartPositionCross(rowRange, flowIndex);
            if (flowIndex == FlowRepeatCount - 1 && GridTracksCross.GetGridSpan(rowRange).EndTrack.IsFrozenTail)
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

        internal override Thickness GetFrozenClip(int flowIndex)
        {
            var clipMain = new Clip();
            var clipCross = GetFrozenClipCross(flowIndex);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetFrozenClipCross(int flowIndex)
        {
            var startPosition = GetRowViewStartPositionCross(flowIndex);
            var endPosition = GetEndPositionCross(Template.RowRange, flowIndex);
            return GetFrozenClipCross(startPosition, endPosition, Template.RowRange, flowIndex);
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

        internal override Thickness GetFrozenClip(RowView rowView, RowBinding rowBinding)
        {
            var clipMain = GetFrozenClipMain(rowView, rowBinding);
            var clipCross = GetFrozenClipCross(rowView, rowBinding);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetFrozenClipMain(RowView rowView, RowBinding rowBinding)
        {
            var containerView = this[rowView];
            var startPosition = GetStartPositionMain(containerView) + GetStartPositionMain(rowView, rowBinding);
            var endPosition = startPosition + GetMeasuredLengthMain(containerView, rowBinding.GridRange);
            return GetFrozenClipMain(startPosition, endPosition, rowBinding);
        }

        private double GetStartPositionCross(RowView rowView, RowBinding rowBinding)
        {
            return GetStartPositionCross(rowBinding.GridRange, rowView.FlowIndex);
        }

        private double GetEndPositionCross(RowView rowView, RowBinding rowBinding)
        {
            return GetEndPositionCross(rowBinding.GridRange, rowView.FlowIndex);
        }

        private Clip GetFrozenClipCross(RowView rowView, RowBinding rowBinding)
        {
            var startPosition = GetStartPositionCross(rowView, rowBinding);
            var endPosition = GetEndPositionCross(rowView, rowBinding);
            return GetFrozenClipCross(startPosition, endPosition, rowBinding);
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

        protected abstract IEnumerable<LineFigure> GetLineFiguresX(int startGridPointX, int endGridPointX, GridPlacement? placement, int gridPointY);

        protected abstract IEnumerable<LineFigure> GetLineFiguresY(int startGridPointY, int endGridPointY, GridPlacement? placement, int gridPointX);

        private static GridTrack GetPrevGridTrack(IReadOnlyList<GridTrack> gridTracks, int gridPoint, GridPlacement? placement)
        {
            if (placement.HasValue && placement.GetValueOrDefault() != GridPlacement.Tail)
                return null;
            return gridPoint == 0 ? null : gridTracks[gridPoint - 1];
        }

        private static GridTrack GetNextGridTrack(IReadOnlyList<GridTrack> gridTracks, int gridPoint, GridPlacement? placement)
        {
            if (placement.HasValue && placement.GetValueOrDefault() != GridPlacement.Head)
                return null;
            return gridPoint == gridTracks.Count ? null : gridTracks[gridPoint];
        }

        protected IEnumerable<LineFigure> GetLineFiguresMain(int startGridPointMain, int endGridPointMain, GridPlacement? placement, int gridPointCross)
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
            var startLogicalMainTrack = GetStartLogicalMainTrack(startTrackMain);
            if (startLogicalMainTrack.IsEof)
                return null;
            var startPositionMain = startLogicalMainTrack.StartPosition;
            var endPositionMain = GetEndLogicalMainTrack(endTrackMain).EndPosition;
            if (endPositionMain <= startPositionMain)
                return null;

            var clip = GetFrozenClipMain(startPositionMain, endPositionMain, new GridSpan(startTrackMain, endTrackMain));
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
            if (!prevTrack.IsContainer)
                return new LogicalMainTrack(prevTrack).EndPosition;

            if (ContainerViewList.Last != null)
                return new LogicalMainTrack(prevTrack, ContainerViewList.Last).EndPosition;

            prevTrack = HeadTracksCountMain == 0 ? null : GridTracksMain[HeadTracksCountMain - 1];
            return prevTrack == null ? -ScrollOffsetMain : new LogicalMainTrack(prevTrack).EndPosition;
        }

        private static void AnalyzeLineGridPoint(IGridTrackCollection gridTracks, int startGridPoint, int endGridPoint,
            int gridPoint, GridPlacement? placement,
            out GridTrack prevGridTrack, out GridTrack nextGridTrack, out bool beforeRepeat, out bool isRepeat, out bool afterRepeat)
        {
            prevGridTrack = GetPrevGridTrack(gridTracks, gridPoint, placement);
            nextGridTrack = GetNextGridTrack(gridTracks, gridPoint, placement);

            beforeRepeat = gridPoint <= startGridPoint;
            isRepeat = gridPoint >= startGridPoint && gridPoint <= endGridPoint;
            afterRepeat = gridPoint >= endGridPoint;

            if (beforeRepeat && isRepeat)
            {
                if (prevGridTrack == null)
                    beforeRepeat = false;
                else if (nextGridTrack == null)
                    isRepeat = false;
            }

            if (isRepeat && afterRepeat)
            {
                if (prevGridTrack == null)
                    isRepeat = false;
                else if (nextGridTrack == null)
                    afterRepeat = false;
            }
        }

        private IEnumerable<double> GetLineFigurePositionsCross(int gridPointCross, GridPlacement? placement)
        {
            GridTrack prevGridTrack, nextGridTrack;
            bool beforeRepeat, isRepeat, afterRepeat;
            AnalyzeLineGridPoint(GridTracksCross, GridTracksCross.RowStart.Ordinal, GridTracksCross.RowEnd.Ordinal + 1,
                gridPointCross, placement, out prevGridTrack, out nextGridTrack, out beforeRepeat, out isRepeat, out afterRepeat);

            GridTrack gridTrack;
            if (beforeRepeat)
            {
                var value = GetBeforeRepeatPositionCross(prevGridTrack, nextGridTrack, out gridTrack);
                if (!Clip.IsHeadClipped(value, GetMinFrozenClipCross(gridTrack)))
                    yield return value;
            }

            if (isRepeat)
            {
                var first = 0;
                if (beforeRepeat)
                    first += 1;
                var last = FlowRepeatCount - 1;
                if (afterRepeat)
                    last -= 1;
                for (int i = first; i <= last; i++)
                {
                    var value = GetRepeatPositionCross(prevGridTrack, nextGridTrack, i, out gridTrack);
                    if (!Clip.IsClipped(value, GetMinFrozenClipCross(gridTrack), GetMaxFrozenClipCross(gridTrack, i)))
                        yield return value;
                }
            }

            if (afterRepeat)
            {
                var value = GetAfterRepeatPositionCross(prevGridTrack, nextGridTrack, out gridTrack);
                if (!Clip.IsTailClipped(value, GetMaxFrozenClipCross(gridTrack)))
                    yield return value;
            }
        }

        private double GetBeforeRepeatPositionCross(GridTrack prevGridTrack, GridTrack nextGridTrack, out GridTrack gridTrack)
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
            if (prevGridTrack != null && prevGridTrack.IsRow)
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

        private double GetAfterRepeatPositionCross(GridTrack prevGridTrack, GridTrack nextGridTrack, out GridTrack gridTrack)
        {
            if (nextGridTrack != null)
            {
                gridTrack = nextGridTrack;
                return GetStartPositionCross(gridTrack, 0);
            }
            else
            {
                gridTrack = prevGridTrack;
                return GetEndPositionCross(gridTrack, gridTrack.IsRow ? FlowRepeatCount - 1 : 0);
            }
        }

        protected IEnumerable<LineFigure> GetLineFiguresCross(int startGridPointCross, int endGridPointCross, GridPlacement? placement, int gridPointMain)
        {
            var spanCross = GetLineFigureSpanCross(startGridPointCross, endGridPointCross);
            if (spanCross.Length <= 0)
                yield break;

            foreach (var positionMain in GetPositionsMain(gridPointMain, placement))
                yield return new LineFigure(ToPoint(positionMain, spanCross.Start), ToPoint(positionMain, spanCross.End));
        }

        private Span GetLineFigureSpanCross(int startGridPointCross, int endGridPointCross)
        {
            var startTrackCross = GridTracksCross[startGridPointCross];
            var endTrackCross = GridTracksCross[endGridPointCross - 1];
            var startPositionCross = GetStartPositionCross(startTrackCross, 0);
            var endPositionCross = GetEndPositionCross(endTrackCross, endTrackCross.IsContainer ? FlowRepeatCount - 1 : 0);
            if (endPositionCross <= startPositionCross)
                return new Span();

            var clip = GetFrozenClipCross(startPositionCross, endPositionCross, new GridSpan(startTrackCross, endTrackCross));
            if (double.IsPositiveInfinity(clip.Head) || double.IsPositiveInfinity(clip.Tail))
                return new Span();
            startPositionCross += clip.Head;
            endPositionCross -= clip.Tail;
            if (endPositionCross <= startPositionCross)
                return new Span();

            return new Span(startPositionCross, endPositionCross);
        }

        private IEnumerable<double> GetPositionsMain(int gridPointMain, GridPlacement? placement)
        {
            GridTrack prevGridTrack, nextGridTrack;
            bool beforeRepeat, isRepeat, afterRepeat;
            AnalyzeLineGridPoint(GridTracksMain, GridTracksMain.ContainerStart.Ordinal, GridTracksMain.ContainerEnd.Ordinal + 1,
                gridPointMain, placement, out prevGridTrack, out nextGridTrack, out beforeRepeat, out isRepeat, out afterRepeat);
            bool isStretch = gridPointMain == MinStretchGridOrdinal && StretchGap.HasValue;

            GridTrack gridTrack;

            if (beforeRepeat)
            {
                var value = GetPositionMain(prevGridTrack, nextGridTrack, null, true, out gridTrack);
                if (!Clip.IsHeadClipped(value, GetMinClipMain(gridTrack)))
                    yield return value;
            }

            if (isRepeat)
            {
                var first = 0;
                if (beforeRepeat)
                    first += 1;
                var last = ContainerViewList.Count - 1;
                if (last == MaxContainerCount - 1 && afterRepeat && !isStretch)
                    last -= 1;
                for (int i = first; i <= last; i++)
                {
                    var containerView = ContainerViewList[i];
                    var value = GetPositionMain(prevGridTrack, nextGridTrack, containerView, (prevGridTrack != null && prevGridTrack.IsContainer), out gridTrack);
                    if (!Clip.IsClipped(value, GetMinClipMain(gridTrack), GetMaxClipMain(gridTrack)))
                        yield return value;
                }
            }

            if (afterRepeat)
            {
                if (isStretch && !prevGridTrack.IsContainer)
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

        private void EnsureVisibleMain(LogicalMainTrack start, LogicalMainTrack end)
        {
            if (start.GridTrack.IsFrozenHead || end.GridTrack.IsFrozenTail)
                return;

            var startGridExtent = start.StartGridExtent;
            var isHeadClipped = IsHeadClippedMain(startGridExtent);
            var endGridExtent = end.EndGridExtent;
            var isTailClipped = IsTailClippedMain(endGridExtent);

            if (isHeadClipped && isTailClipped)
                return;
            else if (isHeadClipped)
                ScrollToMain(startGridExtent, 0, GridPlacement.Head, false);
            else if (isTailClipped)
                ScrollToMain(startGridExtent, 1, GridPlacement.Tail, false);
        }

        private bool IsHeadClippedMain(int gridExtent)
        {
            var startPosition = GetPositionMain(gridExtent, GridPlacement.Head);
            var frozenHeadPosition = FrozenHeadGridExtentMain == 0 ? 0 : GetPositionMain(FrozenHeadGridExtentMain, GridPlacement.Tail);
            return startPosition < frozenHeadPosition;
        }

        private bool IsTailClippedMain(int gridExtent)
        {
            var endPosition = GetPositionMain(gridExtent, GridPlacement.Tail);
            var frozenTailPosition = FrozenTailGridExtentMain == MaxGridExtentMain ? ViewportMain : GetPositionMain(FrozenTailGridExtentMain, GridPlacement.Head);
            return endPosition > frozenTailPosition;
        }

        private void EnsureVisibleCross(GridTrack startGridTrack, int startFlowIndex, GridTrack endGridTrack, int endFlowIndex)
        {
            if (startGridTrack.IsFrozenHead || endGridTrack.IsFrozenTail)
                return;

            var start = new LogicalCrossTrack(startGridTrack, startFlowIndex);
            var end = new LogicalCrossTrack(endGridTrack, endFlowIndex);

            var startGridExtent = start.StartGridExtent;
            var startPosition = GetPositionCross(startGridExtent, GridPlacement.Head);
            var frozenHeadPosition = FrozenHeadGridExtentCross == 0 ? 0 : GetPositionMain(FrozenHeadGridExtentCross, GridPlacement.Tail);
            var isHeadClipped = startPosition < frozenHeadPosition;
            var endGridExtent = end.EndGridExtent;
            var endPosition = GetPositionCross(endGridExtent, GridPlacement.Tail);
            var frozenTailPosition = FrozenTailGridExtentCross == MaxGridExtentCross ? ViewportCross : GetPositionCross(FrozenTailGridExtentCross, GridPlacement.Head);
            var isTailClipped = endPosition > frozenTailPosition;

            if (isHeadClipped && isTailClipped)
                return;
            else if (isHeadClipped)
                ScrollToCross(startGridExtent, 0, GridPlacement.Head);
            else if (isTailClipped)
                ScrollToCross(startGridExtent, 1, GridPlacement.Tail);
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
                    for (int i = 0; i < scalarBinding.FlowRepeatCount; i++)
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
            EnsureVisibleMain(scalarBinding);
            EnsureVisibleCross(scalarBinding, flowIndex);
        }

        private void EnsureVisibleMain(ScalarBinding scalarBinding)
        {
            var gridRange = scalarBinding.GridRange;
            EnsureVisibleMain(GetStartLogicalMainTrack(gridRange), GetEndLogicalMainTrack(gridRange));
        }

        private void EnsureVisibleCross(ScalarBinding scalarBinding, int flowIndex)
        {
            var gridSpan = GridTracksCross.GetGridSpan(scalarBinding.GridRange);
            var endFlowIndex = ShouldStretchCross(scalarBinding) ? FlowRepeatCount - 1 : flowIndex;
            EnsureVisibleCross(gridSpan.StartTrack, flowIndex, gridSpan.EndTrack, endFlowIndex);
        }

        private void EnsureVisible(RowView rowView)
        {
            EnsureVisibleMain(rowView);
            EnsureVisibleCross(rowView);
        }

        private void EnsureVisibleMain(RowView rowView)
        {
            var gridSpan = GridTracksMain.GetGridSpan(Template.RowRange);
            var containerOrdinal = rowView.ContainerOrdinal;
            EnsureVisibleMain(new LogicalMainTrack(gridSpan.StartTrack, containerOrdinal), new LogicalMainTrack(gridSpan.EndTrack, containerOrdinal));
        }

        private void EnsureVisibleCross(RowView rowView)
        {
            var gridSpan = GridTracksCross.GetGridSpan(Template.RowRange);
            var flowIndex = rowView.FlowIndex;
            EnsureVisibleCross(gridSpan.StartTrack, flowIndex, gridSpan.EndTrack, flowIndex);
        }

        private void EnsureVisible(RowView rowView, RowBinding rowBinding)
        {
            EnsureVisibleMain(rowView, rowBinding);
            EnsureVisibleCross(rowView, rowBinding);
        }

        private void EnsureVisibleMain(RowView rowView, RowBinding rowBinding)
        {
            var gridSpan = GridTracksMain.GetGridSpan(rowBinding.GridRange);
            var containerOrdinal = rowView.ContainerOrdinal;
            EnsureVisibleMain(new LogicalMainTrack(gridSpan.StartTrack, containerOrdinal), new LogicalMainTrack(gridSpan.EndTrack, containerOrdinal));
        }

        private void EnsureVisibleCross(RowView rowView, RowBinding rowBinding)
        {
            var gridSpan = GridTracksCross.GetGridSpan(rowBinding.GridRange);
            var flowIndex = rowView.FlowIndex;
            EnsureVisibleCross(gridSpan.StartTrack, flowIndex, gridSpan.EndTrack, flowIndex);
        }

        private void EnsureVisible(BlockView blockView, BlockBinding blockBinding)
        {
            EnsureVisibleMain(blockView, blockBinding);
            EnsureVisibleCross(blockView, blockBinding);
        }

        private void EnsureVisibleMain(BlockView blockView, BlockBinding blockBinding)
        {
            var gridSpan = GridTracksMain.GetGridSpan(blockBinding.GridRange);
            var blockOrdinal = blockView.ContainerOrdinal;
            EnsureVisibleMain(new LogicalMainTrack(gridSpan.StartTrack, blockOrdinal), new LogicalMainTrack(gridSpan.EndTrack, blockOrdinal));
        }

        private void EnsureVisibleCross(BlockView blockView, BlockBinding blockBinding)
        {
            var gridSpan = GridTracksCross.GetGridSpan(blockBinding.GridRange);
            var flowIndex = gridSpan.EndTrack.Ordinal > GridTracksCross.GetGridSpan(Template.RowRange).EndTrack.Ordinal ? FlowRepeatCount : 0;
            EnsureVisibleCross(gridSpan.StartTrack, flowIndex, gridSpan.EndTrack, flowIndex);
        }

        protected sealed override void Reload()
        {
            base.Reload();

            _scrollDeltaMain = 0;
            _scrollOffsetMain = 0;
            _scrollToMainPlacement = default(GridPlacement);
            _scrollToMain = default(LogicalExtent);
        }

        internal override void Inherit(LayoutManager layoutManager)
        {
            base.Inherit(layoutManager);
            var scrollable = layoutManager as ScrollableManager;
            if (scrollable == null)
                return;

            _scrollDeltaMain = scrollable._scrollDeltaMain;
            _scrollOffsetMain = scrollable._scrollOffsetMain;
            _scrollToMainPlacement = scrollable._scrollToMainPlacement;
            _scrollToMain = scrollable._scrollToMain;
            _scrollDeltaCross = scrollable._scrollDeltaCross;
            ScrollOffsetCross = scrollable.ScrollOffsetCross;
        }
    }
}
