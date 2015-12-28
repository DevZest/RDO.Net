using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public struct GridRange
    {
        internal GridRange(GridColumn left, GridRow top)
            : this(left, top, left, top)
        {
        }

        internal GridRange(GridColumn left, GridRow top, GridColumn right, GridRow bottom)
        {
            Debug.Assert(left != null);
            Debug.Assert(top != null && top.Owner == left.Owner);
            Debug.Assert(right != null && right.Owner == top.Owner);
            Debug.Assert(bottom != null && bottom.Owner == right.Owner);
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public readonly GridColumn Left;
        public readonly GridRow Top;
        public readonly GridColumn Right;
        public readonly GridRow Bottom;

        internal GridTemplate Owner
        {
            get { return Left == null ? null : Left.Owner; }
        }

        public bool IsEmpty
        {
            get { return Owner == null; }
        }

        public bool Contains(GridRange gridRange)
        {
            if (IsEmpty || Owner != gridRange.Owner)
                return false;
            return Left.Ordinal <= gridRange.Left.Ordinal && Right.Ordinal >= gridRange.Right.Ordinal
                && Top.Ordinal <= gridRange.Top.Ordinal && Bottom.Ordinal >= gridRange.Bottom.Ordinal;
        }

        public bool Contains(GridColumn gridColumn)
        {
            if (IsEmpty || Owner != gridColumn.Owner)
                return false;

            return Left.Ordinal <= gridColumn.Ordinal && Right.Ordinal >= gridColumn.Ordinal;
        }

        public bool Contains(GridRow gridRow)
        {
            if (IsEmpty || Owner != gridRow.Owner)
                return false;

            return Top.Ordinal <= gridRow.Ordinal && Bottom.Ordinal >= gridRow.Ordinal;
        }

        public bool IntersectsWith(GridRange gridRange)
        {
            if (IsEmpty || Owner != gridRange.Owner)
                return false;

            return gridRange.Left.Ordinal <= this.Right.Ordinal
                && gridRange.Right.Ordinal >= this.Left.Ordinal
                && gridRange.Top.Ordinal <= this.Bottom.Ordinal
                && gridRange.Bottom.Ordinal >= this.Top.Ordinal;
        }

        public GridRange Union(GridRange gridRange)
        {
            if (gridRange.IsEmpty)
                return this;

            if (this.IsEmpty)
                return gridRange;

            if (Owner != gridRange.Owner)
                throw new ArgumentException(Strings.GridRange_InvalidOwner, nameof(gridRange));

            return new GridRange(
                Left.Ordinal < gridRange.Left.Ordinal ? Left : gridRange.Left,
                Top.Ordinal < gridRange.Top.Ordinal ? Top : gridRange.Top,
                Right.Ordinal > gridRange.Right.Ordinal ? Right : gridRange.Right,
                Bottom.Ordinal > gridRange.Bottom.Ordinal ? Bottom : gridRange.Bottom);
        }
    }
}
