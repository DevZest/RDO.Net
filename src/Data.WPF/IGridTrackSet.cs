namespace DevZest.Data.Windows
{
    internal interface IGridTrackSet
    {
        int Count { get; }
        GridTrack this[int index] { get; }
    }

    internal interface IGridTrackSet<T> : IGridTrackSet
        where T : GridTrack
    {
        new T this[int index] { get; }
    }
}
