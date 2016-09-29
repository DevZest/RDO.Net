using DevZest.Data.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ElementManager : RowManager
    {
        internal ElementManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy)
        {
            BlockViewList = emptyBlockViewList ? BlockViewList.Empty : BlockViewList.Create(this);
        }

        List<BlockView> _cachedBlockViews;

        private BlockView Setup(int blockOrdinal)
        {
            var blockView = CachedList.GetOrCreate(ref _cachedBlockViews, Template.BlockViewConstructor);
            blockView.Setup(this, blockOrdinal);
            return blockView;
        }

        private void Cleanup(BlockView blockView)
        {
            Debug.Assert(blockView != null);
            blockView.Cleanup();
            CachedList.Recycle(ref _cachedBlockViews, blockView);
        }

        internal BlockViewList BlockViewList { get; private set; }

        private BlockView _currentBlockView;
        internal BlockView CurrentBlockView
        {
            get { return _currentBlockView; }
            private set
            {
                Debug.Assert(CurrentBlockViewPosition == CurrentBlockViewPosition.None || CurrentBlockViewPosition == CurrentBlockViewPosition.Alone);
                _currentBlockView = value;
                CurrentBlockViewPosition = value != null ? CurrentBlockViewPosition.Alone : CurrentBlockViewPosition.None;
            }
        }

        private void CoerceCurrentBlockView(RowPresenter oldValue)
        {
            Debug.Assert(BlockViewList.Count == 0);

            var newValue = CurrentRow;
            if (newValue != null)
            {
                if (CurrentBlockView == null)
                {
                    CurrentBlockView = Setup(newValue.Index / BlockDimensions);
                    ElementCollection.Insert(HeadScalarElementsCount, CurrentBlockView);
                }
                else
                    CurrentBlockView.OnCurrentRowChanged(oldValue);
            }
            else if (CurrentBlockView != null)
                ClearCurrentBlockView();
        }

        private void ClearCurrentBlockView()
        {
            Debug.Assert(CurrentBlockView != null);
            Cleanup(CurrentBlockView);
            ElementCollection.RemoveAt(HeadScalarElementsCount);
            CurrentBlockView = null;
        }

        internal CurrentBlockViewPosition CurrentBlockViewPosition { get; private set; }

        protected BlockView this[RowView rowView]
        {
            get
            {
                if (CurrentBlockView != null && rowView.BlockOrdinal == CurrentBlockView.Ordinal)
                    return CurrentBlockView;
                return BlockViewList[rowView];
            }
        }

        internal void Realize(int blockOrdinal)
        {
            CurrentBlockViewPosition = DoRealize(blockOrdinal);
        }

        private CurrentBlockViewPosition DoRealize(int blockOrdinal)
        {
            Debug.Assert(CurrentBlockView != null);

            if (CurrentBlockView.Ordinal == blockOrdinal)
                return CurrentBlockViewPosition.WithinList;

            var index = HeadScalarElementsCount;
            switch (CurrentBlockViewPosition)
            {
                case CurrentBlockViewPosition.Alone:
                    if (blockOrdinal > CurrentBlockView.Ordinal)
                        index++;
                    break;
                case CurrentBlockViewPosition.BeforeList:
                    index++;
                    if (blockOrdinal > BlockViewList.Last.Ordinal)
                        index += BlockViewList.Count;
                    break;
                case CurrentBlockViewPosition.WithinList:
                case CurrentBlockViewPosition.AfterList:
                    if (blockOrdinal > BlockViewList.Last.Ordinal)
                        index += BlockViewList.Count;
                    break;
            }

            var blockView = Setup(blockOrdinal);
            ElementCollection.Insert(index, blockView);
            if (CurrentBlockViewPosition == CurrentBlockViewPosition.Alone)
                return blockOrdinal > CurrentBlockView.Ordinal ? CurrentBlockViewPosition.BeforeList : CurrentBlockViewPosition.AfterList;
            return CurrentBlockViewPosition;
        }

        internal int BlockViewListStartIndex
        {
            get
            {
                var result = HeadScalarElementsCount;
                if (CurrentBlockViewPosition == CurrentBlockViewPosition.BeforeList)
                    result++;
                return result;
            }
        }

        internal void VirtualizeAll()
        {
            Debug.Assert(BlockViewList.Count > 0);

            var startIndex = BlockViewListStartIndex;
            for (int i = BlockViewList.Count - 1; i >= 0; i--)
            {
                var blockView = BlockViewList[i];
                if (blockView == CurrentBlockView)
                    continue;
                Cleanup(blockView);
                ElementCollection.RemoveAt(startIndex + i);
            }

            CurrentBlockViewPosition = CurrentBlockViewPosition.Alone;
        }

        List<RowView> _cachedRowViews;

        internal RowView Setup(RowPresenter row)
        {
            Debug.Assert(row != null && row.View == null);

            var rowView = CachedList.GetOrCreate(ref _cachedRowViews, Template.RowViewConstructor);
            rowView.Setup(row);
            return rowView;
        }

        internal void Cleanup(RowPresenter row)
        {
            var rowView = row.View;
            Debug.Assert(rowView != null);
            rowView.Cleanup();
            CachedList.Recycle(ref _cachedRowViews, rowView);
        }

        internal IElementCollection ElementCollection { get; private set; }

        public IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        internal int HeadScalarElementsCount { get; private set; }

        internal void SetElementsPanel(FrameworkElement elementsPanel)
        {
            Debug.Assert(elementsPanel != null);

            if (ElementCollection != null)
                ClearElements();
            InitializeElements(elementsPanel);
        }

        internal void InitializeElements(FrameworkElement elementsPanel)
        {
            Debug.Assert(ElementCollection == null);

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);

            var scalarItems = Template.ScalarItems;
            for (int i = 0; i < scalarItems.Count; i++)
                InsertScalarElementsAfter(scalarItems[i], Elements.Count - 1, 1);
            HeadScalarElementsCount = Template.ScalarItemsSplit;
            CoerceCurrentBlockView(null);
        }

        private void RefreshElements()
        {
            if (Elements == null || Elements.Count == 0)
                return;

            RefreshScalarElements();
            RefreshBlockViews();

            _isDirty = false;
        }

        private void RefreshScalarElements()
        {
            var scalarItems = Template.ScalarItems;
            foreach (var scalarItem in scalarItems)
            {
                for (int i = 0; i < scalarItem.BlockDimensions; i++)
                {
                    var element = scalarItem[i];
                    scalarItem.Refresh(element);
                }
            }
        }

        private void RefreshBlockViews()
        {
            if (CurrentBlockView != null && CurrentBlockViewPosition != CurrentBlockViewPosition.WithinList)
                CurrentBlockView.Refresh();
            foreach (var blockView in BlockViewList)
                blockView.Refresh();
        }

        internal void ClearElements()
        {
            if (ElementCollection == null)
                return;

            BlockViewList.VirtualizeAll();
            if (CurrentBlockView != null)
                ClearCurrentBlockView();

            var scalarItems = Template.ScalarItems;
            for (int i = 0; i < scalarItems.Count; i++)
            {
                var scalarItem = scalarItems[i];
                scalarItem.CumulativeBlockDimensionsDelta = 0;
                int count = scalarItem.IsMultidimensional ? BlockDimensions : 1;
                RemoveScalarElementsAfter(scalarItem, -1, count);
            }
            Debug.Assert(Elements.Count == 0);
            _blockDimensions = 1;
            ElementCollection = null;
        }

        private int InsertScalarElementsAfter(ScalarItem scalarItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = scalarItem.Setup();
                ElementCollection.Insert(index + i + 1, element);
            }
            return index + count;
        }

        private void RemoveScalarElementsAfter(ScalarItem scalarItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = Elements[index + 1];
                Debug.Assert(element.GetTemplateItem() == scalarItem);
                scalarItem.Cleanup(element);
                ElementCollection.RemoveAt(index + 1);
            }
        }

        private int _blockDimensions = 1;
        internal int BlockDimensions
        {
            get { return _blockDimensions; }
            set
            {
                Debug.Assert(value >= 1);

                if (_blockDimensions == value)
                    return;

                var delta = value - _blockDimensions;
                _blockDimensions = value;
                OnBlockDimensionsChanged(delta);
            }
        }

        private void OnBlockDimensionsChanged(int blockDimensionsDelta)
        {
            Debug.Assert(blockDimensionsDelta != 0);

            BlockViewList.VirtualizeAll();

            var index = -1;
            var delta = 0;
            var scalarItems = Template.ScalarItems;
            for (int i = 0; i < scalarItems.Count; i++)
            {
                index++;
                if (i == Template.ScalarItemsSplit && CurrentBlockView != null)
                    index += 1;
                var scalarItem = scalarItems[i];

                var prevCumulativeBlockDimensionsDelta = i == 0 ? 0 : scalarItems[i - 1].CumulativeBlockDimensionsDelta;
                if (!scalarItem.IsMultidimensional)
                {
                    scalarItem.CumulativeBlockDimensionsDelta = prevCumulativeBlockDimensionsDelta + (BlockDimensions - 1);
                    continue;
                }
                scalarItem.CumulativeBlockDimensionsDelta = prevCumulativeBlockDimensionsDelta;

                if (i < Template.ScalarItemsSplit)
                    delta += blockDimensionsDelta;

                if (blockDimensionsDelta > 0)
                    index = InsertScalarElementsAfter(scalarItem, index, blockDimensionsDelta);
                else
                    RemoveScalarElementsAfter(scalarItem, index, -blockDimensionsDelta);
            }

            HeadScalarElementsCount += delta;

            if (CurrentBlockView != null)
                CurrentBlockView.OnBlockDimensionsChanged(blockDimensionsDelta);
        }

        protected override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            if (ElementCollection != null)
            {
                BlockViewList.VirtualizeAll();
                CoerceCurrentBlockView(oldValue);
            }
            InvalidateElements();
        }

        protected override void OnSelectedRowsChanged()
        {
            base.OnSelectedRowsChanged();
            InvalidateElements();
        }

        protected override void OnRowsChanged()
        {
            var oldCurrentRow = CurrentRow;
            base.OnRowsChanged();
            //if (oldCurrentRow == CurrentRow && CurrentBlockView != null)
            //    CurrentBlockView.OnRowsChanged();
        }

        protected override void OnRowUpdated(RowPresenter row)
        {
            base.OnRowUpdated(row);
            InvalidateElements();
        }

        private bool _isDirty;
        internal void InvalidateElements()
        {
            if (_isDirty || ElementCollection == null)
                return;

            _isDirty = true;
            BeginRefreshElements();
        }

        private void BeginRefreshElements()
        {
            Debug.Assert(ElementCollection != null && _isDirty);

            var panel = ElementCollection.Parent;
            if (panel == null)
                RefreshElements();
            else
            {
                panel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    RefreshElements();
                }));
            }
        }
    }
}
