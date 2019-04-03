using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
{
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

        public GridLength Width
        {
            get { return Length; }
        }

        public double MinWidth
        {
            get { return MinLength; }
        }

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
