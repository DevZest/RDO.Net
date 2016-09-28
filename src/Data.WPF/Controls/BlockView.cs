using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace DevZest.Data.Windows.Controls
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(BlockElementPanel))]
    public class BlockView : Control, IReadOnlyList<RowPresenter>
    {
        static BlockView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BlockView), new FrameworkPropertyMetadata(typeof(BlockView)));
        }

        public BlockView()
        {
            Ordinal = -1;
        }

        internal void Setup(ElementManager elementManager, int ordinal)
        {
            ElementManager = elementManager;
            Ordinal = ordinal;
            SetupElements();
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

        private void OnSetup()
        {
            if (ElementManager.Template.OnSetupBlockView != null)
                ElementManager.Template.OnSetupBlockView(this);
        }

        internal void Cleanup()
        {
            CleanupElements();
            Ordinal = -1;
            ElementManager = null;
        }

        private void CleanupElements()
        {
            OnCleanup();
            ClearElements();
        }

        private void OnCleanup()
        {
            if (ElementManager.Template.OnCleanupBlockView != null)
                ElementManager.Template.OnCleanupBlockView(this);
            ClearMeasuredLengths();
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

        public DataPresenter DataPresenter
        {
            get { return LayoutManager == null ? null : LayoutManager.DataPresenter; }
        }

        public int Dimensions
        {
            get { return ElementManager == null ? 1 : ElementManager.BlockDimensions; }
        }

        public int Ordinal { get; private set; }

        public int Count
        {
            get
            {
                if (ElementManager == null)
                    return 0;

                var blockDimensions = ElementManager.BlockDimensions;
                var nextBlockFirstRowOrdinal = (Ordinal + 1) * blockDimensions;
                var rowCount = ElementManager.Rows.Count;
                return nextBlockFirstRowOrdinal <= rowCount ? blockDimensions : blockDimensions - (nextBlockFirstRowOrdinal - rowCount);
            }
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

        private RecapItemCollection<BlockItem> BlockItems
        {
            get { return ElementManager.Template.InternalBlockItems; }
        }

        private int BlockItemsSplit
        {
            get { return ElementManager.Template.BlockItemsSplit; }
        }

        private IElementCollection ElementCollection { get; set; }

        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        private void AddElements()
        {
            var blockItems = BlockItems;
            for (int i = 0; i < BlockItemsSplit; i++)
                AddElement(blockItems[i]);

            for (int i = 0; i < ElementManager.BlockDimensions; i++)
            {
                var success = AddRowView(i);
                if (!success)   // Exceeded the total count of the rows
                    break;
            }

            for (int i = BlockItemsSplit; i < BlockItems.Count; i++)
                AddElement(blockItems[i]);
        }

        private void AddElement(BlockItem blockItem)
        {
            var element = blockItem.Setup(this);
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
            ElementCollection.Insert(BlockItemsSplit + offset, rowView);
            return true;
        }

        private void ClearElements()
        {
            if (ElementCollection == null)
                return;

            int blockDimensions = Elements.Count - BlockItems.Count;

            var blockItems = BlockItems;
            for (int i = BlockItems.Count - 1; i >= BlockItemsSplit; i--)
                RemoveLastElement(blockItems[i]);

            for (int i = blockDimensions - 1; i >= 0; i--)
                RemoveRowViewAt(Elements.Count - 1);

            for (int i = BlockItemsSplit - 1; i >= 0 ; i--)
                RemoveLastElement(blockItems[i]);
        }

        private void RemoveLastElement(BlockItem blockItem)
        {
            var lastIndex = Elements.Count - 1;
            var element = Elements[lastIndex];
            blockItem.Cleanup(element);
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

        internal void Refresh()
        {
            if (Elements == null)
                return;

            var blockItems = BlockItems;
            int blockDimensions = Elements.Count - blockItems.Count;
            var index = 0;

            for (int i = 0; i < BlockItemsSplit; i++)
                Refresh(blockItems[i], index++);

            for (int i = 0; i < blockDimensions; i++)
                ((RowView)Elements[index++]).Refresh();

            for (int i = BlockItemsSplit; i < BlockItems.Count; i++)
                Refresh(blockItems[i], index++);
        }

        private void Refresh(BlockItem blockItem, int index)
        {
            var element = Elements[index];
            blockItem.Refresh(element);
        }

        internal void OnCurrentRowChanged(RowPresenter oldValue)
        {
            Debug.Assert(ElementManager.CurrentBlockView == this);

            var currentRowView = RemoveAllRowViewsExcept(oldValue);
            currentRowView.Reload(ElementManager.CurrentRow);
            FillMissingRowViews(currentRowView);
        }

        internal void OnBlockDimensionsChanged(int blockDimensionsDelta)
        {
            Debug.Assert(ElementManager.CurrentBlockView == this);

            var currentRowView = RemoveAllRowViewsExcept(ElementManager.CurrentRow);
            FillMissingRowViews(currentRowView);
        }

        private RowView RemoveAllRowViewsExcept(RowPresenter row)
        {
            OnCleanup();

            RowView result = null;
            var startIndex = BlockItemsSplit;
            int blockDimensions = Elements.Count - BlockItems.Count;
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
            Ordinal = currentRowIndex / blockDimensions;
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

        internal UIElement this[BlockItem blockItem]
        {
            get
            {
                var index = blockItem.Ordinal;
                if (index >= BlockItemsSplit)
                    index += Count;
                return Elements[index];
            }
        }
    }
}
