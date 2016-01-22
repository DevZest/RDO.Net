namespace DevZest.Data.Windows
{
    internal interface IGridColumnSet : IGridTrackSet<GridColumn>
    {
    }

    internal static class IGridColumnSetExtensions
    {
        public static IGridColumnSet Merge(this IGridColumnSet x, IGridColumnSet y)
        {
            return GridColumnSet.Merge(x, y);
        }
    }
}
