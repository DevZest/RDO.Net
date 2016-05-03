using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal struct GridSpan<T>
        where T : GridTrack
    {
        public static GridSpan<T> Empty = new GridSpan<T>();

        public readonly T StartTrack;
        public readonly T EndTrack;

        internal GridSpan(IReadOnlyList<T> gridTracks)
        {
            if (gridTracks == null || gridTracks.Count == 0)
                StartTrack = EndTrack = null;
            else
            {
                StartTrack = gridTracks[0];
                EndTrack = gridTracks[gridTracks.Count - 1];
            }
        }

        internal GridSpan(T startTrack, T endTrack)
        {
            Debug.Assert(endTrack == null || (endTrack.Template == startTrack.Template && endTrack.Ordinal >= startTrack.Ordinal));
            StartTrack = startTrack;
            EndTrack = endTrack;
        }

        public bool IsEmpty
        {
            get { return StartTrack == null; }
        }
    }
}
