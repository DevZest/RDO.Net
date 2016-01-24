using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Windows
{
    // https://www.w3.org/TR/css-grid-1/
    public sealed partial class GridTemplate
    {
        private class GridTrackCollection<T> : ReadOnlyCollection<T>
            where T : GridTrack
        {
            internal GridTrackCollection()
                : base(new List<T>())
            {
            }

            internal void Add(T item)
            {
                Items.Add(item);
            }
        }

        private sealed class TemplateItemCollection<T> : ReadOnlyCollection<T>
            where T : TemplateItem
        {
            internal TemplateItemCollection()
                : base(new List<T>())
            {
            }

            internal GridRange Range { get; private set; }

            internal void Add(GridRange gridRange, T item)
            {
                Debug.Assert(item != null);
                Items.Add(item);
                Range = Range.Union(gridRange);
            }
        }

        internal GridTemplate(DataView owner)
        {
            Owner = owner;
        }

        public DataView Owner { get; private set; }

        public Model Model
        {
            get { return Owner.Model; }
        }

        private ListOrientation _listOrientation = ListOrientation.Y;
        public ListOrientation ListOrientation
        {
            get { return _listOrientation; }
            internal set
            {
                VerifyGridColumnWidth(value, 0, GridColumns.Count - 1, nameof(value));
                VerifyGridRowHeight(value, 0, GridRows.Count - 1, nameof(value));
                _listOrientation = value;
            }
        }

        private GridTrackCollection<GridColumn> _gridColumns = new GridTrackCollection<GridColumn>();
        public ReadOnlyCollection<GridColumn> GridColumns
        {
            get { return _gridColumns; }
        }

        private GridTrackCollection<GridRow> _gridRows = new GridTrackCollection<GridRow>();
        public ReadOnlyCollection<GridRow> GridRows
        {
            get { return _gridRows; }
        }

        private TemplateItemCollection<ScalarItem> _scalarItems = new TemplateItemCollection<ScalarItem>();
        public ReadOnlyCollection<ScalarItem> ScalarItems
        {
            get { return _scalarItems; }
        }

        private TemplateItemCollection<ListItem> _listItems = new TemplateItemCollection<ListItem>();
        public ReadOnlyCollection<ListItem> ListItems
        {
            get { return _listItems; }
        }

        private TemplateItemCollection<ChildItem> _childItems = new TemplateItemCollection<ChildItem>();
        public ReadOnlyCollection<ChildItem> ChildItems
        {
            get { return _childItems; }
        }

        private GridRange? _listRange;
        public GridRange ListRange
        {
            get { return _listRange.HasValue ? _listRange.GetValueOrDefault() : AutoListRange; }
            internal set
            {
                if (!value.Contains(AutoListRange))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _listRange = value;
            }
        }

        private GridRange AutoListRange
        {
            get { return _listItems.Range.Union(_childItems.Range); }
        }

        internal int AddGridColumn(string width)
        {
            var gridWidth = GridLengthParser.Parse(width);
            _gridColumns.Add(new GridColumn(this, GridColumns.Count, gridWidth));
            var result = GridColumns.Count - 1;
            VerifyGridColumnWidth(ListOrientation, result, result, nameof(width));
            if (gridWidth.IsStar)
                StarSizeColumnsCount++;
            else if (gridWidth.IsAuto)
                AutoSizeColumnsCount++;
            return result;
        }

        internal void AddGridColumns(params string[] widths)
        {
            Debug.Assert(widths != null);

            foreach (var width in widths)
                AddGridColumn(width);
        }

        internal int AddGridRow(string height)
        {
            var gridHeight = GridLengthParser.Parse(height);
            _gridRows.Add(new GridRow(this, GridRows.Count, gridHeight));
            var result = GridRows.Count - 1;
            VerifyGridRowHeight(ListOrientation, result, result, nameof(height));
            if (gridHeight.IsStar)
                StarSizeRowsCount++;
            else if (gridHeight.IsAuto)
                AutoSizeRowsCount++;
            return result;
        }

        internal void AddGridRows(params string[] heights)
        {
            Debug.Assert(heights != null);

            foreach (var height in heights)
                AddGridRow(height);
        }

        private void VerifyGridRowHeight(ListOrientation orientation, int startIndex, int endIndex, string paramName)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!IsValidGridRowHeight(GridRows[i], orientation))
                    throw new ArgumentException(Strings.GridTemplate_InvalidGridRowHeightOrientation(i, GridRows[i].Height, orientation), paramName);
            }
        }

        private static bool IsValidGridRowHeight(GridRow gridRow, ListOrientation orentation)
        {
            var height = gridRow.Height;

            if (height.IsAbsolute)
                return true;

            if (height.IsStar)
                return orentation != ListOrientation.Y && orentation != ListOrientation.YX && orentation != ListOrientation.XY;

            Debug.Assert(height.IsAuto);
            return orentation != ListOrientation.YX;
        }

        private void VerifyGridColumnWidth(ListOrientation orientation, int startIndex, int endIndex, string paramName)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!IsValidGridColumnWidth(GridColumns[i], orientation))
                    throw new ArgumentException(Strings.GridTemplate_InvalidGridColumnWidthOrientation(i, GridColumns[i].Width, orientation), paramName);
            }
        }

        private static bool IsValidGridColumnWidth(GridColumn gridColumn, ListOrientation orentation)
        {
            var width = gridColumn.Width;

            if (width.IsAbsolute)
                return true;

            if (width.IsStar)
                return orentation != ListOrientation.X && orentation != ListOrientation.YX && orentation != ListOrientation.XY;

            Debug.Assert(width.IsAuto);
            return orentation != ListOrientation.XY;
        }

        internal int ScalarItemsCountBeforeList { get; private set; }

        internal int StarSizeColumnsCount { get; private set; }

        internal int StarSizeRowsCount { get; private set; }

        internal int AutoSizeColumnsCount { get; private set; }

        internal int AutoSizeRowsCount { get; private set; }

        internal void AddScalarItem(GridRange gridRange, ScalarItem scalarItem)
        {
            VerifyAddTemplateItem(gridRange, scalarItem, nameof(scalarItem), true);
            scalarItem.Construct(this, gridRange, _scalarItems.Count);
            _scalarItems.Add(gridRange, scalarItem);
            if (_listItems.Count == 0)
                ScalarItemsCountBeforeList = _scalarItems.Count;
        }

        internal void AddListItem(GridRange gridRange, ListItem listItem)
        {
            VerifyAddTemplateItem(gridRange, listItem, nameof(listItem), true);
            listItem.Construct(this, gridRange, _listItems.Count);
            _listItems.Add(gridRange, listItem);
        }

        internal void AddChildItem(GridRange gridRange, ChildItem childItem)
        {
            VerifyAddTemplateItem(gridRange, childItem, nameof(childItem), false);
            childItem.Seal(this, gridRange, _listItems.Count, _childItems.Count);
            _listItems.Add(gridRange, childItem);
            _childItems.Add(gridRange, childItem);
        }

        private void VerifyAddTemplateItem(GridRange gridRange, TemplateItem templateItem, string paramTemplateItemName, bool isScalar)
        {
            if (!GetGridRangeAll().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
            if (!isScalar && _listRange.HasValue && !_listRange.GetValueOrDefault().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
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

        public int PinnedLeft { get; internal set; }

        public int PinnedTop { get; internal set; }

        public int PinnedRight { get; internal set; }

        public int PinnedBottom { get; internal set; }

        internal bool AllowsInfiniteX
        {
            get { return ListOrientation == ListOrientation.X || ListOrientation == ListOrientation.YX ? true : !GridColumns.Any(x => x.Length.IsStar); }
        }

        internal bool AllowsInfiniteY
        {
            get { return ListOrientation == ListOrientation.Y || ListOrientation == ListOrientation.XY ? true : !GridRows.Any(x => x.Length.IsStar); }
        }
    }
}
