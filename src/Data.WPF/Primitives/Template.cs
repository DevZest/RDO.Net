using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed partial class Template
    {
        internal Template()
        {
            InternalGridColumns = new GridColumnCollection(this);
            InternalGridRows = new GridRowCollection(this);
            BlockDimensions = 1;
            HierarchicalModelOrdinal = -1;
        }

        internal RowManager RowManager { get; set; }

        internal ElementManager ElementManager
        {
            get { return RowManager as ElementManager; }
        }

        internal LayoutManager LayoutManager
        {
            get { return RowManager as LayoutManager; }
        }

        internal LayoutXYManager LayoutXYManager
        {
            get { return RowManager as LayoutXYManager; }
        }

        public DataPresenter DataPresenter
        {
            get { return LayoutManager == null ? null : LayoutManager.DataPresenter; }
        }

        public Orientation? Orientation { get; private set; }

        public int BlockDimensions { get; private set; }

        internal void Layout(Orientation orientation, int blockDimensions = 1)
        {
            Debug.Assert(blockDimensions >= 0);
            Orientation = orientation;
            BlockDimensions = blockDimensions;
        }

        internal GridColumnCollection InternalGridColumns { get; private set; }

        public ReadOnlyCollection<GridColumn> GridColumns
        {
            get { return InternalGridColumns; }
        }

        internal GridRowCollection InternalGridRows { get; private set; }

        public ReadOnlyCollection<GridRow> GridRows
        {
            get { return InternalGridRows; }
        }

        internal readonly RecapItemCollection<ScalarItem> InternalScalarItems = new RecapItemCollection<ScalarItem>();

        public IReadOnlyList<ScalarItem> ScalarItems
        {
            get { return InternalScalarItems; }
        }

        internal readonly RecapItemCollection<BlockItem> InternalBlockItems = new RecapItemCollection<BlockItem>();
        public IReadOnlyList<BlockItem> BlockItems
        {
            get { return InternalBlockItems; }
        }

        private Func<RowPresenter, int> _rowItemGroupSelector;
        public Func<RowPresenter, int> RowItemGroupSelector
        {
            get { return _rowItemGroupSelector ?? (rowPresenter => 0); }
            internal set { _rowItemGroupSelector = value; }
        }

        private IConcatList<RowItemCollection> _rowItemGroups = new RowItemCollection();
        internal IReadOnlyList<RowItemCollection> InternalRowItemGroups
        {
            get { return _rowItemGroups; }
        }
        public IReadOnlyList<IReadOnlyList<RowItem>> RowItemGroups
        {
            get { return _rowItemGroups; }
        }

        internal void NextRowItemGroup()
        {
            _rowItemGroups = _rowItemGroups.Concat(new RowItemCollection());
        }

        private IConcatList<SubviewItem> _subviewItems = ConcatList<SubviewItem>.Empty;
        public IReadOnlyList<SubviewItem> SubviewItems
        {
            get { return _subviewItems; }
        }

        private GridRange? _rowRange;
        public GridRange RowRange
        {
            get
            {
                if (!_rowRange.HasValue)
                    _rowRange = CalcRowRange();
                return _rowRange.GetValueOrDefault();
            }
            internal set { _rowRange = value; }
        }

        private GridRange CalcRowRange()
        {
            var result = new GridRange();
            foreach (var rowItems in InternalRowItemGroups)
                result = result.Union(rowItems.Range);
            return result;
        }

        public GridRange BlockRange
        {
            get { return RowRange.Union(InternalBlockItems.Range); }
        }

        internal void VerifyLayout()
        {
            VerifyGridUnitType();
            VerifyRowRange();
            VerifyFrozenMargins();
        }

        internal void VerifyGridUnitType()
        {
            GridColumns.ForEach(x => x.VerifyUnitType());
            GridRows.ForEach(x => x.VerifyUnitType());
        }

        private void VerifyRowRange()
        {
            if (RowRange.IsEmpty)
                throw new InvalidOperationException(Strings.Template_EmptyRowRange);

            for (int i = 0; i < RowItemGroups.Count; i++)
            {
                var rowItems = RowItemGroups[i];
                for (int j = 0; j < rowItems.Count; j++)
                    rowItems[i].VerifyRowRange();
            }

            for (int i = 0; i < ScalarItems.Count; i++)
                ScalarItems[i].VerifyRowRange();

            for (int i = 0; i < BlockItems.Count; i++)
                BlockItems[i].VerifyRowRange();
        }

        private void VerifyFrozenMargins()
        {
            if (!Orientation.HasValue)
                return;

            var orientation = Orientation.GetValueOrDefault();
            if (orientation == System.Windows.Controls.Orientation.Horizontal)
                InternalGridColumns.VerifyFrozenMargins();
            else
                InternalGridRows.VerifyFrozenMargins();

            ScalarItems.ForEach(x => x.VerifyFrozenMargins(nameof(ScalarItems)));
            BlockItems.ForEach(x => x.VerifyFrozenMargins(nameof(BlockItems)));
            for (int i = 0; i < RowItemGroups.Count; i++)
            {
                var rowItems = RowItemGroups[i];
                rowItems.ForEach(x => x.VerifyFrozenMargins(string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", nameof(RowItemGroups), i)));
            }
        }

        internal int AddGridColumn(string width)
        {
            var gridColumn = new GridColumn(InternalGridColumns, GridColumns.Count, GridLengthParser.Parse(width));
            InternalGridColumns.Add(gridColumn);
            return gridColumn.Ordinal;
        }

        internal void AddGridColumns(params string[] widths)
        {
            Debug.Assert(widths != null);

            foreach (var width in widths)
                AddGridColumn(width);
        }

        internal int AddGridRow(string height)
        {
            var gridRow = new GridRow(InternalGridRows, GridRows.Count, GridLengthParser.Parse(height));
            InternalGridRows.Add(gridRow);
            return gridRow.Ordinal;
        }

        internal void AddGridRows(params string[] heights)
        {
            Debug.Assert(heights != null);

            foreach (var height in heights)
                AddGridRow(height);
        }

        internal bool IsMultidimensional(Orientation orientation)
        {
            return !Orientation.HasValue ? false : Orientation.GetValueOrDefault() != orientation && BlockDimensions != 1;
        }

        internal int ScalarItemsSplit { get; private set; }

        internal int BlockItemsSplit { get; private set; }

        private bool HasRowItem
        {
            get
            {
                foreach (var rowItems in RowItemGroups)
                {
                    if (rowItems.Count > 0)
                        return true;
                }
                return false;
            }
        }

        internal void AddScalarItem(GridRange gridRange, ScalarItem scalarItem)
        {
            Debug.Assert(IsValid(gridRange));
            scalarItem.Construct(this, gridRange, InternalScalarItems.Count);
            InternalScalarItems.Add(gridRange, scalarItem);
            if (!HasRowItem)
                ScalarItemsSplit = InternalScalarItems.Count;
        }

        internal void AddBlockItem(GridRange gridRange, BlockItem blockItem)
        {
            Debug.Assert(IsValid(gridRange));
            blockItem.Construct(this, gridRange, InternalBlockItems.Count);
            InternalBlockItems.Add(gridRange, blockItem);
            if (!HasRowItem)
                BlockItemsSplit = InternalBlockItems.Count;
        }

        private RowItemCollection CurrentRowItems
        {
            get { return InternalRowItemGroups[InternalRowItemGroups.Count - 1]; }
        }

        internal void AddRowItem(GridRange gridRange, RowItem rowItem)
        {
            Debug.Assert(IsValid(gridRange));
            var currentRowItems = CurrentRowItems;
            rowItem.Construct(this, gridRange, CurrentRowItems.Count);
            CurrentRowItems.Add(gridRange, rowItem);
        }

        internal void AddSubviewItem(GridRange gridRange, SubviewItem subviewItem)
        {
            Debug.Assert(IsValid(gridRange));
            var currentRowItems = CurrentRowItems;
            subviewItem.Seal(this, gridRange, currentRowItems.Count, SubviewItems.Count);
            currentRowItems.Add(gridRange, subviewItem);
            _subviewItems = _subviewItems.Concat(subviewItem);
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

        private EofVisibility _eofVisibility = EofVisibility.Never;
        public EofVisibility EofVisibility
        {
            get { return IsHierarchical ? EofVisibility.Never : _eofVisibility; }
            internal set { _eofVisibility = value; }
        }

        public int HierarchicalModelOrdinal { get; internal set; }

        public bool IsHierarchical
        {
            get { return HierarchicalModelOrdinal >= 0; }
        }

        private Func<BlockView> _blockViewConstructor;
        public Func<BlockView> BlockViewConstructor
        {
            get { return _blockViewConstructor ?? (() => new BlockView()); }
            internal set { _blockViewConstructor = value; }
        }

        public Action<BlockView> BlockViewInitializer { get; internal set; }

        internal void InitializeBlockView(BlockView blockView)
        {
            if (BlockViewInitializer != null)
                BlockViewInitializer(blockView);
        }

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
            InitMeasuredLengths();
            DistributeStarLengths();
        }

        private void InitMeasuredLengths()
        {
            InternalGridColumns.InitMeasuredLengths();
            InternalGridRows.InitMeasuredLengths();
        }

        private void DistributeStarLengths()
        {
            DistributeStarWidths();
            DistributeStarHeights();
        }

        internal void DistributeStarWidths()
        {
            InternalGridColumns.DistributeStarLengths();
        }

        internal void DistributeStarHeights()
        {
            InternalGridRows.DistributeStarLengths();
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

                InternalGridColumns.InvalidateStarLengthTracks();
                InternalScalarItems.InvalidateAutoWidthItems();
                InternalBlockItems.InvalidateAutoWidthItems();
                foreach (var rowItems in InternalRowItemGroups)
                    rowItems.InvalidateAutoWidthItems();
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

                InternalGridRows.InvalidateStarLengthTracks();
                InternalScalarItems.InvalidateAutoHeightItems();
                InternalBlockItems.InvalidateAutoHeightItems();
                foreach (var rowItems in InternalRowItemGroups)
                    rowItems.InvalidateAutoHeightItems();
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

        internal int CoerceBlockDimensions()
        {
            if (!Orientation.HasValue)
                return 1;

            return Orientation.GetValueOrDefault() == System.Windows.Controls.Orientation.Horizontal
                ? CoerceBlockDimensions(SizeToContentX, AvailableWidth, InternalGridColumns)
                : CoerceBlockDimensions(SizeToContentY, AvailableHeight, InternalGridRows);
        }

        private int CoerceBlockDimensions<T>(bool sizeToContent, double availableLength, GridTrackCollection<T> gridTracks)
            where T : GridTrack, IConcatList<T>
        {
            if (sizeToContent)
                return 1;

            return BlockDimensions > 0 ? BlockDimensions : (int)(availableLength / gridTracks.TotalAbsoluteLength);
        }

        public int FrozenLeft { get; internal set; }

        public int FrozenTop { get; internal set; }

        public int FrozenRight { get; internal set; }

        public int FrozenBottom { get; internal set; }

        public int Stretches { get; internal set; }
    }
}
