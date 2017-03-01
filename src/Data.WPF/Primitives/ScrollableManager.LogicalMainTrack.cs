using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    partial class ScrollableManager
    {
        /// <summary>The (GridTrack, ContainerOrdinal) pair to uniquely identify the grid track on the main axis, can be converted to/from an int index value.</summary>
        private struct LogicalMainTrack
        {
            public static LogicalMainTrack Eof
            {
                get { return new LogicalMainTrack(); }
            }

            public LogicalMainTrack(GridTrack gridTrack)
            {
                Debug.Assert(!gridTrack.IsContainer);
                GridTrack = gridTrack;
                _containerOrdinal = -1;
            }

            public LogicalMainTrack(GridTrack gridTrack, ContainerView containerView)
                : this(gridTrack, containerView.ContainerOrdinal)
            {
            }

            public LogicalMainTrack(GridTrack gridTrack, int containerOrdinal)
            {
                Debug.Assert(gridTrack.IsContainer && containerOrdinal >= 0);
                GridTrack = gridTrack;
                _containerOrdinal = containerOrdinal;
            }

            public readonly GridTrack GridTrack;
            private readonly int _containerOrdinal;
            public int ContainerOrdinal
            {
                get { return IsContainer ? _containerOrdinal : -1; }
            }

            public bool IsEof
            {
                get { return GridTrack == null; }
            }

            public bool IsHead
            {
                get { return GridTrack != null && GridTrack.IsHead; }
            }

            public bool IsFrozenHead
            {
                get { return GridTrack != null && GridTrack.IsFrozenHead; }
            }

            public bool IsContainer
            {
                get { return GridTrack != null && GridTrack.IsContainer; }
            }

            public bool IsTail
            {
                get { return GridTrack != null && GridTrack.IsTail; }
            }

            public bool IsFrozenTail
            {
                get { return GridTrack != null && GridTrack.IsFrozenTail; }
            }

            public static bool operator ==(LogicalMainTrack x, LogicalMainTrack y)
            {
                return x.GridTrack == y.GridTrack && x.ContainerOrdinal == y.ContainerOrdinal;
            }

            public static bool operator !=(LogicalMainTrack x, LogicalMainTrack y)
            {
                return !(x == y);
            }

            public override int GetHashCode()
            {
                return IsEof ? 0 : GridTrack.GetHashCode() ^ ContainerOrdinal;
            }

            public override bool Equals(object obj)
            {
                return obj is LogicalMainTrack ? (LogicalMainTrack)obj == this : false;
            }

            public Span ExtentSpan
            {
                get
                {
                    Debug.Assert(!IsEof);
                    return GridTrack.IsContainer ? GetExtentSpan(ContainerOrdinal) : GetExtentSpan();
                }
            }

            public double Length
            {
                get { return ExtentSpan.Length; }
            }

            public double StartExtent
            {
                get { return ExtentSpan.Start; }
            }

            public double EndExtent
            {
                get { return ExtentSpan.End; }
            }

            public double StartPosition
            {
                get
                {
                    Debug.Assert(!IsEof);
                    var result = ExtentSpan.Start;
                    var gridTrack = GridTrack;

                    if (gridTrack.IsFrozenHead)
                        return result;

                    result -= ScrollableManager.ScrollOffsetMain;

                    if (gridTrack.IsFrozenTail)
                    {
                        double max = ScrollableManager.ViewportMain - (ScrollableManager.MaxExtentMain - ExtentSpan.Start);
                        if (result > max || gridTrack.Ordinal >= ScrollableManager.MinStretchGridOrdinal)
                            result = max;
                    }

                    return result;
                }
            }

            public double EndPosition
            {
                get { return StartPosition + ExtentSpan.Length; }
            }

            private Template Template
            {
                get { return GridTrack.Template; }
            }

            private ScrollableManager ScrollableManager
            {
                get { return Template.ScrollableManager; }
            }

            private ContainerViewList ContainerViewList
            {
                get { return ScrollableManager.ContainerViewList; }
            }

            private int MaxContainerCount
            {
                get { return ContainerViewList.MaxCount; }
            }

            private VariantLengthHandler VariantLengthHandler
            {
                get { return ScrollableManager.GetVariantLengthHandler(); }
            }

            private IGridTrackCollection GridTrackOwner
            {
                get { return GridTrack.Owner; }
            }

            private int MaxFrozenHead
            {
                get { return GridTrackOwner.HeadTracksCount; }
            }

            private bool VariantByContainer
            {
                get { return GridTrackOwner.VariantByContainer; }
            }

            private Span GetExtentSpan()
            {
                Debug.Assert(GridTrackOwner == ScrollableManager.GridTracksMain);
                Debug.Assert(!IsContainer);

                if (GridTrack.IsHead)
                    return new Span(GridTrack.StartOffset, GridTrack.EndOffset);

                Debug.Assert(GridTrack.IsTail);
                var delta = GetContainerViewsLength(MaxContainerCount);
                if (!GridTrackOwner.VariantByContainer && MaxContainerCount > 0)
                    delta -= GridTrackOwner.GetMeasuredLength(Template.ContainerRange);
                return new Span(GridTrack.StartOffset + delta, GridTrack.EndOffset + delta);
            }

            private Span GetExtentSpan(int ordinal)
            {
                Debug.Assert(GridTrackOwner == ScrollableManager.GridTracksMain);
                Debug.Assert(IsContainer && ordinal >= 0);

                var relativeSpan = GetRelativeSpan(ordinal);
                var startOffset = (MaxFrozenHead == 0 ? 0 : GridTrackOwner[MaxFrozenHead - 1].EndOffset) + GetContainerViewsLength(ordinal);
                return new Span(startOffset + relativeSpan.Start, startOffset + relativeSpan.End);
            }

            private double GetContainerViewsLength(int count)
            {
                Debug.Assert(count >= 0 && count <= MaxContainerCount);
                if (count == 0)
                    return 0;

                return VariantByContainer
                    ? VariantLengthHandler.GetContainerViewsLength(GridTrack, count)
                    : GridTrackOwner.GetGridSpan(Template.RowRange).MeasuredLength * count;
            }

            private Span GetRelativeSpan(int ordinal)
            {
                Debug.Assert(ordinal >= 0 && ordinal < MaxContainerCount);

                return !VariantByContainer ? GetRelativeSpan() : VariantLengthHandler.GetRelativeSpan(GridTrack, ordinal);
            }

            private Span GetRelativeSpan()
            {
                Debug.Assert(IsContainer && !VariantByContainer);
                var originOffset = GridTrackOwner.GetGridSpan(Template.RowRange).StartTrack.StartOffset;
                return new Span(GridTrack.StartOffset - originOffset, GridTrack.EndOffset - originOffset);
            }
        }

        private LogicalMainTrack GetLogicalMainTrack(int gridExtent)
        {
            Debug.Assert(gridExtent >= 0);

            if (gridExtent >= MaxGridExtentMain)
                return LogicalMainTrack.Eof;

            if (gridExtent < HeadTracksCountMain)
                return new LogicalMainTrack(GridTracksMain[gridExtent]);

            gridExtent -= HeadTracksCountMain;
            var totalContainerGridTracks = TotalContainerGridTracksMain;
            if (gridExtent < totalContainerGridTracks)
                return new LogicalMainTrack(GridTracksMain[HeadTracksCountMain + gridExtent % ContainerTracksCountMain], gridExtent / ContainerTracksCountMain);

            gridExtent -= totalContainerGridTracks;
            Debug.Assert(gridExtent < TailTracksCountMain);
            return new LogicalMainTrack(GridTracksMain[HeadTracksCountMain + ContainerTracksCountMain + gridExtent]);
        }

        private LogicalMainTrack GetStartLogicalMainTrack(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).StartTrack;
            return GetStartLogicalMainTrack(gridTrack);
        }

        private LogicalMainTrack GetStartLogicalMainTrack(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Owner == GridTracksMain);
            if (!gridTrack.IsContainer)
                return new LogicalMainTrack(gridTrack);

            if (MaxContainerCount > 0)
                return new LogicalMainTrack(gridTrack, 0);
            else
                return ScrollEndOffsetMain;
        }

        private LogicalMainTrack GetEndLogicalMainTrack(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).EndTrack;
            return GetEndLogicalMainTrack(gridTrack);
        }

        private LogicalMainTrack GetEndLogicalMainTrack(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Owner == GridTracksMain);
            if (!gridTrack.IsContainer)
                return new LogicalMainTrack(gridTrack);

            if (MaxContainerCount > 0)
                return new LogicalMainTrack(gridTrack, MaxContainerCount - 1);
            else
                return ScrollEndOffsetMain;
        }
    }
}
