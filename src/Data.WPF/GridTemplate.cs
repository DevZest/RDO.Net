using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

        private sealed class GridEntryCollection<T> : ReadOnlyCollection<T>
            where T : GridEntry
        {
            internal GridEntryCollection()
                : base(new List<T>())
            {
            }

            internal GridRange Range { get; private set; }

            internal void Add(GridRange gridRange, T gridEntry)
            {
                Debug.Assert(gridEntry != null);
                Items.Add(gridEntry);
                Range = Range.Union(gridRange);
            }
        }

        internal GridTemplate(DataSetPresenter owner)
        {
            Owner = owner;
        }

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

        private GridSpecCollection<GridColumn> _gridColumns = new GridSpecCollection<GridColumn>();
        public ReadOnlyCollection<GridColumn> GridColumns
        {
            get { return _gridColumns; }
        }

        private GridSpecCollection<GridRow> _gridRows = new GridSpecCollection<GridRow>();
        public ReadOnlyCollection<GridRow> GridRows
        {
            get { return _gridRows; }
        }

        private GridEntryCollection<ScalarEntry> _scalarEntries = new GridEntryCollection<ScalarEntry>();
        public ReadOnlyCollection<ScalarEntry> ScalarEntries
        {
            get { return _scalarEntries; }
        }

        private GridEntryCollection<RowEntry> _rowEntries = new GridEntryCollection<RowEntry>();
        public ReadOnlyCollection<RowEntry> RowEntries
        {
            get { return _rowEntries; }
        }

        private GridEntryCollection<ChildEntry> _childEntries = new GridEntryCollection<ChildEntry>();
        public ReadOnlyCollection<ChildEntry> ChildEntries
        {
            get { return _childEntries; }
        }

        private GridRange? _rowRange;
        public GridRange RowRange
        {
            get { return _rowRange.HasValue ? _rowRange.GetValueOrDefault() : AutoRowRange; }
            internal set
            {
                if (!value.Contains(AutoRowRange))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _rowRange = value;
            }
        }

        private GridRange AutoRowRange
        {
            get { return _rowEntries.Range.Union(_childEntries.Range); }
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

        internal int NumberOfScallarEntriesBeforeRow { get; private set; }

        internal void AddScalarEntry(GridRange gridRange, ScalarEntry scalarEntry)
        {
            VerifyAddEntry(gridRange, scalarEntry, nameof(scalarEntry), true);
            scalarEntry.Seal(this, gridRange, _scalarEntries.Count);
            _scalarEntries.Add(gridRange, scalarEntry);
            if (_rowEntries.Count == 0)
                NumberOfScallarEntriesBeforeRow = _scalarEntries.Count;
        }

        internal void AddRowEntry(GridRange gridRange, RowEntry rowEntry)
        {
            VerifyAddEntry(gridRange, rowEntry, nameof(rowEntry), true);
            rowEntry.Seal(this, gridRange, _rowEntries.Count);
            _rowEntries.Add(gridRange, rowEntry);
        }

        internal void AddChildEntry(GridRange gridRange, ChildEntry childEntry)
        {
            VerifyAddEntry(gridRange, childEntry, nameof(childEntry), false);
            childEntry.Seal(this, gridRange, _rowEntries.Count, _childEntries.Count);
            _rowEntries.Add(gridRange, childEntry);
            _childEntries.Add(gridRange, childEntry);
        }

        private void VerifyAddEntry(GridRange gridRange, GridEntry gridEntry, string paramGridEntryName, bool isScalar)
        {
            if (!GetGridRangeAll().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
            if (!isScalar && _rowRange.HasValue && !_rowRange.GetValueOrDefault().Contains(gridRange))
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

        public bool IsPinned
        {
            get { return PinnedLeft > 0 || PinnedTop > 0 || PinnedRight > 0 || PinnedBottom > 0; }
        }
    }
}
