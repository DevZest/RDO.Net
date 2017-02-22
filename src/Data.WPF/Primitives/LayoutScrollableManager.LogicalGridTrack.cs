using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutScrollableManager
    {
        /// <summary>The (GridTrack, ContainerOrdinal) pair to uniquely identify the grid track on the main axis, can be converted to/from an int index value.</summary>
        private struct LogicalGridTrack
        {
            public static LogicalGridTrack Eof
            {
                get { return new LogicalGridTrack(); }
            }

            public LogicalGridTrack(GridTrack gridTrack)
            {
                Debug.Assert(!gridTrack.IsRepeat);
                GridTrack = gridTrack;
                _containerOrdinal = -1;
            }

            public LogicalGridTrack(GridTrack gridTrack, ContainerView containerView)
                : this(gridTrack, containerView.ContainerOrdinal)
            {
            }

            public LogicalGridTrack(GridTrack gridTrack, int containerOrdinal)
            {
                Debug.Assert(gridTrack.IsRepeat && containerOrdinal >= 0);
                GridTrack = gridTrack;
                _containerOrdinal = containerOrdinal;
            }

            public readonly GridTrack GridTrack;
            private readonly int _containerOrdinal;
            public int ContainerOrdinal
            {
                get { return IsRepeat ? _containerOrdinal : -1; }
            }

            public bool IsEof
            {
                get { return GridTrack == null; }
            }

            public bool IsRepeat
            {
                get { return GridTrack != null && GridTrack.IsRepeat; }
            }

            public static bool operator ==(LogicalGridTrack x, LogicalGridTrack y)
            {
                return x.GridTrack == y.GridTrack && x.ContainerOrdinal == y.ContainerOrdinal;
            }

            public static bool operator !=(LogicalGridTrack x, LogicalGridTrack y)
            {
                return !(x == y);
            }

            public override int GetHashCode()
            {
                return IsEof ? 0 : GridTrack.GetHashCode() ^ ContainerOrdinal;
            }

            public override bool Equals(object obj)
            {
                return obj is LogicalGridTrack ? (LogicalGridTrack)obj == this : false;
            }

            public Span Span
            {
                get
                {
                    Debug.Assert(!IsEof);
                    return GridTrack.IsRepeat ? GetGridTrackSpan(ContainerOrdinal) : GetGridTrackSpan();
                }
            }

            private Template Template
            {
                get { return GridTrack.Template; }
            }

            private LayoutScrollableManager LayoutXYManager
            {
                get { return Template.LayoutXYManager; }
            }

            private ContainerViewList ContainerViewList
            {
                get { return LayoutXYManager.ContainerViewList; }
            }

            private int MaxContainerCount
            {
                get { return ContainerViewList.MaxCount; }
            }

            private VariantLengthHandler VariantLengthHandler
            {
                get { return LayoutXYManager.GetVariantLengthHandler(); }
            }

            private IGridTrackCollection GridTrackOwner
            {
                get { return GridTrack.Owner; }
            }

            private int MaxFrozenHead
            {
                get { return GridTrackOwner.MaxFrozenHead; }
            }

            private bool VariantByContainer
            {
                get { return GridTrackOwner.VariantByContainer; }
            }

            private Span GetGridTrackSpan()
            {
                Debug.Assert(GridTrackOwner == LayoutXYManager.GridTracksMain);
                Debug.Assert(!IsRepeat);

                if (GridTrack.IsHead)
                    return new Span(GridTrack.StartOffset, GridTrack.EndOffset);

                Debug.Assert(GridTrack.IsTail);
                var delta = GetContainerViewsLength(MaxContainerCount);
                if (!GridTrackOwner.VariantByContainer && MaxContainerCount > 0)
                    delta -= GridTrackOwner.GetMeasuredLength(Template.BlockRange);
                return new Span(GridTrack.StartOffset + delta, GridTrack.EndOffset + delta);
            }

            private Span GetGridTrackSpan(int ordinal)
            {
                Debug.Assert(GridTrackOwner == LayoutXYManager.GridTracksMain);
                Debug.Assert(IsRepeat && ordinal >= 0);

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
                Debug.Assert(IsRepeat && !VariantByContainer);
                var originOffset = GridTrackOwner.GetGridSpan(Template.RowRange).StartTrack.StartOffset;
                return new Span(GridTrack.StartOffset - originOffset, GridTrack.EndOffset - originOffset);
            }
        }

        private LogicalGridTrack GetLogicalGridTrack(int gridExtent)
        {
            Debug.Assert(gridExtent >= 0);

            if (gridExtent >= MaxGridExtentMain)
                return LogicalGridTrack.Eof;

            if (gridExtent < MaxFrozenHeadMain)
                return new LogicalGridTrack(GridTracksMain[gridExtent]);

            gridExtent -= MaxFrozenHeadMain;
            var totalContainerGridTracks = TotalContainerGridTracksMain;
            if (gridExtent < totalContainerGridTracks)
                return new LogicalGridTrack(GridTracksMain[MaxFrozenHeadMain + gridExtent % ContainerGridTracksMain], gridExtent / ContainerGridTracksMain);

            gridExtent -= totalContainerGridTracks;
            Debug.Assert(gridExtent < MaxFrozenTailMain);
            return new LogicalGridTrack(GridTracksMain[MaxFrozenHeadMain + ContainerGridTracksMain + gridExtent]);
        }
    }
}
