using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private struct AutoSizeEntry
        {
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
        }
    }
}
