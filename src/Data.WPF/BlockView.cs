using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace DevZest.Data.Windows
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(BlockElementPanel))]
    public class BlockView : Control, IReadOnlyList<RowPresenter>
    {
        private static readonly DependencyPropertyKey OrdinalPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Ordinal),
            typeof(int), typeof(BlockView), new FrameworkPropertyMetadata(-1));
        public static readonly DependencyProperty OrdinalProperty = OrdinalPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey DimensionsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Dimensions),
            typeof(int), typeof(BlockView), new FrameworkPropertyMetadata(1));
        public static readonly DependencyProperty DimensionsProperty = DimensionsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CountPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Count),
             typeof(int), typeof(BlockView), new FrameworkPropertyMetadata(0));
        public static readonly DependencyProperty CountProperty = CountPropertyKey.DependencyProperty;

        static BlockView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BlockView), new FrameworkPropertyMetadata(typeof(BlockView)));
        }

        public BlockView()
        {
        }

        internal void Setup(ElementManager elementManager, int ordinal)
        {
            ElementManager = elementManager;
            RefreshDependencyProperties(ordinal);
            SetupElements();
        }

        private void RefreshDependencyProperties(int ordinal)
        {
            SetValue(OrdinalPropertyKey, ordinal);
            SetValue(CountPropertyKey, GetCount());
            SetValue(DimensionsPropertyKey, GetDimensions());
        }

        private void ClearDependencyProperties()
        {
            ClearValue(OrdinalPropertyKey);
            ClearValue(CountPropertyKey);
            ClearValue(DimensionsPropertyKey);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null)
                return;

            var panel = Template.FindName("PART_Panel", this) as BlockElementPanel;
            if (panel != null)
                Setup(panel);
        }

        internal void Setup(FrameworkElement elementsPanel)
        {
            if (ElementCollection != null)
            {
                if (ElementCollection.Parent == elementsPanel)
                    return;

                CleanupElements();
            }

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);
            SetupElements();
        }

        private void SetupElements()
        {
            if (ElementManager == null || ElementCollection == null)
                return;

            AddElements();
            OnSetup();
        }

        protected virtual void OnSetup()
        {
        }

        internal void Cleanup()
        {
            CleanupElements();
            ClearDependencyProperties();
            ElementManager = null;
        }

        private void CleanupElements()
        {
            InternalCleanup();
            ClearElements();
        }

        private void InternalCleanup()
        {
            OnCleanup();
            ClearMeasuredLengths();
        }

        protected virtual void OnCleanup()
        {
        }

        internal ElementManager ElementManager { get; private set; }

        internal LayoutManager LayoutManager
        {
            get { return ElementManager as LayoutManager; }
        }

        private LayoutXYManager LayoutXYManager
        {
            get { return LayoutManager as LayoutXYManager; }
        }

        protected DataPresenter DataPresenter
        {
            get { return LayoutManager == null ? null : LayoutManager.DataPresenter; }
        }

        public int Dimensions
        {
            get { return (int)GetValue(DimensionsProperty); }
        }

        private int GetDimensions()
        {
            return ElementManager == null ? 1 : ElementManager.BlockDimensions;
        }

        public int Ordinal
        {
            get { return (int)GetValue(OrdinalProperty); }
        }

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
        }

        private int GetCount()
        {
            if (ElementManager == null)
                return 0;

            var blockDimensions = ElementManager.BlockDimensions;
            var nextBlockFirstRowOrdinal = (Ordinal + 1) * blockDimensions;
            var rowCount = ElementManager.Rows.Count;
            return nextBlockFirstRowOrdinal <= rowCount ? blockDimensions : blockDimensions - (nextBlockFirstRowOrdinal - rowCount);
        }

        public RowPresenter this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return ElementManager.Rows[Ordinal * ElementManager.BlockDimensions + index];
            }
        }

        public IEnumerator<RowPresenter> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private RecapBindingCollection<BlockBinding> BlockBindings
        {
            get { return ElementManager.Template.InternalBlockBindings; }
        }

        private int BlockBindingsSplit
        {
            get { return ElementManager.Template.BlockBindingsSplit; }
        }

        private IElementCollection ElementCollection { get; set; }

        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        private void AddElements()
        {
            var blockBindings = BlockBindings;
            for (int i = 0; i < BlockBindingsSplit; i++)
                AddElement(blockBindings[i]);

            for (int i = 0; i < ElementManager.BlockDimensions; i++)
            {
                var success = AddRowView(i);
                if (!success)   // Exceeded the total count of the rows
                    break;
            }

            for (int i = BlockBindingsSplit; i < BlockBindings.Count; i++)
                AddElement(blockBindings[i]);
        }

        private void AddElement(BlockBinding blockBinding)
        {
            var element = blockBinding.Setup(this);
            ElementCollection.Add(element);
        }

        private bool AddRowView(int offset)
        {
            var rows = ElementManager.Rows;
            var rowIndex = Ordinal * ElementManager.BlockDimensions + offset;
            if (rowIndex >= rows.Count)
                return false;
            var row = rows[rowIndex];
            var rowView = ElementManager.Setup(row);
            ElementCollection.Insert(BlockBindingsSplit + offset, rowView);
            return true;
        }

        private void ClearElements()
        {
            if (ElementCollection == null)
                return;

            int blockDimensions = Elements.Count - BlockBindings.Count;

            var blockBindings = BlockBindings;
            for (int i = BlockBindings.Count - 1; i >= BlockBindingsSplit; i--)
                RemoveLastElement(blockBindings[i]);

            for (int i = blockDimensions - 1; i >= 0; i--)
                RemoveRowViewAt(Elements.Count - 1);

            for (int i = BlockBindingsSplit - 1; i >= 0 ; i--)
                RemoveLastElement(blockBindings[i]);
        }

        private void RemoveLastElement(BlockBinding blockBinding)
        {
            var lastIndex = Elements.Count - 1;
            var element = Elements[lastIndex];
            blockBinding.Cleanup(element);
            RemoveAt(lastIndex);
        }

        private void RemoveRowViewAt(int index)
        {
            var rowView = (RowView)Elements[index];
            ElementManager.Cleanup(rowView.RowPresenter);
            RemoveAt(index);
        }

        private void RemoveAt(int index)
        {
            ElementCollection.RemoveAt(index);
        }

        internal void Refresh(bool isReload)
        {
            if (Elements == null)
                return;

            var blockBindings = BlockBindings;
            int blockDimensions = Elements.Count - blockBindings.Count;
            var index = 0;

            for (int i = 0; i < BlockBindingsSplit; i++)
                Refresh(blockBindings[i], index++);

            for (int i = 0; i < blockDimensions; i++)
                ((RowView)Elements[index++]).Refresh(isReload);

            for (int i = BlockBindingsSplit; i < BlockBindings.Count; i++)
                Refresh(blockBindings[i], index++);
        }

        private void Refresh(BlockBinding blockBinding, int index)
        {
            var element = Elements[index];
            blockBinding.Refresh(element);
        }

        internal void Reload()
        {
            Reload(ElementManager.CurrentRow);
        }

        internal void Reload(RowPresenter oldCurrentRow)
        {
            Debug.Assert(ElementManager.CurrentBlockView == this && ElementManager.CurrentBlockViewPosition == CurrentBlockViewPosition.Alone);

            var currentRow = ElementManager.CurrentRow;
            var currentRowView = RemoveAllRowViewsExcept(oldCurrentRow);
            if (oldCurrentRow != currentRow)
                currentRowView.Reload(currentRow);
            FillMissingRowViews(currentRowView);
            RefreshDependencyProperties(currentRow.Index / ElementManager.BlockDimensions);
            Refresh(true);
        }

        internal void ReloadIfInvalid()
        {
            if (IsInvalid)
                Reload();
        }

        private bool IsInvalid
        {
            get
            {
                var startRowIndex = Ordinal * ElementManager.BlockDimensions;
                var startIndex = BlockBindingsSplit;
                int blockDimensions = Elements.Count - BlockBindings.Count;
                for (int i = 0; i < blockDimensions; i++)
                {
                    var index = startIndex + i;
                    var rowView = (RowView)Elements[index];
                    if (rowView.RowPresenter.Index != startRowIndex + i)
                        return true;
                }
                return false;
            }
        }

        private RowView RemoveAllRowViewsExcept(RowPresenter row)
        {
            InternalCleanup();

            RowView result = null;
            var startIndex = BlockBindingsSplit;
            int blockDimensions = Elements.Count - BlockBindings.Count;
            for (int i = blockDimensions - 1; i >= 0; i--)
            {
                var index = startIndex + i;
                var rowView = (RowView)Elements[index];
                if (rowView.RowPresenter == row)
                    result = rowView;
                else
                    RemoveRowViewAt(index);
            }
            Debug.Assert(result != null);
            return result;
        }

        private void FillMissingRowViews(RowView currentRowView)
        {
            var currentRowIndex = currentRowView.RowPresenter.Index;
            var blockDimensions = ElementManager.BlockDimensions;
            var offset = currentRowIndex % blockDimensions;

            for (int i = 0; i < offset; i++)
                AddRowView(i);

            for (int i = offset + 1; i < blockDimensions; i++)
            {
                var success = AddRowView(i);
                if (!success)   // Exceeded the total count of the rows
                    break;
            }

            OnSetup();
        }

        private GridSpan VariantByBlockGridSpan
        {
            get { return LayoutXYManager.VariantByBlockGridSpan; }
        }

        private double[] _cumulativeMeasuredLengths;
        private double[] CumulativeMeasuredLengths
        {
            get
            {
                Debug.Assert(VariantByBlockGridSpan.Count > 0);
                return _cumulativeMeasuredLengths ?? (_cumulativeMeasuredLengths = InitCumulativeMeasuredLengths());
            }
        }

        private double[] InitCumulativeMeasuredLengths()
        {
            Debug.Assert(VariantByBlockGridSpan.Count > 0);
            var result = new double[VariantByBlockGridSpan.Count];
            ClearMeasuredLengths();
            return result;
        }

        private void ClearMeasuredLengths()
        {
            if (_cumulativeMeasuredLengths == null)
                return;

            double totalLength = 0;
            var gridSpan = LayoutXYManager.VariantByBlockGridSpan;
            Debug.Assert(gridSpan.Count == _cumulativeMeasuredLengths.Length);
            for (int i = 0; i < _cumulativeMeasuredLengths.Length; i++)
            {
                var gridTrack = gridSpan[i];
                if (!gridTrack.IsAutoLength)
                    totalLength += gridTrack.Length.Value;
                _cumulativeMeasuredLengths[i] = totalLength;
            }

            _startOffset = 0;
        }

        internal Span GetReleativeSpan(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByBlock);
            return new Span(GetRelativeStartOffset(gridTrack), GetRelativeEndOffset(gridTrack));
        }

        private double GetRelativeStartOffset(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByBlock);
            return GetRelativeEndOffset(gridTrack) - GetMeasuredLength(gridTrack);
        }

        private double GetRelativeEndOffset(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByBlock);
            return CumulativeMeasuredLengths[gridTrack.VariantByBlockIndex];
        }

        internal double GetMeasuredLength(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByBlock);
            int index = gridTrack.VariantByBlockIndex;
            return index == 0 ? CumulativeMeasuredLengths[0] : CumulativeMeasuredLengths[index] - CumulativeMeasuredLengths[index - 1];
        }

        internal void SetMeasuredLength(GridTrack gridTrack, double value)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByBlock);
            var oldValue = GetMeasuredLength(gridTrack);
            var delta = value - oldValue;
            if (delta == 0)
                return;

            var index = gridTrack.VariantByBlockIndex;
            for (int i = index; i < CumulativeMeasuredLengths.Length; i++)
                CumulativeMeasuredLengths[i] += delta;
            LayoutXYManager.InvalidateBlockLengths();
        }

        private double MeasuredLength
        {
            get
            {
                Debug.Assert(VariantByBlockGridSpan.Count > 0);
                var cumulativeMeasuredLengths = CumulativeMeasuredLengths;
                return cumulativeMeasuredLengths[cumulativeMeasuredLengths.Length - 1];
            }
        }

        private double _startOffset;
        internal double StartOffset
        {
            get
            {
                Debug.Assert(VariantByBlockGridSpan.Count > 0);
                LayoutXYManager.RefreshBlockLengths();
                return _startOffset;
            }
            set
            {
                Debug.Assert(VariantByBlockGridSpan.Count > 0);
                _startOffset = value;
            }
        }

        internal double EndOffset
        {
            get { return StartOffset + MeasuredLength; }
        }

        internal UIElement this[BlockBinding blockBinding]
        {
            get
            {
                var index = blockBinding.Ordinal;
                if (index >= BlockBindingsSplit)
                    index += Count;
                return Elements[index];
            }
        }
    }
}
