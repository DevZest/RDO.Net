using DevZest.Data.Utilities;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public struct DataRowCompared
    {
        public readonly DataRow X;
        public readonly DataRow Y;
        public readonly int Result;

        internal DataRowCompared(DataRowComparing comparing, int result)
            : this(comparing.X, comparing.Y, result)
        {
        }

        private DataRowCompared(DataRow x, DataRow y, int result)
        {
            Debug.Assert(x != null);
            Debug.Assert(y != null);
            X = x;
            Y = y;
            Result = result;
        }

        public DataRowCompared ThenBy<T>(Column<T> column, SortDirection direction = SortDirection.Ascending, IComparer<T> comparer = null)
        {
            Check.NotNull(column, nameof(column));
            if (Result != 0)
                return this;

            var result = column.Compare(X, Y, direction, comparer);
            return new DataRowCompared(X, Y, result);
        }
    }
}
