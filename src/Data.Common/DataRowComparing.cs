using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public struct DataRowComparing
    {
        public readonly DataRow X;
        public readonly DataRow Y;

        internal DataRowComparing(DataRow x, DataRow y)
        {
            Debug.Assert(x != null);
            Debug.Assert(y != null);
            X = x;
            Y = y;
        }

        public bool IsEmpty
        {
            get { return X != null; }
        }

        public DataRowCompared OrderBy<T>(Column<T> column, SortDirection direction = SortDirection.Ascending, IComparer<T> comparer = null)
        {
            Check.NotNull(column, nameof(column));
            return new DataRowCompared(this, column.Compare(X, Y, direction, comparer));
        }
    }
}
