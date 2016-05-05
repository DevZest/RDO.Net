using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace DevZest.Data.Windows
{
    public class BlockView : Control, IBlockPresenter
    {
        static BlockView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BlockView), new FrameworkPropertyMetadata(typeof(BlockView)));
        }

        public BlockView()
        {
            Ordinal = -1;
        }

        internal void Initialize(ElementManager elementManager, int ordinal)
        {
            Debug.Assert(ElementManager == null);
            ElementManager = elementManager;
            Ordinal = ordinal;
            if (ElementCollection != null)
                InitializeElements();
        }

        internal void Cleanup()
        {
            ClearMeasuredAutoLengths();
            ClearElements();
            Ordinal = -1;
            ElementManager = null;
        }

        internal ElementManager ElementManager { get; private set; }

        internal LayoutManager LayoutManager
        {
            get { return ElementManager as LayoutManager; }
        }

        private LayoutManagerXY LayoutManagerXY
        {
            get { return LayoutManager as LayoutManagerXY; }
        }

        private BlockViewCollection BlockViews
        {
            get { return LayoutManager == null ? null : LayoutManager.BlockViews; }
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

        private TemplateItemCollection<BlockItem> BlockItems
        {
            get { return ElementManager.Template.InternalBlockItems; }
        }

        private int BlockItemsSplit
        {
            get { return ElementManager.Template.BlockItemsSplit; }
        }

        internal IElementCollection ElementCollection { get; private set; }

        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        internal void SetElementsPanel(BlockElementPanel elementsPanel)
        {
            Debug.Assert(elementsPanel != null);

            if (ElementCollection != null)
                ClearElements();
            InitializeElements(elementsPanel);
        }

        internal void InitializeElements(FrameworkElement elementsPanel)
        {
            // Prevent re-entrance. This only happens in unit testing because BlockView implements both view and presenter all together.
            if (ElementCollection != null && ElementCollection.Parent == null && elementsPanel == null)
                return;

            Debug.Assert(ElementCollection == null);

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);

            if (ElementManager != null)
                InitializeElements();

        }

        private void InitializeElements()
        {
            Debug.Assert(ElementManager != null);
            var blockItems = BlockItems;
            for (int i = 0; i < BlockItemsSplit; i++)
                AddElement(blockItems[i]);

            for (int i = 0; i < ElementManager.BlockDimensions; i++)
            {
                var success = AddElement(Ordinal, i);
                if (!success)   // Exceeded the total count of the rows
                    break;
            }

            for (int i = BlockItemsSplit; i < BlockItems.Count; i++)
                AddElement(blockItems[i]);

            // Initialization happens only after all elements are generated because BlockView implements both view and presenter all together.
            ElementManager.Template.InitializeBlockView(this);
        }

        private void AddElement(BlockItem blockItem)
        {
            var element = blockItem.Generate();
            AddElement(element);
            blockItem.Initialize(element);
        }

        private bool AddElement(int blockIndex, int offset)
        {
            var rows = ElementManager.Rows;
            var index = blockIndex * ElementManager.BlockDimensions + offset;
            if (index >= rows.Count)
                return false;
            var row = rows[index];
            var rowView = ElementManager.Realize(row);
            AddElement(rowView);
            return true;
        }

        private void AddElement(UIElement element)
        {
            ElementCollection.Add(element);
            element.SetBlockPresenter(this);
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
                RemoveLastRow();

            for (int i = BlockItemsSplit - 1; i >= 0 ; i--)
                RemoveLastElement(blockItems[i]);

            ElementCollection = null;
        }

        private void RemoveLastElement(BlockItem blockItem)
        {
            var lastIndex = Elements.Count - 1;
            var element = Elements[lastIndex];
            blockItem.Cleanup(element);
            RemoveAt(lastIndex);
        }

        private void RemoveLastRow()
        {
            var lastIndex = Elements.Count - 1;
            var rowView = (RowView)Elements[lastIndex];
            ElementManager.Virtualize(rowView.RowPresenter);
            RemoveAt(lastIndex);
        }

        private void RemoveAt(int index)
        {
            Elements[index].SetBlockPresenter(null);
            ElementCollection.RemoveAt(index);
        }

        internal void RefreshElements()
        {
            if (Elements == null)
                return;

            var blockItems = BlockItems;
            int blockDimensions = Elements.Count - blockItems.Count;
            var index = 0;

            for (int i = 0; i < BlockItemsSplit; i++)
                RefreshElement(blockItems[i], index++);

            for (int i = 0; i < blockDimensions; i++)
                ((RowView)Elements[index++]).RowPresenter.RefreshElements();

            for (int i = BlockItemsSplit; i < BlockItems.Count; i++)
                RefreshElement(blockItems[i], index++);
        }

        private void RefreshElement(BlockItem blockItem, int index)
        {
            var element = Elements[index];
            blockItem.UpdateTarget(element);
        }

        private double[] _cumulativeMeasuredAutoLengths;
        private double[] CumulativeMeasuredAutoLengths
        {
            get
            {
                Debug.Assert(LayoutManagerXY.VariantAutoLengthTracks.Count > 0);
                return _cumulativeMeasuredAutoLengths ?? (_cumulativeMeasuredAutoLengths = new double[LayoutManagerXY.VariantAutoLengthTracks.Count]);
            }
        }

        private void ClearMeasuredAutoLengths()
        {
            if (_cumulativeMeasuredAutoLengths != null)
            {
                for (int i = 0; i < _cumulativeMeasuredAutoLengths.Length; i++)
                    _cumulativeMeasuredAutoLengths[i] = 0;
            }
        }

        internal double GetMeasuredAutoLengthStartOffset(GridTrack gridTrack)
        {
            return GetMeasuredAutoLengthEndOffset(gridTrack) - GetMeasuredAutoLength(gridTrack);
        }

        internal double GetMeasuredAutoLengthEndOffset(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.IsVariantAutoLength);
            int index = gridTrack.VariantAutoLengthIndex;
            return CumulativeMeasuredAutoLengths[index];
        }

        internal double GetMeasuredAutoLength(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.IsVariantAutoLength);
            int index = gridTrack.VariantAutoLengthIndex;
            return index == 0 ? CumulativeMeasuredAutoLengths[0] : CumulativeMeasuredAutoLengths[index] - CumulativeMeasuredAutoLengths[index - 1];
        }

        internal void SetMeasuredAutoLength(GridTrack gridTrack, double value)
        {
            Debug.Assert(gridTrack != null && gridTrack.IsVariantAutoLength);
            var oldValue = GetMeasuredAutoLength(gridTrack);
            var delta = value - oldValue;
            if (delta == 0)
                return;

            var index = gridTrack.VariantAutoLengthIndex;
            for (int i = index; i < CumulativeMeasuredAutoLengths.Length; i++)
                CumulativeMeasuredAutoLengths[i] += delta;
            LayoutManagerXY.InvalidateVariantAutoLengths();
        }

        internal double TotalMeasuredAutoLength
        {
            get { return _cumulativeMeasuredAutoLengths == null ? 0 : _cumulativeMeasuredAutoLengths[_cumulativeMeasuredAutoLengths.Length - 1]; }
        }

        private double _startMeasuredAutoLengthOffset;
        internal double StartMeasuredAutoLengthOffset
        {
            get
            {
                LayoutManagerXY.RefreshVariantAutoLengths();
                return _startMeasuredAutoLengthOffset;
            }
            set { _startMeasuredAutoLengthOffset = value; }
        }

        internal double EndMeasuredAutoLengthOffset
        {
            get { return StartMeasuredAutoLengthOffset + TotalMeasuredAutoLength; }
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
