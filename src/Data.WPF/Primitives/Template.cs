using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

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

        internal Template(RowManager rowManager)
        {
            RowManager = rowManager;
            GridColumns = new GridTrackCollection<GridColumn>();
            GridRows = new GridTrackCollection<GridRow>();
            CrossRepeats = 1;
            HierarchicalModelOrdinal = -1;
            VirtualizationThreshold = 50;
        }

        internal RowManager RowManager { get; private set; }

        internal ElementManager ElementManager
        {
            get { return RowManager as ElementManager; }
        }

        internal LayoutManager LayoutManager
        {
            get { return RowManager as LayoutManager; }
        }

        public DataPresenter DataPresenter
        {
            get { return RowManager as DataPresenter; }
        }

        public Orientation? RepeatOrientation { get; private set; }

        public int CrossRepeats { get; private set; }

        internal void Repeat(Orientation orientation, int crossRepeats = 1)
        {
            Debug.Assert(crossRepeats >= 0);
            RepeatOrientation = orientation;
            CrossRepeats = crossRepeats;
            VerifyGridLengths();
        }

        private void VerifyGridLengths()
        {
            if (GridColumns.Count > 0)
                VerifyGridColumnWidth(0, GridColumns.Count - 1);
            if (GridRows.Count > 0)
                VerifyGridRowHeight(0, GridRows.Count - 1);
        }

        public GridTrackCollection<GridColumn> GridColumns { get; private set; }

        public GridTrackCollection<GridRow> GridRows { get; private set; }

        private TemplateItemCollection<DataItem> _dataItems = new TemplateItemCollection<DataItem>();
        public ReadOnlyCollection<DataItem> DataItems
        {
            get { return _dataItems; }
        }

        private TemplateItemCollection<RowItem> _rowItems = new TemplateItemCollection<RowItem>();
        public ReadOnlyCollection<RowItem> RowItems
        {
            get { return _rowItems; }
        }

        private TemplateItemCollection<SubviewItem> _subviewItems = new TemplateItemCollection<SubviewItem>();
        public ReadOnlyCollection<SubviewItem> SubviewItems
        {
            get { return _subviewItems; }
        }

        private GridRange? _rowRange;
        public GridRange RowRange
        {
            get { return _rowRange.HasValue ? _rowRange.GetValueOrDefault() : AutoRowRange; }
            internal set
            {
                _rowRange = value;
                VerifyRowRange();
            }
        }

        private void VerifyRowRange()
        {
            var rowRange = RowRange;
            var autoRowRange = AutoRowRange;
            if ((!autoRowRange.IsEmpty && !rowRange.Contains(autoRowRange)) || DataItems.Any(x => rowRange.IntersectsWith(x.GridRange)))
                throw new InvalidOperationException(Strings.Template_InvalidRowRange);
        }

        private GridRange AutoRowRange
        {
            get { return _rowItems.Range; }
        }

        internal int AddGridColumn(string width)
        {
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GridLengthParser.Parse(width)));
            var result = GridColumns.Count - 1;
            VerifyGridColumnWidth(result, result);
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
            VerifyGridRowHeight(result, result);
            return result;
        }

        internal void AddGridRows(params string[] heights)
        {
            Debug.Assert(heights != null);

            foreach (var height in heights)
                AddGridRow(height);
        }

        private void VerifyGridRowHeight(int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
                VerifyGridRowHeight(GridRows[i]);
        }

        private void VerifyGridRowHeight(GridRow gridRow)
        {
            var height = gridRow.Height;

            if (height.IsAbsolute)
                return;

            if (height.IsStar)
            {
                if (Repeatable(System.Windows.Controls.Orientation.Vertical))
                    throw new InvalidOperationException(Strings.Template_InvalidStarHeightGridRow(gridRow.Ordinal));
            }
            else
            {
                Debug.Assert(height.IsAuto);
                if (CrossRepeatable(System.Windows.Controls.Orientation.Vertical))
                    throw new InvalidOperationException(Strings.Template_InvalidAutoHeightGridRow(gridRow.Ordinal));
            }
        }

        private void VerifyGridColumnWidth(int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
                VerifyGridColumnWidth(GridColumns[i]);
        }

        private void VerifyGridColumnWidth(GridColumn gridColumn)
        {
            var width = gridColumn.Width;

            if (width.IsAbsolute)
                return;

            if (width.IsStar)
            {
                if (Repeatable(System.Windows.Controls.Orientation.Horizontal))
                    throw new InvalidOperationException(Strings.Template_InvalidStarWidthGridColumn(gridColumn.Ordinal));
            }
            else
            {
                Debug.Assert(width.IsAuto);
                if (CrossRepeatable(System.Windows.Controls.Orientation.Horizontal))
                    throw new InvalidOperationException(Strings.Template_InvalidAutoWidthGridColumn(gridColumn.Ordinal));
            }
        }

        private bool Repeatable(Orientation orientation)
        {
            return !RepeatOrientation.HasValue ? false : RepeatOrientation.GetValueOrDefault() == orientation || CrossRepeats != 1;
        }

        internal bool CrossRepeatable(Orientation orientation)
        {
            return !RepeatOrientation.HasValue ? false : RepeatOrientation.GetValueOrDefault() != orientation && CrossRepeats != 1;
        }

        internal int DataItemsCountBeforeRepeat { get; private set; }

        internal void AddDataItem(GridRange gridRange, DataItem dataItem)
        {
            VerifyAddTemplateItem(gridRange, dataItem, nameof(dataItem), true);
            dataItem.Construct(this, gridRange, _dataItems.Count);
            _dataItems.Add(gridRange, dataItem);
            if (_rowItems.Count == 0)
                DataItemsCountBeforeRepeat = _dataItems.Count;
            VerifyRowRange();
        }

        internal void AddRowItem(GridRange gridRange, RowItem rowItem)
        {
            VerifyAddTemplateItem(gridRange, rowItem, nameof(rowItem), true);
            rowItem.Construct(this, gridRange, _rowItems.Count);
            _rowItems.Add(gridRange, rowItem);
            VerifyRowRange();
        }

        internal void AddSubviewItem(GridRange gridRange, SubviewItem subviewItem)
        {
            VerifyAddTemplateItem(gridRange, subviewItem, nameof(subviewItem), false);
            subviewItem.Seal(this, gridRange, _rowItems.Count, _subviewItems.Count);
            _rowItems.Add(gridRange, subviewItem);
            _subviewItems.Add(gridRange, subviewItem);
            VerifyRowRange();
        }

        private void VerifyAddTemplateItem(GridRange gridRange, TemplateItem templateItem, string paramTemplateItemName, bool isScalar)
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

        public EofRowMapping EofRowMapping { get; internal set; }

        public int HierarchicalModelOrdinal { get; internal set; }

        private Func<RowView> _rowViewConstructor;
        public Func<RowView> RowViewConstructor
        {
            get { return _rowViewConstructor ?? (() => new RowView()); }
            internal set { _rowViewConstructor = value; }
        }

        public Action<RowView> RowViewInitializer { get; internal set; }

        public Action<RowView> RowViewCleanupAction { get; internal set; }

        public int VirtualizationThreshold { get; internal set; }
    }
}
