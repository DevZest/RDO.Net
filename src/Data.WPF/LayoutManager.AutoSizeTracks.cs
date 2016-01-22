using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private struct AutoSizeTracks
        {
            public AutoSizeTracks(IGridColumnSet columns, IGridRowSet rows)
            {
                Debug.Assert(columns != null);
                Debug.Assert(rows != null);
                Columns = columns;
                Rows = rows;
            }

            public readonly IGridColumnSet Columns;

            public readonly IGridRowSet Rows;

            public bool IsAutoX
            {
                get { return Columns.Count > 0; }
            }

            public bool IsAutoY
            {
                get { return Rows.Count > 0; }
            }
        }
    }
}
