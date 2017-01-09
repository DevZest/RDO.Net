using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutXYManager
    {
        /// <summary>The (GridTrack, Block) pair to uniquely identify the grid on the main axis, can be converted to/from an int index value.</summary>
        private struct GridOffset
        {
            public static GridOffset Eof
            {
                get { return new GridOffset(); }
            }

            public GridOffset(GridTrack gridTrack)
            {
                Debug.Assert(!gridTrack.IsRepeat);
                GridTrack = gridTrack;
                _ordinal = -1;
            }

            public GridOffset(GridTrack gridTrack, ContainerView containerView)
            {
                Debug.Assert(gridTrack.IsRepeat && containerView != null);
                GridTrack = gridTrack;
                _ordinal = containerView.ContainerOrdinal;
            }

            public GridOffset(GridTrack gridTrack, int ordinal)
            {
                Debug.Assert(gridTrack.IsRepeat && ordinal >= 0);
                GridTrack = gridTrack;
                _ordinal = ordinal;
            }

            public readonly GridTrack GridTrack;
            private readonly int _ordinal;
            public int Ordinal
            {
                get { return IsRepeat ? _ordinal : -1; }
            }

            public bool IsEof
            {
                get { return GridTrack == null; }
            }

            public bool IsRepeat
            {
                get { return GridTrack != null && GridTrack.IsRepeat; }
            }

            public static bool operator ==(GridOffset x, GridOffset y)
            {
                return x.GridTrack == y.GridTrack && x.Ordinal == y.Ordinal;
            }

            public static bool operator !=(GridOffset x, GridOffset y)
            {
                return !(x == y);
            }

            public override int GetHashCode()
            {
                return IsEof ? 0 : GridTrack.GetHashCode() ^ Ordinal;
            }

            public override bool Equals(object obj)
            {
                return obj is GridOffset ? (GridOffset)obj == this : false;
            }

            public Span Span
            {
                get
                {
                    Debug.Assert(!IsEof);
                    return GridTrack.IsRepeat ? GetGridTrackSpan(Ordinal) : GetGridTrackSpan();
                }
            }

            private Template Template
            {
                get { return GridTrack.Template; }
            }

            private LayoutXYManager LayoutXYManager
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

        private GridOffset GetGridOffset(int gridOffset)
        {
            Debug.Assert(gridOffset >= 0);

            if (gridOffset >= MaxGridOffsetMain)
                return GridOffset.Eof;

            if (gridOffset < MaxFrozenHeadMain)
                return new GridOffset(GridTracksMain[gridOffset]);

            gridOffset -= MaxFrozenHeadMain;
            var totalContainerGridTracks = TotalContainerGridTracksMain;
            if (gridOffset < totalContainerGridTracks)
                return new GridOffset(GridTracksMain[MaxFrozenHeadMain + gridOffset % ContainerGridTracksMain], gridOffset / ContainerGridTracksMain);

            gridOffset -= totalContainerGridTracks;
            Debug.Assert(gridOffset < MaxFrozenTailMain);
            return new GridOffset(GridTracksMain[MaxFrozenHeadMain + ContainerGridTracksMain + gridOffset]);
        }
    }
}
