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

        private sealed class TemplateUnitCollection<T> : ReadOnlyCollection<T>
            where T : TemplateUnit
        {
            internal TemplateUnitCollection()
                : base(new List<T>())
            {
            }

            internal GridRange Range { get; private set; }

            internal void Add(GridRange gridRange, T unit)
            {
                Debug.Assert(unit != null);
                Items.Add(unit);
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

        private TemplateUnitCollection<ScalarUnit> _scalarUnits = new TemplateUnitCollection<ScalarUnit>();
        public ReadOnlyCollection<ScalarUnit> ScalarEntries
        {
            get { return _scalarUnits; }
        }

        private TemplateUnitCollection<ListUnit> _listUnits = new TemplateUnitCollection<ListUnit>();
        public ReadOnlyCollection<ListUnit> ListUnits
        {
            get { return _listUnits; }
        }

        private TemplateUnitCollection<ChildUnit> _childUnits = new TemplateUnitCollection<ChildUnit>();
        public ReadOnlyCollection<ChildUnit> ChildUnits
        {
            get { return _childUnits; }
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
            get { return _listUnits.Range.Union(_childUnits.Range); }
        }

        internal int AddGridColumn(string width)
        {
            _gridColumns.Add(new GridColumn(this, GridColumns.Count, GridLengthParser.Parse(width)));
            var result = GridColumns.Count - 1;
            VerifyGridColumnWidth(ListOrientation, result, result, nameof(width));
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
            VerifyGridRowHeight(ListOrientation, result, result, nameof(height));
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

        internal int NumberOfScallarEntriesBeforeRow { get; private set; }

        internal void AddScalarUnit(GridRange gridRange, ScalarUnit scalarUnit)
        {
            VerifyAddTemplateUnit(gridRange, scalarUnit, nameof(scalarUnit), true);
            scalarUnit.Construct(this, gridRange, _scalarUnits.Count);
            _scalarUnits.Add(gridRange, scalarUnit);
            if (_listUnits.Count == 0)
                NumberOfScallarEntriesBeforeRow = _scalarUnits.Count;
        }

        internal void AddListUnit(GridRange gridRange, ListUnit listUnit)
        {
            VerifyAddTemplateUnit(gridRange, listUnit, nameof(listUnit), true);
            listUnit.Construct(this, gridRange, _listUnits.Count);
            _listUnits.Add(gridRange, listUnit);
        }

        internal void AddChildUnit(GridRange gridRange, ChildUnit childUnit)
        {
            VerifyAddTemplateUnit(gridRange, childUnit, nameof(childUnit), false);
            childUnit.Seal(this, gridRange, _listUnits.Count, _childUnits.Count);
            _listUnits.Add(gridRange, childUnit);
            _childUnits.Add(gridRange, childUnit);
        }

        private void VerifyAddTemplateUnit(GridRange gridRange, TemplateUnit templateUnit, string paramTemplateUnitName, bool isScalar)
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

        public bool IsPinned
        {
            get { return PinnedLeft > 0 || PinnedTop > 0 || PinnedRight > 0 || PinnedBottom > 0; }
        }
    }
}
