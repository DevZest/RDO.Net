using DevZest.Data.Utilities;
using System;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>A combination of <see cref="Column"/> and <see cref="SortDirection"/> for data sorting.</summary>
    public struct ColumnSort
    {
        internal ColumnSort(Column column, SortDirection direction)
        {
            Debug.Assert(column != null);

            Column = column;
            Direction = direction;
        }

        /// <summary>Gets the column.</summary>
        public readonly Column Column;

        /// <summary>Gets the sort direction.</summary>
        public readonly SortDirection Direction;

        /// <summary>Implicitly converts a column into <see cref="ColumnSort"/>, with <see cref="SortDirection.Unspecified"/> direction.</summary>
        /// <param name="x">The column.</param>
        public static implicit operator ColumnSort(Column x)
        {
            Check.NotNull(x, nameof(x));
            return new ColumnSort(x, SortDirection.Unspecified);
        }
    }
}
