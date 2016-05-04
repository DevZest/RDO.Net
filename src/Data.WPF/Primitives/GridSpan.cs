using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
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
    }
}
