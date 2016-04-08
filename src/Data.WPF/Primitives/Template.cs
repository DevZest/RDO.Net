using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
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
            BlockDimensions = 1;
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

        public Orientation? Orientation { get; private set; }

        public int BlockDimensions { get; private set; }

        internal void Block(Orientation orientation, int blockDimensions = 1)
        {
            Debug.Assert(blockDimensions >= 0);
            Orientation = orientation;
            BlockDimensions = blockDimensions;
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

        private TemplateItemCollection<BlockItem> _blockItems = new TemplateItemCollection<BlockItem>();
        public ReadOnlyCollection<BlockItem> BlockItems
        {
            get { return _blockItems; }
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

            for (int i = 0; i < BlockItems.Count; i++)
                BlockItems[i].VerifyGridRange();
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
                if (IsRepeatable(System.Windows.Controls.Orientation.Vertical))
                    throw new InvalidOperationException(Strings.Template_InvalidStarHeightGridRow(gridRow.Ordinal));
            }
            else
            {
                Debug.Assert(height.IsAuto);
                if (IsMultidimensional(System.Windows.Controls.Orientation.Vertical))
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
                if (IsRepeatable(System.Windows.Controls.Orientation.Horizontal))
                    throw new InvalidOperationException(Strings.Template_InvalidStarWidthGridColumn(gridColumn.Ordinal));
            }
            else
            {
                Debug.Assert(width.IsAuto);
                if (IsMultidimensional(System.Windows.Controls.Orientation.Horizontal))
                    throw new InvalidOperationException(Strings.Template_InvalidAutoWidthGridColumn(gridColumn.Ordinal));
            }
        }

        private bool IsRepeatable(Orientation orientation)
        {
            return !Orientation.HasValue ? false : Orientation.GetValueOrDefault() == orientation || BlockDimensions != 1;
        }

        internal bool IsMultidimensional(Orientation orientation)
        {
            return !Orientation.HasValue ? false : Orientation.GetValueOrDefault() != orientation && BlockDimensions != 1;
        }

        internal int DataItemsSplit { get; private set; }

        internal int BlockItemsSplit { get; private set; }

        internal void AddDataItem(GridRange gridRange, DataItem dataItem)
        {
            Debug.Assert(IsValid(gridRange));
            dataItem.Construct(this, gridRange, _dataItems.Count);
            _dataItems.Add(gridRange, dataItem);
            if (_rowItems.Count == 0)
                DataItemsSplit = _dataItems.Count;
        }

        internal void AddBlockItem(GridRange gridRange, BlockItem blockItem)
        {
            Debug.Assert(IsValid(gridRange));
            blockItem.Construct(this, gridRange, _blockItems.Count);
            _blockItems.Add(gridRange, blockItem);
            if (_rowItems.Count == 0)
                BlockItemsSplit = _blockItems.Count;
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

        private Func<BlockView> _blockViewConstructor;
        public Func<BlockView> BlockViewConstructor
        {
            get { return _blockViewConstructor ?? (() => new BlockView()); }
            internal set { _blockViewConstructor = value; }
        }

        public Action<BlockView> BlockViewInitializer { get; internal set; }

        public Action<BlockView> BlockViewCleanupAction { get; internal set; }

        private Func<RowView> _rowViewConstructor;
        public Func<RowView> RowViewConstructor
        {
            get { return _rowViewConstructor ?? (() => new RowView()); }
            internal set { _rowViewConstructor = value; }
        }

        public Action<RowView> RowViewInitializer { get; internal set; }

        public Action<RowView> RowViewCleanupAction { get; internal set; }

        internal void InitMeasure(Size availableSize)
        {
            AvailableSize = availableSize;
            InitMeasuredAutoLengths();
            DistributeStarLengths();
        }

        private void InitMeasuredAutoLengths()
        {
            GridColumns.InitMeasuredAutoLengths(SizeToContentX);
            GridRows.InitMeasuredAutoLengths(SizeToContentY);
        }

        internal void DistributeStarLengths()
        {
            DistributeStarLengths(AvailableWidth, GridColumns, StarWidthGridColumns);
            DistributeStarLengths(AvailableHeight, GridRows, StarHeightGridRows);
        }

        private static void DistributeStarLengths<T>(double totalLength, GridTrackCollection<T> gridTracks, IReadOnlyList<T> starGridTracks)
            where T : GridTrack
        {
            if (starGridTracks.Count == 0)
                return;

            totalLength = Math.Max(0d, totalLength - gridTracks.TotalAbsoluteLength - gridTracks.TotalAutoLength);
            var totalStarFactor = gridTracks.TotalStarFactor;
            foreach (var gridTrack in starGridTracks)
                gridTrack.MeasuredLength = totalLength * (gridTrack.Length.Value / totalStarFactor);
        }

        internal Size AvailableSize
        {
            get { return new Size(_availableWidth, _availableHeight); }
            private set
            {
                AvailableWidth = value.Width;
                AvailableHeight = value.Height;
            }
        }

        private double _availableWidth;
        internal double AvailableWidth
        {
            get { return _availableWidth; }
            private set
            {
                bool oldSizeToContentX = double.IsPositiveInfinity(_availableWidth);
                bool newSizeToContentX = double.IsPositiveInfinity(value);

                _availableWidth = value;

                if (oldSizeToContentX == newSizeToContentX)
                    return;

                _starWidthGridColumns = null;
                DataItems.ForEach(x => x.InvalidateAutoWidthGridColumns());
                BlockItems.ForEach(x => x.InvalidateAutoWidthGridColumns());
                RowItems.ForEach(x => x.InvalidateAutoWidthGridColumns());
            }
        }

        private double _availableHeight;
        internal double AvailableHeight
        {
            get { return _availableHeight; }
            private set
            {
                bool oldSizeToContentY = double.IsPositiveInfinity(_availableHeight);
                bool newSizeToContentY = double.IsPositiveInfinity(value);

                _availableHeight = value;

                if (oldSizeToContentY == newSizeToContentY)
                    return;

                _starHeightGridRows = null;
                DataItems.ForEach(x => x.InvalidateAutoHeightGridRows());
                BlockItems.ForEach(x => x.InvalidateAutoHeightGridRows());
                RowItems.ForEach(x => x.InvalidateAutoHeightGridRows());
            }
        }

        internal bool SizeToContentX
        {
            get { return double.IsPositiveInfinity(_availableWidth); }
        }

        internal bool SizeToContentY
        {
            get { return double.IsPositiveInfinity(_availableHeight); }
        }

        private IConcatList<GridColumn> _starWidthGridColumns;
        private IConcatList<GridColumn> StarWidthGridColumns
        {
            get
            {
                if (_starWidthGridColumns == null)
                    _starWidthGridColumns = GridColumns.Filter(x => x.IsStarLength(SizeToContentX));
                return _starWidthGridColumns;
            }
        }

        private IConcatList<GridRow> _starHeightGridRows;
        private IConcatList<GridRow> StarHeightGridRows
        {
            get
            {
                if (_starHeightGridRows == null)
                    _starHeightGridRows = GridRows.Filter(x => x.IsStarLength(SizeToContentY));
                return _starHeightGridRows;
            }
        }

        public int VirtualizationThreshold { get; internal set; }
    }
}
