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

            if (gridOffset >= MaxGridOffsetMain)
                return GridOffset.Eof;

            if (gridOffset < MaxFrozenHeadMain)
                return new GridOffset(GridTracksMain[gridOffset]);

            gridOffset -= MaxFrozenHeadMain;
            var totalBlockGridTracks = TotalBlockGridTracksMain;
            if (gridOffset < totalBlockGridTracks)
                return new GridOffset(GridTracksMain[MaxFrozenHeadMain + gridOffset % BlockGridTracksMain], gridOffset / BlockGridTracksMain);

            gridOffset -= totalBlockGridTracks;
            Debug.Assert(gridOffset < MaxFrozenTailMain);
            return new GridOffset(GridTracksMain[MaxFrozenHeadMain + BlockGridTracksMain + gridOffset]);
        }
    }
}
