using DevZest.Data.Utilities;
using System;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>A combination of <see cref="Column"/> and <see cref="SortDirection"/> for data sorting.</summary>
    public struct OrderBy
    {
        internal OrderBy(Column column, SortDirection direction)
        {
            Debug.Assert(column != null);

            Column = column;
            Direction = direction;
        }

        /// <summary>Gets the column.</summary>
        public readonly Column Column;

        /// <summary>Gets the sort direction.</summary>
        public readonly SortDirection Direction;

        /// <summary>Implicitly converts a column into <see cref="OrderBy"/>, with <see cref="SortDirection.Unspecified"/> direction.</summary>
        /// <param name="x">The column.</param>
        public static implicit operator OrderBy(Column x)
        {
            Check.NotNull(x, nameof(x));
            return new OrderBy(x, SortDirection.Unspecified);
        }
    }
}
