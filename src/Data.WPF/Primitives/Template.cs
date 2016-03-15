using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    public sealed partial class Template
    {
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

        internal Template()
        {
            GridColumns = new GridTrackCollection<GridColumn>();
            GridRows = new GridTrackCollection<GridRow>();
        }

        private RepeatOrientation _repeatOrientation = RepeatOrientation.Y;
        public RepeatOrientation RepeatOrientation
        {
            get { return _repeatOrientation; }
            internal set
            {
                VerifyGridColumnWidth(value, 0, GridColumns.Count - 1, nameof(value));
                VerifyGridRowHeight(value, 0, GridRows.Count - 1, nameof(value));
                _repeatOrientation = value;
            }
        }

        public GridTrackCollection<GridColumn> GridColumns { get; private set; }

        public GridTrackCollection<GridRow> GridRows { get; private set; }

        private TemplateItemCollection<ScalarItem> _scalarItems = new TemplateItemCollection<ScalarItem>();
        public ReadOnlyCollection<ScalarItem> ScalarItems
        {
            get { return _scalarItems; }
        }

        private TemplateItemCollection<RepeatItem> _repeatItems = new TemplateItemCollection<RepeatItem>();
        public ReadOnlyCollection<RepeatItem> RepeatItems
        {
            get { return _repeatItems; }
        }

        private TemplateItemCollection<SubviewItem> _subviewItems = new TemplateItemCollection<SubviewItem>();
        public ReadOnlyCollection<SubviewItem> SubviewItems
        {
            get { return _subviewItems; }
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
            get { return _repeatItems.Range.Union(_subviewItems.Range); }
        }

        internal int AddGridColumn(string width)
        {
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GridLengthParser.Parse(width)));
            var result = GridColumns.Count - 1;
            VerifyGridColumnWidth(RepeatOrientation, result, result, nameof(width));
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
            GridRows.Add(new GridRow(this, GridRows.Count, GridLengthParser.Parse(height)));
            var result = GridRows.Count - 1;
            VerifyGridRowHeight(RepeatOrientation, result, result, nameof(height));
            return result;
        }

        internal void AddGridRows(params string[] heights)
        {
            Debug.Assert(heights != null);

            foreach (var height in heights)
                AddGridRow(height);
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

        internal int ScalarItemsCountBeforeRepeat { get; private set; }

        //internal int StarWidthColumnsCount
        //{
        //    get { return GridColumns.StarLengthCount; }
        //}

        //internal int StarHeightRowsCount
        //{
        //    get { return GridRows.StarLengthCount; }
        //}

        //internal int AutoWidthColumnsCount
        //{
        //    get { return GridColumns.AutoLengthCount; }
        //}

        //internal int AutoHeightRowsCount
        //{
        //    get { return GridRows.AutoLengthCount; }
        //}

        //internal double AbsoluteWidthTotal
        //{
        //    get { return GridColumns.AbsoluteLengthTotal; }
        //}

        //internal double AbsouteHeightTotal
        //{
        //    get { return GridRows.AbsoluteLengthTotal; }
        //}

        internal void AddScalarItem(GridRange gridRange, ScalarItem scalarItem)
        {
            VerifyAddTemplateItem(gridRange, scalarItem, nameof(scalarItem), true);
            scalarItem.Construct(this, gridRange, _scalarItems.Count);
            _scalarItems.Add(gridRange, scalarItem);
            if (_repeatItems.Count == 0)
                ScalarItemsCountBeforeRepeat = _scalarItems.Count;
        }

        internal void AddRepeatItem(GridRange gridRange, RepeatItem repeatItem)
        {
            VerifyAddTemplateItem(gridRange, repeatItem, nameof(repeatItem), true);
            repeatItem.Construct(this, gridRange, _repeatItems.Count);
            _repeatItems.Add(gridRange, repeatItem);
        }

        internal void AddSubviewItem(GridRange gridRange, SubviewItem subviewItem)
        {
            VerifyAddTemplateItem(gridRange, subviewItem, nameof(subviewItem), false);
            subviewItem.Seal(this, gridRange, _repeatItems.Count, _subviewItems.Count);
            _repeatItems.Add(gridRange, subviewItem);
            _subviewItems.Add(gridRange, subviewItem);
        }

        private void VerifyAddTemplateItem(GridRange gridRange, TemplateItem templateItem, string paramTemplateItemName, bool isScalar)
        {
            if (!GetGridRangeAll().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
            if (!isScalar && _repeatRange.HasValue && !_repeatRange.GetValueOrDefault().Contains(gridRange))
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

        public bool IsEofVisible { get; internal set; }

        public bool IsEmptySetVisible { get; internal set; }
    }
}
