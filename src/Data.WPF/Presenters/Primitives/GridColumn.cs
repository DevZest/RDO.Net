using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents the column of flexible grid layout.
    /// </summary>
    public sealed class GridColumn : GridTrack, IConcatList<GridColumn>
    {
        #region IConcatList<GridColumn>

        IConcatList<GridColumn> IConcatList<GridColumn>.Sort(Comparison<GridColumn> comparision)
        {
            return this;
        }

        int IReadOnlyCollection<GridColumn>.Count
        {
            get { return 1; }
        }

        GridColumn IReadOnlyList<GridColumn>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        IEnumerator<GridColumn> IEnumerable<GridColumn>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        bool IConcatList<GridColumn>.IsSealed
        {
            get { return true; }
        }

        IConcatList<GridColumn> IConcatList<GridColumn>.Seal()
        {
            return this;
        }

        #endregion

        internal GridColumn(IGridTrackCollection owner, int ordinal, GridLengthParser.Result result)
            : base(owner, ordinal, result)
        {
        }

        /// <summary>
        /// Gets the width of the grid column.
        /// </summary>
        public GridLength Width
        {
            get { return Length; }
        }

        /// <summary>
        /// Gets the minimum width of the grid column.
        /// </summary>
        public double MinWidth
        {
            get { return MinLength; }
        }

        /// <summary>
        /// Gets the maximum width of the grid column.
        /// </summary>
        public double MaxWidth
        {
            get { return MaxLength; }
        }

        internal override string InvalidStarLengthMessage
        {
            get { return DiagnosticMessages.GridColumn_InvalidStarWidth(Ordinal); }
        }

        internal override string InvalidAutoLengthMessage
        {
            get { return DiagnosticMessages.GridColumn_InvalidAutoWidth(Ordinal); }
        }
    }
}
