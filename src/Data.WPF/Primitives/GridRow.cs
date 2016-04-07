using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class GridRow : GridTrack, IConcatList<GridRow>
    {
        #region IConcatList<GridRow>

        bool IConcatList<GridRow>.IsReadOnly
        {
            get { return true; }
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

        IConcatList<GridRow> IConcatList<GridRow>.Concat(GridRow item)
        {
            throw new NotSupportedException();
        }

        IConcatList<GridRow> IConcatList<GridRow>.Concat(IConcatList<GridRow> items)
        {
            throw new NotSupportedException();
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

        internal GridRow(Template owner, int ordinal, GridLengthParser.Result result)
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

        public override Orientation Orientation
        {
            get { return Orientation.Vertical; }
        }
    }
}