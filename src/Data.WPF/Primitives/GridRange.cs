using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
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

        internal Template Template
        {
            get { return Left == null ? null : Left.Owner; }
        }

        public bool IsEmpty
        {
            get { return Template == null; }
        }

        public bool Contains(GridRange gridRange)
        {
            if (IsEmpty || Template != gridRange.Template)
                return false;
            return Left.Ordinal <= gridRange.Left.Ordinal && Right.Ordinal >= gridRange.Right.Ordinal
                && Top.Ordinal <= gridRange.Top.Ordinal && Bottom.Ordinal >= gridRange.Bottom.Ordinal;
        }

        public bool Contains(GridColumn gridColumn)
        {
            if (IsEmpty || Template != gridColumn.Owner)
                return false;

            return Left.Ordinal <= gridColumn.Ordinal && Right.Ordinal >= gridColumn.Ordinal;
        }

        public bool Contains(GridRow gridRow)
        {
            if (IsEmpty || Template != gridRow.Owner)
                return false;

            return Top.Ordinal <= gridRow.Ordinal && Bottom.Ordinal >= gridRow.Ordinal;
        }

        public bool IntersectsWith(GridRange gridRange)
        {
            if (IsEmpty || Template != gridRange.Template)
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

            if (Template != gridRange.Template)
                throw new ArgumentException(Strings.GridRange_InvalidOwner, nameof(gridRange));

            return new GridRange(
                Left.Ordinal < gridRange.Left.Ordinal ? Left : gridRange.Left,
                Top.Ordinal < gridRange.Top.Ordinal ? Top : gridRange.Top,
                Right.Ordinal > gridRange.Right.Ordinal ? Right : gridRange.Right,
                Bottom.Ordinal > gridRange.Bottom.Ordinal ? Bottom : gridRange.Bottom);
        }

        internal IConcatList<GridColumn> FilterColumns(Func<GridColumn, bool> predict, Action<GridColumn> action = null)
        {
            Debug.Assert(!IsEmpty);
            return Template.GridColumns.Filter(Left.Ordinal, Right.Ordinal, predict, action);
        }

        internal IConcatList<GridRow> FilterRows(Func<GridRow, bool> predict, Action<GridRow> action = null)
        {
            Debug.Assert(!IsEmpty);
            return Template.GridRows.Filter(Top.Ordinal, Bottom.Ordinal, predict, action);
        }

        internal double MeasuredWidth
        {
            get { return Template.GridColumns.GetMeasuredLength(Left.Ordinal, Right.Ordinal); }
        }

        internal double GetMeasuredWidth(Func<GridColumn, bool> predict)
        {
            return Template.GridColumns.GetMeasuredLength(Left.Ordinal, Right.Ordinal, predict);
        }

        internal double MeasuredHeight
        {
            get { return Template.GridRows.GetMeasuredLength(Top.Ordinal, Bottom.Ordinal); }
        }

        internal double GetMeasuredHeight(Func<GridRow, bool> predict)
        {
            return Template.GridRows.GetMeasuredLength(Top.Ordinal, Bottom.Ordinal, predict);
        }

        internal Size GetMeasuredSize(BlockView blockView)
        {
            double width = 0;
            var blockViewX = Template.Orientation == Orientation.Horizontal ? blockView : null;
            for (int i = Left.Ordinal; i <= Right.Ordinal; i++)
                width += Template.GridColumns[i].GetMeasuredLength(blockViewX);

            double height = 0;
            var blockViewY = Template.Orientation == Orientation.Vertical ? blockView : null;
            for (int i = Top.Ordinal; i <= Bottom.Ordinal; i++)
                height += Template.GridRows[i].GetMeasuredLength(blockViewY);

            return new Size(width, height);
        }
    }
}
