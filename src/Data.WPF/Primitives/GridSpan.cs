namespace DevZest.Data.Windows.Primitives
{
    internal struct GridSpan<T>
        where T : GridTrack, IConcatList<T>
    {
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
