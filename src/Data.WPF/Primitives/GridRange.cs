﻿using System;
using System.Diagnostics;
using System.Globalization;
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
            return Template.InternalGridColumns.Filter(Left.Ordinal, Right.Ordinal, predict, action);
        }

        internal IConcatList<GridRow> FilterRows(Func<GridRow, bool> predict, Action<GridRow> action = null)
        {
            Debug.Assert(!IsEmpty);
            return Template.InternalGridRows.Filter(Top.Ordinal, Bottom.Ordinal, predict, action);
        }

        internal Point MeasuredPoint
        {
            get { return new Point(Left == null ? 0 : Left.StartOffset, Top == null ? 0 : Top.StartOffset); }
        }

        internal Size MeasuredSize
        {
            get { return new Size(MeasuredWidth, MeasuredHeight); }
        }

        internal double MeasuredWidth
        {
            get { return IsEmpty ? 0 : Template.InternalGridColumns.GetMeasuredLength(Left.Ordinal, Right.Ordinal); }
        }

        internal double GetMeasuredWidth(Func<GridColumn, bool> predict)
        {
            return IsEmpty ? 0 : Template.InternalGridColumns.GetMeasuredLength(Left.Ordinal, Right.Ordinal, predict);
        }

        internal double MeasuredHeight
        {
            get { return IsEmpty? 0 : Template.InternalGridRows.GetMeasuredLength(Top.Ordinal, Bottom.Ordinal); }
        }

        internal double GetMeasuredHeight(Func<GridRow, bool> predict)
        {
            return IsEmpty ? 0 : Template.InternalGridRows.GetMeasuredLength(Top.Ordinal, Bottom.Ordinal, predict);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[({0},{1}),({2},{3})]",
                Left == null ? string.Empty : Left.Ordinal.ToString(CultureInfo.InvariantCulture),
                Top == null ? string.Empty : Top.Ordinal.ToString(CultureInfo.InvariantCulture),
                Right == null ? string.Empty : Right.Ordinal.ToString(CultureInfo.InvariantCulture),
                Bottom == null ? string.Empty : Bottom.Ordinal.ToString(CultureInfo.InvariantCulture));
        }
    }
}
