using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed partial class GridTemplate
    {
        private class GridSpecCollection<T> : ReadOnlyCollection<T>
            where T : GridSpec
        {
            internal GridSpecCollection()
                : base(new List<T>())
            {
            }

            internal void Add(T item)
            {
                Items.Add(item);
            }
        }

        private sealed class GridItemCollection<T> : ReadOnlyCollection<T>
            where T : GridItem
        {
            internal GridItemCollection(GridTemplate owner)
                : base(new List<T>())
            {
                Debug.Assert(owner != null);
                _owner = owner;
            }

            private GridTemplate _owner;

            internal GridRange Range { get; private set; }

            internal void Add(GridRange gridRange, T gridItem)
            {
                Debug.Assert(gridItem != null && gridItem.Owner == null);
                gridItem.Seal(_owner, gridRange);
                Items.Add(gridItem);
                Range = Range.Union(gridRange);
            }
        }

        internal GridTemplate(DataSetPresenter owner)
        {
            Owner = owner;
            _gridRows = new GridSpecCollection<GridRow>();
            _gridColumns = new GridSpecCollection<GridColumn>();
            _scalarItems = new GridItemCollection<ScalarGridItem>(this);
            _listItems = new GridItemCollection<ListGridItem>(this);
            _childItems = new GridItemCollection<ChildGridItem>(this);
        }

        internal int ChildOrdinal { get; private set; }

        public DataSetPresenter Owner { get; private set; }

        public Model Model
        {
            get { return Owner.Model; }
        }

        private GridOrientation _orientation = GridOrientation.Y;
        public GridOrientation Orientation
        {
            get { return _orientation; }
            internal set
            {
                VerifyGridColumnWidth(value, 0, GridColumns.Count - 1, nameof(value));
                VerifyGridRowHeight(value, 0, GridRows.Count - 1, nameof(value));
                _orientation = value;
            }
        }

        private GridSpecCollection<GridRow> _gridRows;
        public ReadOnlyCollection<GridRow> GridRows
        {
            get { return _gridRows; }
        }

        private GridSpecCollection<GridColumn> _gridColumns;
        public ReadOnlyCollection<GridColumn> GridColumns
        {
            get { return _gridColumns; }
        }

        private GridItemCollection<ScalarGridItem> _scalarItems;
        public ReadOnlyCollection<ScalarGridItem> ScalarItems
        {
            get { return _scalarItems; }
        }

        private GridItemCollection<ListGridItem> _listItems;
        public ReadOnlyCollection<ListGridItem> ListItems
        {
            get { return _listItems; }
        }

        private GridItemCollection<ChildGridItem> _childItems;
        public ReadOnlyCollection<ChildGridItem> ChildItems
        {
            get { return _childItems; }
        }

        private GridRange? _repeatRange;
        public GridRange RepeatRange
        {
            get { return _repeatRange.HasValue ? _repeatRange.GetValueOrDefault() : AutoRepeatRange; }
            internal set
            {
                if (!value.Contains(AutoRepeatRange))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _repeatRange = value;
            }
        }

        private GridRange AutoRepeatRange
        {
            get { return _listItems.Range.Union(_childItems.Range); }
        }

        internal int AddGridColumn(string width)
        {
            _gridColumns.Add(new GridColumn(this, GridColumns.Count, GridLengthParser.Parse(width)));
            var result = GridColumns.Count - 1;
            VerifyGridColumnWidth(Orientation, result, result, nameof(width));
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
            _gridRows.Add(new GridRow(this, GridRows.Count, GridLengthParser.Parse(height)));
            var result = GridRows.Count - 1;
            VerifyGridRowHeight(Orientation, result, result, nameof(height));
            return result;
        }

        internal void AddGridRows(params string[] heights)
        {
            Debug.Assert(heights != null);

            foreach (var height in heights)
                AddGridRow(height);
        }

        private void VerifyGridRowHeight(GridOrientation orientation, int startIndex, int endIndex, string paramName)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!IsValidGridRowHeight(GridRows[i], orientation))
                    throw new ArgumentException(Strings.GridTemplate_InvalidGridRowHeightOrientation(i, GridRows[i].Height, orientation), paramName);
            }
        }

        private static bool IsValidGridRowHeight(GridRow gridRow, GridOrientation orentation)
        {
            var height = gridRow.Height;

            if (height.IsAbsolute)
                return true;

            if (height.IsStar)
                return orentation != GridOrientation.Y && orentation != GridOrientation.YX && orentation != GridOrientation.XY;

            Debug.Assert(height.IsAuto);
            return orentation != GridOrientation.YX;
        }

        private void VerifyGridColumnWidth(GridOrientation orientation, int startIndex, int endIndex, string paramName)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!IsValidGridColumnWidth(GridColumns[i], orientation))
                    throw new ArgumentException(Strings.GridTemplate_InvalidGridColumnWidthOrientation(i, GridColumns[i].Width, orientation), paramName);
            }
        }

        private static bool IsValidGridColumnWidth(GridColumn gridColumn, GridOrientation orentation)
        {
            var width = gridColumn.Width;

            if (width.IsAbsolute)
                return true;

            if (width.IsStar)
                return orentation != GridOrientation.X && orentation != GridOrientation.YX && orentation != GridOrientation.XY;

            Debug.Assert(width.IsAuto);
            return orentation != GridOrientation.XY;
        }

        internal void AddItem<T>(GridRange gridRange, ScalarGridItem<T> scalarItem)
            where T : UIElement, new()
        {
            VerifyAddItem(gridRange, scalarItem, nameof(scalarItem), true);
            _scalarItems.Add(gridRange, scalarItem);
        }

        internal void AddItem<T>(GridRange gridRange, ListGridItem<T> listItem)
            where T : UIElement, new()
        {
            VerifyAddItem(gridRange, listItem, nameof(listItem), true);
            _listItems.Add(gridRange, listItem);
        }

        internal void AddItem<T>(GridRange gridRange, ChildGridItem<T> childItem)
            where T : DataSetView, new()
        {
            VerifyAddItem(gridRange, childItem, nameof(childItem), false);
            var childTemplate = childItem.Template;
            childTemplate.ChildOrdinal = ChildItems.Count;
            _childItems.Add(gridRange, childItem);
        }

        private void VerifyAddItem(GridRange gridRange, GridItem gridItem, string paramGridItemName, bool isScalar)
        {
            if (!GetGridRangeAll().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
            if (!isScalar && _repeatRange.HasValue && !_repeatRange.GetValueOrDefault().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
            if (gridItem == null)
                throw new ArgumentNullException(paramGridItemName);
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

        public bool IsPinned
        {
            get { return PinnedLeft > 0 || PinnedTop > 0 || PinnedRight > 0 || PinnedBottom > 0; }
        }
    }
}
