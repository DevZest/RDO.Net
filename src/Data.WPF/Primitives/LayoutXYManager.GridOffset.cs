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
                    return GridTrack.IsRepeat ? GridTrack.GetSpan(Ordinal) : GridTrack.GetSpan();
                }
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
