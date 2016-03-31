using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            StackDimensions = 1;
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

        public Orientation? StackOrientation { get; private set; }

        public int StackDimensions { get; private set; }

        internal void Stack(Orientation orientation, int stackDimensions = 1)
        {
            Debug.Assert(stackDimensions >= 0);
            StackOrientation = orientation;
            StackDimensions = stackDimensions;
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

        private TemplateItemCollection<StackItem> _stackItems = new TemplateItemCollection<StackItem>();
        public ReadOnlyCollection<StackItem> StackItems
        {
            get { return _stackItems; }
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
            get { return _rowRange.HasValue ? _rowRange.GetValueOrDefault() : _rowItems.Range; }
            internal set { _rowRange = value; }
        }

        internal void VerifyTemplateItemGridRange()
        {
            for (int i = 0; i < RowItems.Count; i++)
                RowItems[i].VerifyGridRange();

            for (int i = 0; i < DataItems.Count; i++)
                DataItems[i].VerifyGridRange();

            for (int i = 0; i < StackItems.Count; i++)
                StackItems[i].VerifyGridRange();
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
                if (IsRepeatable(Orientation.Vertical))
                    throw new InvalidOperationException(Strings.Template_InvalidStarHeightGridRow(gridRow.Ordinal));
            }
            else
            {
                Debug.Assert(height.IsAuto);
                if (IsMultidimensional(Orientation.Vertical))
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
                if (IsRepeatable(Orientation.Horizontal))
                    throw new InvalidOperationException(Strings.Template_InvalidStarWidthGridColumn(gridColumn.Ordinal));
            }
            else
            {
                Debug.Assert(width.IsAuto);
                if (IsMultidimensional(Orientation.Horizontal))
                    throw new InvalidOperationException(Strings.Template_InvalidAutoWidthGridColumn(gridColumn.Ordinal));
            }
        }

        private bool IsRepeatable(Orientation orientation)
        {
            return !StackOrientation.HasValue ? false : StackOrientation.GetValueOrDefault() == orientation || StackDimensions != 1;
        }

        internal bool IsMultidimensional(Orientation orientation)
        {
            return !StackOrientation.HasValue ? false : StackOrientation.GetValueOrDefault() != orientation && StackDimensions != 1;
        }

        internal int DataItemsSplit { get; private set; }

        internal int StackItemsSplit { get; private set; }

        internal void AddDataItem(GridRange gridRange, DataItem dataItem)
        {
            Debug.Assert(IsValid(gridRange));
            dataItem.Construct(this, gridRange, _dataItems.Count);
            _dataItems.Add(gridRange, dataItem);
            if (_rowItems.Count == 0)
                DataItemsSplit = _dataItems.Count;
        }

        internal void AddStackItem(GridRange gridRange, StackItem stackItem)
        {
            Debug.Assert(IsValid(gridRange));
            stackItem.Construct(this, gridRange, _stackItems.Count);
            _stackItems.Add(gridRange, stackItem);
            if (_rowItems.Count == 0)
                StackItemsSplit = _stackItems.Count;
        }

        internal void AddRowItem(GridRange gridRange, RowItem rowItem)
        {
            Debug.Assert(IsValid(gridRange));
            rowItem.Construct(this, gridRange, _rowItems.Count);
            _rowItems.Add(gridRange, rowItem);
        }

        internal void AddSubviewItem(GridRange gridRange, SubviewItem subviewItem)
        {
            Debug.Assert(IsValid(gridRange));
            subviewItem.Seal(this, gridRange, _rowItems.Count, _subviewItems.Count);
            _rowItems.Add(gridRange, subviewItem);
            _subviewItems.Add(gridRange, subviewItem);
        }

        internal bool IsValid(GridRange gridRange)
        {
            return !gridRange.IsEmpty && Range().Contains(gridRange);
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
