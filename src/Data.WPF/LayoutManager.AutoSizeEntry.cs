using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private struct AutoSizeEntry
        {
            public static AutoSizeEntry Empty
            {
                get { return new AutoSizeEntry(GridColumnSet.Empty, GridRowSet.Empty); }
            }

            public AutoSizeEntry(IGridColumnSet columns, IGridRowSet rows)
            {
                Debug.Assert(columns != null);
                Debug.Assert(rows != null);
                Columns = columns;
                Rows = rows;
            }

            public readonly IGridColumnSet Columns;

            public readonly IGridRowSet Rows;

            public bool IsEmpty
            {
                get { return Columns.Count == 0 && Rows.Count == 0; }
            }

            public static AutoSizeEntry Resolve(GridRange gridRange, bool sizeToContentX, bool sizeToContentY)
            {
                //  There is an issue with items contains both auto and star tracks.
                //  Intuitively, we expect that those items receive enough space to layout and that this space is perfectly divided into the auto / star tracks.
                //  The problem is that it is not possible to determine the size of star tracks until all auto track size determined,
                //  and that it is not possible determine missing space to include into the auto-sized tracks for those items as long as we don't know the size 
                //  of star-sized tracks.
                //  We are in a dead-end. There is basically two solutions: 
                //     1. Include all the missing size for those items into the auto tracks
                //     2. Include none of the missing size into the auto tracks and hope that the star tracks will be big enough to contain those items.
                //  Here we chose option (2), that is we ignore those elements during calculation of auto-sized tracks.
                //  The reason between this choice is that (1) will tend to increase excessively the size of auto-sized tracks (for nothing).
                //  Moreover, we consider items included both auto and star-size tracks are rare, and most of the time we want
                //  to be spread along several tracks rather than auto-sized.

                var columnSet = GridColumnSet.Empty;
                if (ContainsStarWidth(gridRange, sizeToContentX))
                {
                    for (int x = gridRange.Left.Ordinal; x <= gridRange.Right.Ordinal; x++)
                    {
                        var column = gridRange.Owner.GridColumns[x];
                        var width = column.Width;
                        if (width.IsAuto || (width.IsStar && sizeToContentX))
                            columnSet = columnSet.Merge(column);
                    }
                }

                var rowSet = GridRowSet.Empty;
                if (ContainsStarHeight(gridRange, sizeToContentY))
                {
                    for (int y = gridRange.Top.Ordinal; y <= gridRange.Bottom.Ordinal; y++)
                    {
                        var row = gridRange.Owner.GridRows[y];
                        var height = row.Height;
                        if (height.IsAuto || (height.IsStar && sizeToContentY))
                            rowSet = rowSet.Merge(row);
                    }
                }

                return new AutoSizeEntry(columnSet, rowSet);
            }

            private static bool ContainsStarWidth(GridRange gridRange, bool sizeToContentX)
            {
                if (sizeToContentX)
                    return false;

                for (int x = gridRange.Left.Ordinal; x <= gridRange.Right.Ordinal; x++)
                {
                    if (gridRange.Owner.GridColumns[x].Width.IsStar)
                        return true;
                }
                return false;
            }

            private static bool ContainsStarHeight(GridRange gridRange, bool sizeToContentY)
            {
                if (sizeToContentY)
                    return false;

                for (int y = gridRange.Top.Ordinal; y <= gridRange.Bottom.Ordinal; y++)
                {
                    if (gridRange.Owner.GridRows[y].Height.IsStar)
                        return true;
                }
                return false;
            }
        }
    }
}
