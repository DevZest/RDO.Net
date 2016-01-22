using System.Diagnostics;

namespace DevZest.Data.Windows
{
    internal abstract class GridRowSet : GridTrackSet
    {
        public static readonly IGridRowSet Empty = new EmptyGridRowSet();

        public static IGridRowSet Merge(IGridRowSet x, IGridRowSet y)
        {
            Debug.Assert(x != null && y != null);
            if (x.Count == 0)
                return y;
            else if (y.Count == 0)
                return x;
            else
                return new ListGridRowSet(x, y);
        }

        private sealed class EmptyGridRowSet : EmptyGridTrackSet<GridRow>, IGridRowSet
        {
        }

        private sealed class ListGridRowSet : ListGridTrackSet<GridRow>, IGridRowSet
        {
            public ListGridRowSet(IGridRowSet x, IGridRowSet y)
                : base(x, y)
            {
            }
        }
    }
}
