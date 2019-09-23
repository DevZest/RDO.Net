using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents a range of grid cells.
    /// </summary>
    public struct GridRange
    {
        internal GridRange(GridColumn left, GridRow top)
            : this(left, top, left, top)
        {
        }

        internal GridRange(GridColumn left, GridRow top, GridColumn right, GridRow bottom)
        {
            Debug.Assert(left != null);
            Debug.Assert(top != null && top.Template == left.Template);
            Debug.Assert(right != null && right.Template == top.Template);
            Debug.Assert(bottom != null && bottom.Template == right.Template);
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Gets the left grid column.
        /// </summary>
        public readonly GridColumn Left;

        /// <summary>
        /// Gets the top grid row.
        /// </summary>
        public readonly GridRow Top;

        /// <summary>
        /// Gets the right grid column.
        /// </summary>
        public readonly GridColumn Right;

        /// <summary>
        /// Gets the bottom grid row.
        /// </summary>
        public readonly GridRow Bottom;

        internal Template Template
        {
            get { return Left == null ? null : Left.Template; }
        }

        /// <summary>
        /// Gets a value indicates whether this <see cref="GridRange"/> is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return Template == null; }
        }

        /// <summary>
        /// Determines whether this <see cref="GridRange"/> contains other <see cref="GridRange"/>.
        /// </summary>
        /// <param name="gridRange">The other <see cref="GridRange"/>.</param>
        /// <returns><see langword="true"/> if this <see cref="GridRange"/> contains other <see cref="GridRange"/>, otherwise <see langword="false"/>.</returns>
        public bool Contains(GridRange gridRange)
        {
            if (IsEmpty || Template != gridRange.Template)
                return false;
            return Left.Ordinal <= gridRange.Left.Ordinal && Right.Ordinal >= gridRange.Right.Ordinal
                && Top.Ordinal <= gridRange.Top.Ordinal && Bottom.Ordinal >= gridRange.Bottom.Ordinal;
        }

        /// <summary>
        /// Determines whether this <see cref="GridRange"/> contains specified <see cref="GridColumn"/>.
        /// </summary>
        /// <param name="gridColumn">The specified <see cref="GridColumn"/>.</param>
        /// <returns><see langword="true"/> if this <see cref="GridRange"/> contains specified <see cref="GridColumn"/>, otherwise <see langword="false"/>.</returns>
        public bool Contains(GridColumn gridColumn)
        {
            if (IsEmpty || Template != gridColumn.Template)
                return false;

            return Contains(Left, Right, gridColumn);
        }

        /// <summary>
        /// Determines whether this <see cref="GridRange"/> contains specified <see cref="GridRow"/>.
        /// </summary>
        /// <param name="gridRow">The specified <see cref="GridRow"/>.</param>
        /// <returns><see langword="true"/> if this <see cref="GridRange"/> contains specified <see cref="GridRow"/>, otherwise <see langword="false"/>.</returns>
        public bool Contains(GridRow gridRow)
        {
            if (IsEmpty || Template != gridRow.Template)
                return false;

            return Contains(Top, Bottom, gridRow);
        }

        /// <summary>
        /// Determines whether this <see cref="GridRange"/> intersects other <see cref="GridRange"/>.
        /// </summary>
        /// <param name="gridRange">The other <see cref="GridRange"/>.</param>
        /// <returns><see langword="true"/> if this <see cref="GridRange"/> intersects other <see cref="GridRange"/>, otherwise <see langword="false"/>.</returns>
        public bool IntersectsWith(GridRange gridRange)
        {
            if (IsEmpty || Template != gridRange.Template)
                return false;

            return gridRange.Left.Ordinal <= this.Right.Ordinal
                && gridRange.Right.Ordinal >= this.Left.Ordinal
                && gridRange.Top.Ordinal <= this.Bottom.Ordinal
                && gridRange.Bottom.Ordinal >= this.Top.Ordinal;
        }

        /// <summary>
        /// Creates a new range defined as the union of this and the specified range.
        /// </summary>
        /// <param name="gridRange">The specified range.</param>
        /// <returns>The new range.</returns>
        public GridRange Union(GridRange gridRange)
        {
            if (gridRange.IsEmpty)
                return this;

            if (this.IsEmpty)
                return gridRange;

            if (Template != gridRange.Template)
                throw new ArgumentException(DiagnosticMessages.GridRange_InvalidOwner, nameof(gridRange));

            return new GridRange(
                Left.Ordinal < gridRange.Left.Ordinal ? Left : gridRange.Left,
                Top.Ordinal < gridRange.Top.Ordinal ? Top : gridRange.Top,
                Right.Ordinal > gridRange.Right.Ordinal ? Right : gridRange.Right,
                Bottom.Ordinal > gridRange.Bottom.Ordinal ? Bottom : gridRange.Bottom);
        }

        internal IConcatList<GridColumn> FilterColumns(Func<GridColumn, bool> predict, Action<GridColumn> action = null)
        {
            Debug.Assert(!IsEmpty);
            return Template.InternalGridColumns.Filter(this, predict, action);
        }

        internal IConcatList<GridRow> FilterRows(Func<GridRow, bool> predict, Action<GridRow> action = null)
        {
            Debug.Assert(!IsEmpty);
            return Template.InternalGridRows.Filter(this, predict, action);
        }

        internal Point MeasuredPosition
        {
            get { return new Point(Left == null ? 0 : Left.StartOffset, Top == null ? 0 : Top.StartOffset); }
        }

        internal Size MeasuredSize
        {
            get { return new Size(MeasuredWidth, MeasuredHeight); }
        }

        internal double MeasuredWidth
        {
            get { return IsEmpty ? 0 : Template.InternalGridColumns.GetMeasuredLength(ColumnSpan); }
        }

        internal double GetMeasuredWidth(Func<GridColumn, bool> predict)
        {
            return IsEmpty ? 0 : Template.InternalGridColumns.GetMeasuredLength(ColumnSpan, predict);
        }

        internal double MeasuredHeight
        {
            get { return IsEmpty? 0 : Template.InternalGridRows.GetMeasuredLength(RowSpan); }
        }

        internal double GetMeasuredHeight(Func<GridRow, bool> predict)
        {
            return IsEmpty ? 0 : Template.InternalGridRows.GetMeasuredLength(RowSpan, predict);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[({0},{1}),({2},{3})]",
                Left == null ? string.Empty : Left.Ordinal.ToString(CultureInfo.InvariantCulture),
                Top == null ? string.Empty : Top.Ordinal.ToString(CultureInfo.InvariantCulture),
                Right == null ? string.Empty : Right.Ordinal.ToString(CultureInfo.InvariantCulture),
                Bottom == null ? string.Empty : Bottom.Ordinal.ToString(CultureInfo.InvariantCulture));
        }

        internal bool HorizontallyIntersectsWith(int gridOrdinal)
        {
            return gridOrdinal > Left.Ordinal && gridOrdinal < Right.Ordinal;
        }

        internal bool VerticallyIntersectsWith(int gridOrdinal)
        {
            return gridOrdinal > Top.Ordinal && gridOrdinal < Bottom.Ordinal;
        }

        private static bool Contains<T>(T headTrack, T tailTrack, T gridTrack)
            where T : GridTrack
        {
            return gridTrack.Ordinal >= headTrack.Ordinal && gridTrack.Ordinal <= tailTrack.Ordinal;
        }

        internal GridSpan<GridColumn> ColumnSpan
        {
            get { return new GridSpan<GridColumn>(Left, Right); }
        }

        internal GridSpan<GridRow> RowSpan
        {
            get { return new GridSpan<GridRow>(Top, Bottom); }
        }

        internal Point GetRelativePosition(GridRange innerGridRange)
        {
            Debug.Assert(Contains(innerGridRange));

            var originPosition = MeasuredPosition;
            var position = innerGridRange.MeasuredPosition;
            return new Point(position.X - originPosition.X, position.Y - originPosition.Y);
        }
    }
}
