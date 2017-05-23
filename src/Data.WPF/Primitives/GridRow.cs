using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Windows.Primitives
{
    public sealed class GridRow : GridTrack, IConcatList<GridRow>
    {
        #region IConcatList<GridRow>

        void IConcatList<GridRow>.Sort(Comparison<GridRow> comparision)
        {
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

        #endregion

        internal GridRow(IGridTrackCollection owner, int ordinal, GridLengthParser.Result result)
            : base(owner, ordinal, result)
        {
        }

        public GridLength Height
        {
            get { return Length; }
        }

        public double MinHeight
        {
            get { return MinLength; }
        }

        public double MaxHeight
        {
            get { return MaxLength; }
        }

        internal override string InvalidStarLengthMessage
        {
            get { return Strings.GridRow_InvalidStarHeight(Ordinal); }
        }

        internal override string InvalidAutoLengthMessage
        {
            get { return Strings.GridRow_InvalidAutoHeight(Ordinal); }
        }
    }
}