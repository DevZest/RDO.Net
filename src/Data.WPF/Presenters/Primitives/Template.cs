using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
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

        internal ScrollableManager ScrollableManager
        {
            get { return RowManager as ScrollableManager; }
        }

        public BasePresenter Presenter
        {
            get { return LayoutManager == null ? null : LayoutManager.Presenter; }
        }

        public DataPresenter DataPresenter
        {
            get { return Presenter as DataPresenter; }
        }

        public Orientation? Orientation { get; private set; }

        public int FlowRepeatCount { get; private set; } = 1;

        internal ContainerKind ContainerKind
        {
            get { return BlockBindings.Count > 0 || (Orientation.HasValue && FlowRepeatCount != 1) ? ContainerKind.Block : ContainerKind.Row; }
        }

        internal void Layout(Orientation orientation, int flowRepeatCount = 1)
        {
            Debug.Assert(flowRepeatCount >= 0);
            Orientation = orientation;
            FlowRepeatCount = flowRepeatCount;
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

        public GridRange ContainerRange
        {
            get { return RowRange.Union(InternalBlockBindings.Range); }
        }

        internal void Seal()
        {
            VerifyGridUnitType();
            VerifyRowRange();
            VerifyFrozenMargins();
            _scalarAsyncValidators = _scalarAsyncValidators.Seal();
            _rowAsyncValidators = _rowAsyncValidators.Seal();
        }

        internal void VerifyGridUnitType()
        {
            GridColumns.ForEach(x => x.VerifyUnitType());
            GridRows.ForEach(x => x.VerifyUnitType());
        }

        private void VerifyRowRange()
        {
            if (RowRange.IsEmpty)
                throw new InvalidOperationException(DiagnosticMessages.Template_EmptyRowRange);

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

        internal bool Flowable(Orientation orientation)
        {
            return !Orientation.HasValue ? false : Orientation.GetValueOrDefault() != orientation && FlowRepeatCount != 1;
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

        internal ContainerView CreateContainerView()
        {
            if (ContainerKind == ContainerKind.Row)
                return CreateRowView();
            else
                return CreateBlockView();
        }

        private BlockView CreateBlockView()
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
                InvalidateGridColumns();
            }
        }

        internal void InvalidateGridColumns()
        {
            InternalGridColumns.InvalidateStarLengthTracks();
            InternalScalarBindings.InvalidateAutoWidthBindings();
            InternalBlockBindings.InvalidateAutoWidthBindings();
            InternalRowBindings.InvalidateAutoWidthBindings();
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

                InvalidateGridRows();
            }
        }

        internal void InvalidateGridRows()
        {
            InternalGridRows.InvalidateStarLengthTracks();
            InternalScalarBindings.InvalidateAutoHeightBindings();
            InternalBlockBindings.InvalidateAutoHeightBindings();
            InternalRowBindings.InvalidateAutoHeightBindings();
        }

        internal bool SizeToContentX
        {
            get { return double.IsPositiveInfinity(_availableWidth); }
        }

        internal bool SizeToContentY
        {
            get { return double.IsPositiveInfinity(_availableHeight); }
        }

        internal int CoerceFlowRepeatCount()
        {
            if (!Orientation.HasValue || FlowRepeatCount == 1)
                return 1;

            if (FlowRepeatCount != 0)
                return FlowRepeatCount;

            return Orientation.GetValueOrDefault() == System.Windows.Controls.Orientation.Vertical
                ? CoerceFlowRepeatCount(SizeToContentX, AvailableWidth, InternalGridColumns)
                : CoerceFlowRepeatCount(SizeToContentY, AvailableHeight, InternalGridRows);
        }

        private int CoerceFlowRepeatCount<T>(bool sizeToContent, double availableLength, GridTrackCollection<T> gridTracks)
            where T : GridTrack, IConcatList<T>
        {
            if (sizeToContent)
                return 1;

            return FlowRepeatCount > 0 ? FlowRepeatCount : Math.Max(1, (int)(availableLength / gridTracks.TotalAbsoluteLength));
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

        [DefaultValue(ValidationMode.Progressive)]
        public ValidationMode RowValidationMode { get; internal set; } = ValidationMode.Progressive;

        [DefaultValue(ValidationMode.Progressive)]
        public ValidationMode ScalarValidationMode { get; internal set; } = ValidationMode.Progressive;

        [DefaultValue(SelectionMode.Extended)]
        public SelectionMode SelectionMode { get; internal set; } = SelectionMode.Extended;

        private IScalarAsyncValidators _scalarAsyncValidators = Presenters.ScalarAsyncValidators.Empty;

        internal void AddAsyncValidator(ScalarAsyncValidator validator)
        {
            Debug.Assert(validator != null);
            validator.Initialize(this);
            _scalarAsyncValidators = _scalarAsyncValidators.Add(validator);
        }

        public IScalarAsyncValidators ScalarAsyncValidators
        {
            get { return IsSealed ? _scalarAsyncValidators : null; }
        }

        private IRowAsyncValidators _rowAsyncValidators = Presenters.RowAsyncValidators.Empty;

        internal void AddAsyncValidator(RowAsyncValidator validator)
        {
            Debug.Assert(validator != null);
            validator.Initialize(this);
            _rowAsyncValidators = _rowAsyncValidators.Add(validator);
        }

        public IRowAsyncValidators RowAsyncValidators
        {
            get { return IsSealed ? _rowAsyncValidators : null; }
        }

        private ScalarPresenter _scalarPresenter;
        internal ScalarPresenter ScalarPresenter
        {
            get
            {
                if (_scalarPresenter == null)
                    _scalarPresenter = new ScalarPresenter(this);
                return _scalarPresenter;
            }
        }

        private BlockPresenter _blockPresenter;
        internal BlockPresenter BlockPresenter
        {
            get
            {
                if (_blockPresenter == null)
                    _blockPresenter = new BlockPresenter(this);
                return _blockPresenter;
            }
        }

        private List<BlockViewBehavior> _blockViewBehaviors;
        public IReadOnlyList<BlockViewBehavior> BlockViewBehaviors
        {
            get
            {
                if (_blockViewBehaviors == null)
                    return Array.Empty<BlockViewBehavior>();
                else
                    return _blockViewBehaviors;
            }
        }

        internal void AddBehavior(BlockViewBehavior behavior)
        {
            Debug.Assert(behavior != null);
            if (_blockViewBehaviors == null)
                _blockViewBehaviors = new List<BlockViewBehavior>();
            _blockViewBehaviors.Add(behavior);
        }

        internal List<RowViewBehavior> _rowViewBehaviors;
        public IReadOnlyList<RowViewBehavior> RowViewBehaviors
        {
            get
            {
                if (_rowViewBehaviors == null)
                    return Array.Empty<RowViewBehavior>();
                else
                    return _rowViewBehaviors;
            }
        }

        internal void AddBehavior(RowViewBehavior behavior)
        {
            Debug.Assert(behavior != null);
            if (_rowViewBehaviors == null)
                _rowViewBehaviors = new List<RowViewBehavior>();
            _rowViewBehaviors.Add(behavior);
        }

        internal void InitializeAsInherited()
        {
            RowManager = null;
            InternalScalarBindings.Clear();
            InternalBlockBindings.Clear();
            InternalRowBindings.Clear();
            _scalarAsyncValidators = Presenters.ScalarAsyncValidators.Empty;
            _rowAsyncValidators = Presenters.RowAsyncValidators.Empty;
        }

        private bool _initialFocusFlag = true;
        public InitialFocus InitialFocus { get; internal set; } = InitialFocus.First;

        internal void ResetInitialFocus()
        {
            _initialFocusFlag = true;
        }

        internal void InitFocus()
        {
            if (_initialFocusFlag)
            {
                _initialFocusFlag = false;
                InitialFocus.MoveFocus(Presenter);
            }
        }

        [DefaultValue(false)]
        public bool AllowsDelete { get; internal set; }

        private List<AttachedScalarBinding> _attachedScalarBindings;
        public IReadOnlyList<ScalarBinding> AttachedScalarBindings
        {
            get
            {
                if (_attachedScalarBindings == null)
                    return Array.Empty<ScalarBinding>();
                return _attachedScalarBindings;
            }
        }

        internal void AddBinding<T>(T element, ScalarBinding<T> scalarBinding)
            where T : UIElement, new()
        {
            element.VerifyNotNull(nameof(element));
            if (element.GetAttachedTo() != null)
                throw new ArgumentException(DiagnosticMessages.CommonPresenter_ElementAttachedAlready, nameof(element));
            scalarBinding.VerifyNotNull(nameof(scalarBinding));
            if (scalarBinding.IsSealed)
                throw new ArgumentException(DiagnosticMessages.Binding_VerifyNotSealed, nameof(scalarBinding));

            var result = AttachedScalarBinding.Attach(this, element, scalarBinding);
            if (_attachedScalarBindings == null)
                _attachedScalarBindings = new List<AttachedScalarBinding>();
            _attachedScalarBindings.Add(result);
        }

        internal void MountAttachedScalarBindings()
        {
            if (_attachedScalarBindings == null)
                return;

            for (int i = 0; i < _attachedScalarBindings.Count; i++)
            {
                var binding = _attachedScalarBindings[i];
                binding.BaseBinding.BeginSetup(binding.Element);
            }

            for (int i = 0; i < _attachedScalarBindings.Count; i++)
            {
                var binding = _attachedScalarBindings[i];
                binding.BaseBinding.Setup(0);
                binding.BaseBinding.Refresh(binding.Element);
            }

            for (int i = 0; i < _attachedScalarBindings.Count; i++)
            {
                var binding = _attachedScalarBindings[i];
                binding.BaseBinding.EndSetup();
                binding.Mount();
            }
        }

        internal void UnmountAttachedScalarBindings()
        {
            if (_attachedScalarBindings == null)
                return;

            for (int i = 0; i < _attachedScalarBindings.Count; i++)
                _attachedScalarBindings[i].Dispose();
        }
    }
}
