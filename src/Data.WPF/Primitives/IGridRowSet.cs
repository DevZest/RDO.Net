namespace DevZest.Data.Windows.Primitives
{
    internal interface IGridRowSet : IGridTrackSet<GridRow>
    {
    }

    internal static class IGridRowSetExtensions
    {
        public static IGridRowSet Merge(this IGridRowSet x, IGridRowSet y)
        {
            return GridRowSet.Merge(x, y);
        }
    }
}
