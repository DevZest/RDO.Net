using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutXYManager
    {
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
                _blockOrdinal = -1;
            }

            public GridOffset(GridTrack gridTrack, BlockView block)
            {
                Debug.Assert(gridTrack.IsRepeat && block != null);
                GridTrack = gridTrack;
                _blockOrdinal = block.Ordinal;
            }

            public GridOffset(GridTrack gridTrack, int blockOrdinal)
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

            public Span Span
            {
                get
                {
                    Debug.Assert(!IsEof);
                    return GridTrack.IsRepeat ? GridTrack.GetSpan(BlockOrdinal) : GridTrack.GetSpan();
                }
            }
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
    }
}
