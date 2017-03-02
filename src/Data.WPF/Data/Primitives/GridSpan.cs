using System;
using System.Diagnostics;

namespace DevZest.Windows.Data.Primitives
{
    internal struct GridSpan
    {
        public readonly GridTrack StartTrack;
        public readonly GridTrack EndTrack;

        internal GridSpan(GridTrack startTrack, GridTrack endTrack)
        {
            StartTrack = startTrack;
            EndTrack = endTrack;
        }

        public bool IsEmpty
        {
            get { return StartTrack == null; }
        }

        public int Count
        {
            get { return IsEmpty ? 0 : EndTrack.Ordinal - StartTrack.Ordinal + 1; }
        }

        public GridTrack this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < Count);
                return StartTrack.Owner[StartTrack.Ordinal + index];
            }
        }

        public double MeasuredLength
        {
            get { return IsEmpty ? 0 : EndTrack.EndOffset - StartTrack.StartOffset; }
        }

        public bool Contains(GridSpan gridSpan)
        {
            Debug.Assert(StartTrack.Owner == gridSpan.StartTrack.Owner);

            return gridSpan.StartTrack.Ordinal >= StartTrack.Ordinal && gridSpan.EndTrack.Ordinal <= EndTrack.Ordinal;
        }

        public bool Contains(GridTrack gridTrack)
        {
            Debug.Assert(StartTrack.Owner == gridTrack.Owner);

            return gridTrack.Ordinal >= StartTrack.Ordinal && gridTrack.Ordinal <= EndTrack.Ordinal;
        }
    }

    internal struct GridSpan<T>
        where T : GridTrack, IConcatList<T>
    {
        internal static GridSpan<T> From(GridTrackCollection<T> gridTracks)
        {
            Debug.Assert(gridTracks != null);
            return gridTracks.Count == 0 ? new GridSpan<T>() : new GridSpan<T>(gridTracks[0], gridTracks[gridTracks.Count - 1]);
        }

        public readonly T StartTrack;
        public readonly T EndTrack;

        internal GridSpan(T startTrack, T endTrack)
        {
            StartTrack = startTrack;
            EndTrack = endTrack;
        }

        public bool IsEmpty
        {
            get { return StartTrack == null; }
        }

        public bool IntersectsWith(GridSpan<T> gridSpan)
        {
            Debug.Assert(!IsEmpty && !gridSpan.IsEmpty);
            return gridSpan.StartTrack.Ordinal <= EndTrack.Ordinal && gridSpan.EndTrack.Ordinal >= StartTrack.Ordinal;
        }

        public int Count
        {
            get { return IsEmpty ? 0 : EndTrack.Ordinal - StartTrack.Ordinal + 1; }
        }

        public GridTrack this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < Count);
                return StartTrack.Owner[StartTrack.Ordinal + index];
            }
        }
    }
}
