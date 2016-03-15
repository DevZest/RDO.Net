using System;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class GridColumnSet : GridTrackSet
    {
        public static readonly IGridColumnSet Empty = new EmptyGridColumnSet();

        public static IGridColumnSet Merge(IGridColumnSet x, IGridColumnSet y)
        {
            Debug.Assert(x != null && y != null);
            if (x.Count == 0)
                return y;
            else if (y.Count == 0)
                return x;
            else
                return new ListGridColumnSet(x, y);
        }

        private sealed class EmptyGridColumnSet : EmptyGridTrackSet<GridColumn>, IGridColumnSet
        {
        }

        private sealed class ListGridColumnSet : ListGridTrackSet<GridColumn>, IGridColumnSet
        {
            public ListGridColumnSet(IGridColumnSet x, IGridColumnSet y)
                : base(x, y)
            {
            }
        }
    }
}
