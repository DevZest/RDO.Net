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

        IConcatList<GridColumn> IConcatList<GridColumn>.Concat(GridColumn item)
        {
            throw new NotSupportedException();
        }

        IConcatList<GridColumn> IConcatList<GridColumn>.Concat(IConcatList<GridColumn> items)
        {
            throw new NotSupportedException();
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

        internal GridColumn(Template owner, int ordinal, GridLengthParser.Result result)
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

        public override Orientation Orientation
        {
            get { return Orientation.Horizontal; }
        }
    }
}
