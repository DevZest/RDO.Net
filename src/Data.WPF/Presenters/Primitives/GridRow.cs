using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents the row of flexible grid layout.
    /// </summary>
    public sealed class GridRow : GridTrack, IConcatList<GridRow>
    {
        #region IConcatList<GridRow>

        IConcatList<GridRow> IConcatList<GridRow>.Sort(Comparison<GridRow> comparision)
        {
            return this;
        }

        int IReadOnlyCollection<GridRow>.Count
        {
            get { return 1; }
        }

        GridRow IReadOnlyList<GridRow>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        IEnumerator<GridRow> IEnumerable<GridRow>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        bool IConcatList<GridRow>.IsSealed
        {
            get { return true; }
        }

        IConcatList<GridRow> IConcatList<GridRow>.Seal()
        {
            return this;
        }
        #endregion

        internal GridRow(IGridTrackCollection owner, int ordinal, GridLengthParser.Result result)
            : base(owner, ordinal, result)
        {
        }

        /// <summary>
        /// Gets the height of the grid row.
        /// </summary>
        public GridLength Height
        {
            get { return Length; }
        }

        /// <summary>
        /// Gets the minimum height of the grid row.
        /// </summary>
        public double MinHeight
        {
            get { return MinLength; }
        }

        /// <summary>
        /// Gets the maximum height of the grid row.
        /// </summary>
        public double MaxHeight
        {
            get { return MaxLength; }
        }

        internal override string InvalidStarLengthMessage
        {
            get { return DiagnosticMessages.GridRow_InvalidStarHeight(Ordinal); }
        }

        internal override string InvalidAutoLengthMessage
        {
            get { return DiagnosticMessages.GridRow_InvalidAutoHeight(Ordinal); }
        }
    }
}