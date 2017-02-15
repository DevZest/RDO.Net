using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        }

        internal bool IsSealed
        {
            get { return RowManager != null; }
        }

        internal RowManager RowManager { get; set; }

        internal ElementManager ElementManager
        {
            get { return RowManager as ElementManager; }
        }

        internal InputManager InputManager
        {
            get { return RowManager as InputManager; }
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

        public int BlockDimensions { get; private set; } = 1;

        internal ContainerKind ContainerKind
        {
            get { return BlockBindings.Count > 0 || (Orientation.HasValue && BlockDimensions != 1) ? ContainerKind.Block : ContainerKind.Row; }
        }

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

        internal readonly RecapBindingCollection<ScalarBinding> InternalScalarBindings = new RecapBindingCollection<ScalarBinding>();

        public IReadOnlyList<ScalarBinding> ScalarBindings
        {
            get { return InternalScalarBindings; }
        }

        internal readonly RecapBindingCollection<BlockBinding> InternalBlockBindings = new RecapBindingCollection<BlockBinding>();
        public IReadOnlyList<BlockBinding> BlockBindings
        {
            get { return InternalBlockBindings; }
        }

        internal RowBindingCollection InternalRowBindings = new RowBindingCollection();
        public IReadOnlyList<RowBinding> RowBindings
        {
            get { return InternalRowBindings; }
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
            foreach (var rowBinding in InternalRowBindings)
                result = result.Union(rowBinding.GridRange);
            return result;
        }

        public GridRange BlockRange
        {
            get { return RowRange.Union(InternalBlockBindings.Range); }
        }

        internal void Seal()
        {
            VerifyGridUnitType();
            VerifyRowRange();
            VerifyFrozenMargins();
            InternalAsyncValidators.Seal();
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

            for (int i = 0; i < RowBindings.Count; i++)
            {
                var rowBinding = RowBindings[i];
                    rowBinding.VerifyRowRange();
            }

            for (int i = 0; i < ScalarBindings.Count; i++)
                ScalarBindings[i].VerifyRowRange();

            for (int i = 0; i < BlockBindings.Count; i++)
                BlockBindings[i].VerifyRowRange();
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

            ScalarBindings.ForEach(x => x.VerifyFrozenMargins(nameof(ScalarBindings)));
            BlockBindings.ForEach(x => x.VerifyFrozenMargins(nameof(BlockBindings)));
            for (int i = 0; i < RowBindings.Count; i++)
            {
                var rowBindings = RowBindings[i];
                rowBindings.ForEach(x => x.VerifyFrozenMargins(string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", nameof(RowBindings), i)));
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

        internal int ScalarBindingsSplit { get; private set; }

        internal int BlockBindingsSplit { get; private set; }

        private bool HasRowBinding
        {
            get { return RowBindings.Count > 0; }
        }

        internal void AddBinding(GridRange gridRange, ScalarBinding scalarBinding)
        {
            Debug.Assert(IsValid(gridRange));
            scalarBinding.Seal(this, gridRange, InternalScalarBindings.Count);
            InternalScalarBindings.Add(gridRange, scalarBinding);
            if (!HasRowBinding)
                ScalarBindingsSplit = InternalScalarBindings.Count;
        }

        internal void AddBinding(GridRange gridRange, BlockBinding blockBinding)
        {
            Debug.Assert(IsValid(gridRange));
            blockBinding.Seal(this, gridRange, InternalBlockBindings.Count);
            InternalBlockBindings.Add(gridRange, blockBinding);
            if (!HasRowBinding)
                BlockBindingsSplit = InternalBlockBindings.Count;
        }

        internal void AddBinding(GridRange gridRange, RowBinding rowBinding)
        {
            Debug.Assert(IsValid(gridRange));
            rowBinding.Seal(this, gridRange, InternalRowBindings.Count);
            InternalRowBindings.Add(gridRange, rowBinding);
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

        [DefaultValue(VirtualRowPlacement.Explicit)]
        public VirtualRowPlacement VirtualRowPlacement { get; internal set; } = VirtualRowPlacement.Explicit;

        public int RecursiveModelOrdinal { get; internal set; } = -1;

        public bool IsRecursive
        {
            get { return RecursiveModelOrdinal >= 0; }
        }

        private Func<BlockView> _blockViewConstructor;
        private Style _blockViewStyle;

        internal void BlockView<T>(Style style)
            where T : BlockView, new()
        {
            _blockViewConstructor = () => new T();
            _blockViewStyle = style;
        }

        internal BlockView CreateBlockView()
        {
            var result = _blockViewConstructor == null ? new BlockView() : _blockViewConstructor();
            if (_blockViewStyle != null)
                result.Style = _blockViewStyle;
            return result;
        }

        private Func<RowView> _rowViewConstructor;
        private Style _rowViewStyle;

        internal void RowView<T>(Style style)
            where T : RowView, new()
        {
            _rowViewConstructor = () => new T();
            _rowViewStyle = style;
        }

        internal RowView CreateRowView()
        {
            var result = _rowViewConstructor == null ? new RowView() : _rowViewConstructor();
            if (_rowViewStyle != null)
                result.Style = _rowViewStyle;
            result.SetupCommandBindings();
            result.SetupInputBindings();
            return result;
        }

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
                InternalScalarBindings.InvalidateAutoWidthBindings();
                InternalBlockBindings.InvalidateAutoWidthBindings();
                InternalRowBindings.InvalidateAutoHeightBindings();
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
                InternalScalarBindings.InvalidateAutoHeightBindings();
                InternalBlockBindings.InvalidateAutoHeightBindings();
                InternalRowBindings.InvalidateAutoHeightBindings();
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

        [DefaultValue(0)]
        public int FrozenLeft { get; internal set; }

        [DefaultValue(0)]
        public int FrozenTop { get; internal set; }

        [DefaultValue(0)]
        public int FrozenRight { get; internal set; }

        [DefaultValue(0)]
        public int FrozenBottom { get; internal set; }

        [DefaultValue(0)]
        public int Stretches { get; internal set; }

        private readonly List<GridLine> _gridLines = new List<GridLine>();
        public IReadOnlyList<GridLine> GridLines
        {
            get { return _gridLines; }
        }

        internal void AddGridLine(GridLine gridLine)
        {
            Debug.Assert(gridLine != null);
            _gridLines.Add(gridLine);
        }

        [DefaultValue(true)]
        public bool TransactionalEdit { get; internal set; } = true;

        [DefaultValue(ValidationScope.CurrentRow)]
        public ValidationScope ValidationScope { get; internal set; } = ValidationScope.CurrentRow;

        [DefaultValue(ValidationMode.Progressive)]
        public ValidationMode ValidationMode { get; internal set; } = ValidationMode.Progressive;

        [DefaultValue(ValidationMode.Progressive)]
        public ValidationMode ScalarValidationMode { get; internal set; } = ValidationMode.Progressive;

        [DefaultValue(100)]
        public int ValidationErrorMaxEntries { get; internal set; } = 100;

        [DefaultValue(100)]
        public int ValidationWarningMaxEntries { get; internal set; } = 100;

        [DefaultValue(null)]
        public SelectionMode? SelectionMode { get; internal set; }

        internal IAsyncValidatorGroup InternalAsyncValidators { get; set; } = AsyncValidatorGroup.Empty;

        public IAsyncValidatorGroup AsyncValidators
        {
            get { return IsSealed ? InternalAsyncValidators : null; }
        }
    }
}
