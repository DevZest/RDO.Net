using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class GridTemplate
    {
        internal GridTemplate(Model model)
        {
            Model = model;
            GridRows = new GridSpecCollection<GridRow>();
            GridColumns = new GridSpecCollection<GridColumn>();
            ScalarItems = new GridItemCollection<ScalarGridItem>(this);
            ListItems = new GridItemCollection<ListGridItem>(this);
            ChildItems = new GridItemCollection<ChildGridItem>(this);
        }

        internal void Seal()
        {
            _isSealed = true;
        }

        internal int ChildOrdinal { get; private set; }

        bool _isSealed = false;
        public bool IsSealed
        {
            get { return _isSealed; }
        }
        private void VerifyNotSealed()
        {
            if (_isSealed)
                throw new InvalidOperationException(Strings.GridTemplate_VerifyNotSealed);
        }

        public Model Model { get; private set; }

        private RepeatOrientation _orientation = RepeatOrientation.Y;
        public RepeatOrientation Orientation
        {
            get { return _orientation; }
            set
            {
                VerifyNotSealed();
                VerifyGridColumnWidth(value, 0, GridColumns.Count - 1, nameof(value));
                VerifyGridRowHeight(value, 0, GridRows.Count - 1, nameof(value));
                _orientation = value;
            }
        }

        public GridTemplate WithOrientation(RepeatOrientation value)
        {
            VerifyNotSealed();
            VerifyGridColumnWidth(value, 0, GridColumns.Count - 1, nameof(value));
            VerifyGridRowHeight(value, 0, GridRows.Count - 1, nameof(value));
            _orientation = value;
            return this;
        }

        private bool _showsEof;
        public bool ShowsEof
        {
            get { return _showsEof; }
            set
            {
                VerifyNotSealed();
                _showsEof = value;
            }
        }

        public GridTemplate WithShowsEof(bool value)
        {
            ShowsEof = value;
            return this;
        }

        private bool _showsEmptyDataRow;
        public bool ShowsEmptyDataRow
        {
            get { return _showsEmptyDataRow; }
            set
            {
                VerifyNotSealed();
                _showsEmptyDataRow = value;
            }
        }

        public GridTemplate WithShowsEmptyDataRow(bool value)
        {
            ShowsEmptyDataRow = value;
            return this;
        }

        public GridSpecCollection<GridRow> GridRows { get; private set; }

        public GridSpecCollection<GridColumn> GridColumns { get; private set; }

        public GridItemCollection<ScalarGridItem> ScalarItems { get; private set; }

        public GridItemCollection<ListGridItem> ListItems { get; private set; }

        public GridItemCollection<ChildGridItem> ChildItems { get; private set; }

        private GridRange? _repeatRange;
        public GridRange RepeatRange
        {
            get { return _repeatRange.HasValue ? _repeatRange.GetValueOrDefault() : AutoRepeatRange; }
            set
            {
                VerifyNotSealed();
                if (!value.Contains(AutoRepeatRange))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _repeatRange = value;
            }
        }

        private GridRange AutoRepeatRange
        {
            get { return ListItems.Range.Union(ChildItems.Range); }
        }

        public GridTemplate AddGridRows(params string[] heights)
        {
            if (heights == null)
                throw new ArgumentNullException(nameof(heights));

            foreach (var height in heights)
                AddGridRow(height);
            return this;
        }

        public GridTemplate AddGridColumns(params string[] widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));

            foreach (var width in widths)
                AddGridColumn(width);
            return this;
        }

        private int AddGridColumn(string width)
        {
            VerifyNotSealed();
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GridLengthParser.Parse(width)));
            var result = GridColumns.Count - 1;
            VerifyGridColumnWidth(Orientation, result, result, nameof(width));
            return result;
        }

        public GridTemplate AddGridColumn(string width, out int index)
        {
            index = AddGridColumn(width);
            return this;
        }

        private int AddGridRow(string height)
        {
            VerifyNotSealed();
            GridRows.Add(new GridRow(this, GridRows.Count, GridLengthParser.Parse(height)));
            var result = GridRows.Count - 1;
            VerifyGridRowHeight(Orientation, result, result, nameof(height));
            return result;
        }

        public GridTemplate AddGridRow(string height, out int index)
        {
            index = AddGridRow(height);
            return this;
        }

        private void VerifyGridRowHeight(RepeatOrientation orientation, int startIndex, int endIndex, string paramName)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!IsValidGridRowHeight(GridRows[i], orientation))
                    throw new ArgumentException(Strings.GridTemplate_InvalidGridRowHeightOrientation(i, GridRows[i].Height, orientation), paramName);
            }
        }

        private static bool IsValidGridRowHeight(GridRow gridRow, RepeatOrientation orentation)
        {
            var height = gridRow.Height;

            if (height.IsAbsolute)
                return true;

            if (height.IsStar)
                return orentation != RepeatOrientation.Y && orentation != RepeatOrientation.YX && orentation != RepeatOrientation.XY;

            Debug.Assert(height.IsAuto);
            return orentation != RepeatOrientation.YX;
        }

        private void VerifyGridColumnWidth(RepeatOrientation orientation, int startIndex, int endIndex, string paramName)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!IsValidGridColumnWidth(GridColumns[i], orientation))
                    throw new ArgumentException(Strings.GridTemplate_InvalidGridColumnWidthOrientation(i, GridColumns[i].Width, orientation), paramName);
            }
        }

        private static bool IsValidGridColumnWidth(GridColumn gridColumn, RepeatOrientation orentation)
        {
            var width = gridColumn.Width;

            if (width.IsAbsolute)
                return true;

            if (width.IsStar)
                return orentation != RepeatOrientation.X && orentation != RepeatOrientation.YX && orentation != RepeatOrientation.XY;

            Debug.Assert(width.IsAuto);
            return orentation != RepeatOrientation.XY;
        }

        internal GridTemplate AddItem<T>(GridRange gridRange, ScalarGridItem<T> scalarItem)
            where T : UIElement, new()
        {
            VerifyAddItem(gridRange, scalarItem, nameof(scalarItem), true);
            ScalarItems.Add(gridRange, scalarItem);
            return this;
        }

        internal GridTemplate AddItem<T>(GridRange gridRange, ListGridItem<T> listItem)
            where T : UIElement, new()
        {
            VerifyAddItem(gridRange, listItem, nameof(listItem), true);
            ListItems.Add(gridRange, listItem);
            return this;
        }

        internal GridTemplate AddItem<T>(GridRange gridRange, ChildGridItem<T> childItem)
            where T : DataSetView, new()
        {
            VerifyAddItem(gridRange, childItem, nameof(childItem), false);
            var childTemplate = childItem.Template;
            childTemplate.ChildOrdinal = ChildItems.Count;
            ChildItems.Add(gridRange, childItem);
            return this;
        }

        private void VerifyAddItem(GridRange gridRange, GridItem gridItem, string paramGridItemName, bool isScalar)
        {
            VerifyNotSealed();
            if (!GetGridRangeAll().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
            if (!isScalar && _repeatRange.HasValue && !_repeatRange.GetValueOrDefault().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
            if (gridItem == null)
                throw new ArgumentNullException(paramGridItemName);
            if (gridItem.Owner != null || (gridItem.ParentModel != null && gridItem.ParentModel != Model))
                throw new ArgumentException(Strings.GridTemplate_InvalidGridItem, paramGridItemName);
        }

        private void VerifyGridColumn(int index, string paramName)
        {
            if (index < 0 || index >= GridColumns.Count)
                throw new ArgumentOutOfRangeException(paramName);
        }

        private void VerifyGridRow(int index, string paramName)
        {
            if (index < 0 || index >= GridRows.Count)
                throw new ArgumentOutOfRangeException(paramName);
        }

        private GridRange GetGridRangeAll()
        {
            if (GridColumns.Count == 0 || GridRows.Count == 0)
                return new GridRange();

            return new GridRange(GridColumns[0], GridRows[0], GridColumns[GridColumns.Count - 1], GridRows[GridRows.Count - 1]);
        }

        public GridRange Range()
        {
            return GridColumns.Count == 0 || GridRows.Count == 0 ? new GridRange() : Range(0, 0, GridColumns.Count - 1, GridRows.Count - 1);
        }

        public GridRange Range(int column, int row)
        {
            VerifyGridColumn(column, nameof(column));
            VerifyGridRow(row, nameof(row));
            return new GridRange(GridColumns[column], GridRows[row]);
        }

        public GridRange Range(int left, int top, int right, int bottom)
        {
            VerifyGridColumn(left, nameof(left));
            VerifyGridRow(top, nameof(top));
            VerifyGridColumn(right, nameof(right));
            VerifyGridRow(bottom, nameof(bottom));
            if (right < left)
                throw new ArgumentOutOfRangeException(nameof(right));
            if (bottom < top)
                throw new ArgumentOutOfRangeException(nameof(bottom));
            return new GridRange(GridColumns[left], GridRows[top], GridColumns[right], GridRows[bottom]);
        }

        private bool _isVirtualizing = true;
        public bool IsVirtualizing
        {
            get { return _isVirtualizing; }
            set
            {
                VerifyNotSealed();
                _isVirtualizing = value;
            }
        }

        public GridTemplate WithIsVirtualizing(bool value)
        {
            IsVirtualizing = value;
            return this;
        }

        private int _pinnedLeft;
        public int PinnedLeft
        {
            get { return _pinnedLeft; }
            set
            {
                VerifyNotSealed();
                _pinnedLeft = value;
            }
        }

        public GridTemplate WithPinnedLeft(int value)
        {
            PinnedLeft = value;
            return this;
        }

        private int _pinnedTop;
        public int PinnedTop
        {
            get { return _pinnedTop; }
            set
            {
                VerifyNotSealed();
                _pinnedTop = value;
            }
        }

        public GridTemplate WithPinnedTop(int value)
        {
            PinnedTop = value;
            return this;
        }

        private int _pinnedRight;
        public int PinnedRight
        {
            get { return _pinnedRight; }
            set
            {
                VerifyNotSealed();
                _pinnedRight = value;
            }
        }

        public GridTemplate WithPinnedRight(int value)
        {
            PinnedRight = value;
            return this;
        }

        private int _pinnedBottom;
        public int PinnedBottom
        {
            get { return _pinnedBottom; }
            set
            {
                VerifyNotSealed();
                _pinnedBottom = value;
            }
        }

        public GridTemplate WithPinnedBottom(int value)
        {
            PinnedBottom = value;
            return this;
        }

        public GridTemplate Pin(int left, int top, int right, int bottom)
        {
            VerifyNotSealed();
            PinnedLeft = left;
            PinnedTop = top;
            PinnedRight = right;
            PinnedBottom = bottom;
            return this;
        }

        public bool IsPinned
        {
            get { return PinnedLeft > 0 || PinnedTop > 0 || PinnedRight > 0 || PinnedBottom > 0; }
        }

        internal void DefaultInitialize()
        {
            var columns = Model.GetColumns();

            this.AddGridColumns(columns.Select(x => "Auto").ToArray())
                .AddGridRows("Auto", "Auto")
                .Range(0, 1, columns.Count - 1, 1).Repeat();

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                //this.AddScalarItem(this[i, 0], column)
                //    .AddSetItem(this[i, 1], column.TextBlock());
            }
        }
    }
}
