using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class GridColumn : GridTrack, IConcatList<GridColumn>
    {
        #region IConcatList<GridColumn>

        bool IConcatList<GridColumn>.IsReadOnly
        {
            get { return true; }
        }

        void IConcatList<GridColumn>.Sort(Comparison<GridColumn> comparision)
        {
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
            get { return Strings.GridColumn_InvalidStarWidth(Ordinal); }
        }

        internal override string InvalidAutoLengthMessage
        {
            get { return Strings.GridColumn_InvalidAutoWidth(Ordinal); }
        }
    }
}
