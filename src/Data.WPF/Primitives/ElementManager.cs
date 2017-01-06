using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ElementManager : RowManager
    {
        internal ElementManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy)
        {
            ContainerViewList = emptyContainerViewList ? ContainerViewList.Empty : ContainerViewList.Create(this);
        }

        List<BlockView> _cachedBlockViews;
        List<RowView> _cachedRowViews;

        private ContainerView Setup(int ordinal)
        {
            if (Template.ContainerKind == ContainerKind.Row)
            {
                var rowView = CachedList.GetOrCreate(ref _cachedRowViews, Template.CreateRowView);
                rowView.Setup(Rows[ordinal]);
                return rowView;
            }
            else
            {
                var blockView = CachedList.GetOrCreate(ref _cachedBlockViews, Template.CreateBlockView);
                blockView.Setup(this, ordinal);
                return blockView;
            }
        }

        private void Cleanup(ContainerView containerView)
        {
            Debug.Assert(containerView != null);
            containerView.Cleanup();
            if (containerView is RowView)
                CachedList.Recycle(ref _cachedRowViews, (RowView)containerView);
            else
                CachedList.Recycle(ref _cachedBlockViews, (BlockView)containerView);
        }

        internal ContainerViewList ContainerViewList { get; private set; }

        private ContainerView _currentContainerView;
        internal ContainerView CurrentContainerView
        {
            get { return _currentContainerView; }
            private set
            {
                Debug.Assert(CurrentContainerViewPosition == CurrentContainerViewPosition.None || CurrentContainerViewPosition == CurrentContainerViewPosition.Alone);
                _currentContainerView = value;
                CurrentContainerViewPosition = value != null ? CurrentContainerViewPosition.Alone : CurrentContainerViewPosition.None;
            }
        }

        private void CoerceCurrentContainerView(RowPresenter oldValue)
        {
            Debug.Assert(ContainerViewList.Count == 0);

            var newValue = CurrentRow;
            if (newValue != null)
            {
                if (CurrentContainerView == null)
                {
                    CurrentContainerView = Setup(newValue.Index / BlockDimensions);
                    ElementCollection.Insert(HeadScalarElementsCount, CurrentContainerView);
                }
                else
                    CurrentContainerView.OnCurrentRowChanged(oldValue);
            }
            else if (CurrentContainerView != null)
                ClearCurrentContainerView();
        }

        private void ClearCurrentContainerView()
        {
            Debug.Assert(CurrentContainerView != null);
            Cleanup(CurrentContainerView);
            ElementCollection.RemoveAt(HeadScalarElementsCount);
            CurrentContainerView = null;
        }

        internal CurrentContainerViewPosition CurrentContainerViewPosition { get; private set; }

        internal bool IsCurrentContainerViewIsolated
        {
            get
            {
                return CurrentContainerViewPosition == CurrentContainerViewPosition.Alone
                    || CurrentContainerViewPosition == CurrentContainerViewPosition.BeforeList
                    || CurrentContainerViewPosition == CurrentContainerViewPosition.AfterList;
            }
        }

        protected ContainerView this[RowView rowView]
        {
            get
            {
                if (CurrentContainerView != null && rowView.BlockOrdinal == CurrentContainerView.ContainerOrdinal)
                    return CurrentContainerView;
                return ContainerViewList[rowView];
            }
        }

        internal void Realize(int ordinal)
        {
            CurrentContainerViewPosition = DoRealize(ordinal);
        }

        private CurrentContainerViewPosition DoRealize(int ordinal)
        {
            Debug.Assert(CurrentContainerView != null);

            if (CurrentContainerView.ContainerOrdinal == ordinal)
                return CurrentContainerViewPosition.WithinList;

            var index = HeadScalarElementsCount;
            switch (CurrentContainerViewPosition)
            {
                case CurrentContainerViewPosition.Alone:
                    if (ordinal > CurrentContainerView.ContainerOrdinal)
                        index++;
                    break;
                case CurrentContainerViewPosition.BeforeList:
                    index++;
                    if (ordinal > ContainerViewList.Last.ContainerOrdinal)
                        index += ContainerViewList.Count;
                    break;
                case CurrentContainerViewPosition.WithinList:
                case CurrentContainerViewPosition.AfterList:
                    if (ordinal > ContainerViewList.Last.ContainerOrdinal)
                        index += ContainerViewList.Count;
                    break;
            }

            var containerView = Setup(ordinal);
            ElementCollection.Insert(index, containerView);
            if (CurrentContainerViewPosition == CurrentContainerViewPosition.Alone)
                return ordinal > CurrentContainerView.ContainerOrdinal ? CurrentContainerViewPosition.BeforeList : CurrentContainerViewPosition.AfterList;
            return CurrentContainerViewPosition;
        }

        internal int ContainerViewListStartIndex
        {
            get
            {
                var result = HeadScalarElementsCount;
                if (CurrentContainerViewPosition == CurrentContainerViewPosition.BeforeList)
                    result++;
                return result;
            }
        }

        internal void VirtualizeContainerViewList()
        {
            Debug.Assert(ContainerViewList.Count > 0);

            var startIndex = ContainerViewListStartIndex;
            for (int i = ContainerViewList.Count - 1; i >= 0; i--)
            {
                var containerView = ContainerViewList[i];
                if (containerView == CurrentContainerView)
                    continue;
                Cleanup(containerView);
                ElementCollection.RemoveAt(startIndex + i);
            }

            CurrentContainerViewPosition = CurrentContainerViewPosition.Alone;
        }

        internal RowView Setup(BlockView blockView, RowPresenter row)
        {
            Debug.Assert(blockView != null);
            Debug.Assert(row != null && row.View == null);

            var rowView = CachedList.GetOrCreate(ref _cachedRowViews, Template.CreateRowView);
            rowView.SetBlockView(blockView);
            rowView.Setup(row);
            return rowView;
        }

        internal void Cleanup(RowPresenter row)
        {
            var rowView = row.View;
            Debug.Assert(rowView != null);
            rowView.Cleanup();
            rowView.SetBlockView(null);
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

            var scalarBindings = Template.InternalScalarBindings;
            scalarBindings.BeginSetup();
            for (int i = 0; i < scalarBindings.Count; i++)
                InsertScalarElementsAfter(scalarBindings[i], Elements.Count - 1, 1);
            scalarBindings.EndSetup();
            HeadScalarElementsCount = Template.ScalarBindingsSplit;
            CoerceCurrentContainerView(null);
            RefreshElements(true);
        }

        private void RefreshElements(bool isReload)
        {
            if (Elements == null || Elements.Count == 0)
                return;

            RefreshScalarElements(isReload);
            RefreshContainerViews();

            _isDirty = false;
        }

        private void RefreshScalarElements(bool isReload)
        {
            var scalarBindings = Template.ScalarBindings;
            foreach (var scalarBinding in scalarBindings)
            {
                for (int i = 0; i < scalarBinding.BlockDimensions; i++)
                {
                    var element = scalarBinding[i];
                    scalarBinding.Refresh(element);
                }
            }
        }

        private void RefreshContainerViews()
        {
            if (CurrentContainerView != null && CurrentContainerViewPosition != CurrentContainerViewPosition.WithinList)
                CurrentContainerView.Refresh();
            foreach (var containerView in ContainerViewList)
                containerView.Refresh();
        }

        internal void ClearElements()
        {
            if (ElementCollection == null)
                return;

            ContainerViewList.VirtualizeAll();
            if (CurrentContainerView != null)
                ClearCurrentContainerView();

            var scalarBindings = Template.ScalarBindings;
            for (int i = 0; i < scalarBindings.Count; i++)
            {
                var scalarBinding = scalarBindings[i];
                scalarBinding.CumulativeBlockDimensionsDelta = 0;
                int count = scalarBinding.IsMultidimensional ? BlockDimensions : 1;
                RemoveScalarElementsAfter(scalarBinding, -1, count);
            }
            Debug.Assert(Elements.Count == 0);
            _blockDimensions = 1;
            ElementCollection = null;
        }

        private int InsertScalarElementsAfter(ScalarBinding scalarBinding, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = scalarBinding.Setup();
                ElementCollection.Insert(index + i + 1, element);
            }
            return index + count;
        }

        private void RemoveScalarElementsAfter(ScalarBinding scalarBinding, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = Elements[index + 1];
                Debug.Assert(element.GetBinding() == scalarBinding);
                scalarBinding.Cleanup(element);
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

            ContainerViewList.VirtualizeAll();

            var index = -1;
            var delta = 0;
            var scalarBindings = Template.InternalScalarBindings;
            if (blockDimensionsDelta > 0)
                scalarBindings.BeginSetup();
            for (int i = 0; i < scalarBindings.Count; i++)
            {
                index++;
                if (i == Template.ScalarBindingsSplit && CurrentContainerView != null)
                    index += 1;
                var scalarBinding = scalarBindings[i];

                var prevCumulativeBlockDimensionsDelta = i == 0 ? 0 : scalarBindings[i - 1].CumulativeBlockDimensionsDelta;
                if (!scalarBinding.IsMultidimensional)
                {
                    scalarBinding.CumulativeBlockDimensionsDelta = prevCumulativeBlockDimensionsDelta + (BlockDimensions - 1);
                    continue;
                }
                scalarBinding.CumulativeBlockDimensionsDelta = prevCumulativeBlockDimensionsDelta;

                if (i < Template.ScalarBindingsSplit)
                    delta += blockDimensionsDelta;

                if (blockDimensionsDelta > 0)
                    index = InsertScalarElementsAfter(scalarBinding, index, blockDimensionsDelta);
                else
                    RemoveScalarElementsAfter(scalarBinding, index, -blockDimensionsDelta);
            }

            if (blockDimensionsDelta > 0)
                scalarBindings.EndSetup();

            HeadScalarElementsCount += delta;

            if (CurrentContainerView != null)
                CurrentContainerView.Reload();
        }

        internal void OnFocused(RowView rowView)
        {
            if (rowView.RowPresenter == CurrentRow)
            {
                InvalidateElements();
                return;
            }

            var newCurrentContainerView = GetContainerView(rowView);
            if (newCurrentContainerView != CurrentContainerView)
            {
                Debug.Assert(CurrentContainerView != null);
                Debug.Assert(CurrentRow != rowView.RowPresenter);
                _currentContainerView = newCurrentContainerView;
                if (CurrentContainerViewPosition != CurrentContainerViewPosition.WithinList)
                {
                    CurrentContainerViewPosition = CurrentContainerViewPosition.WithinList;
                    ContainerViewList.IncreaseCount();
                }
            }
            CurrentRow = rowView.RowPresenter;
        }

        private ContainerView GetContainerView(RowView rowView)
        {
            if (Template.ContainerKind == ContainerKind.Row)
                return rowView;
            else
                return rowView.GetBlockView();
        }

        protected override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            if (ElementCollection != null)
            {
                ContainerViewList.VirtualizeAll();
                CoerceCurrentContainerView(oldValue);
                InvalidateElements();
            }
        }

        protected override void OnSelectedRowsChanged()
        {
            base.OnSelectedRowsChanged();
            InvalidateElements();
        }

        protected override void OnRowsChanged()
        {
            // when oldCurrentRow != CurrentRow, CurrentBlockView should have been reloaded in OnCurrentRowChanged override
            var oldCurrentRow = CurrentRow;
            base.OnRowsChanged();
            ContainerViewList.VirtualizeAll();
            if (CurrentContainerView != null && oldCurrentRow == CurrentRow)
                CurrentContainerView.ReloadIfInvalid();
        }

        protected override void OnRowUpdated(RowPresenter row)
        {
            base.OnRowUpdated(row);
            InvalidateElements();
        }

        protected override void OnIsEditingChanged()
        {
            base.OnIsEditingChanged();
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
                RefreshElements(false);
            else
            {
                panel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    RefreshElements(false);
                }));
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: [{1}]", CurrentContainerViewPosition, DebugWriteElementsString);
        }

        private string DebugWriteElementsString
        {
            get
            {
                if (CurrentContainerViewPosition == CurrentContainerViewPosition.None)
                    return string.Empty;

                if (CurrentContainerViewPosition == CurrentContainerViewPosition.Alone)
                    return GetDebugWriteString(CurrentContainerView);

                var result = GetContainerListDebugWriteString();
                if (CurrentContainerViewPosition == CurrentContainerViewPosition.BeforeList)
                    result = string.Join(", ", GetDebugWriteString(CurrentContainerView), result);
                else if (CurrentContainerViewPosition == CurrentContainerViewPosition.BeforeList)
                    result = string.Join(", ", result, GetDebugWriteString(CurrentContainerView));

                return result;
            }
        }

        private string GetDebugWriteString(ContainerView containerView)
        {
            var ordinal = containerView.ContainerOrdinal;
            return containerView == CurrentContainerView ? string.Format("({0})", ordinal) : ordinal.ToString();
        }

        private string GetContainerListDebugWriteString()
        {
            return string.Join(", ", ContainerViewList.Select(x => GetDebugWriteString(x)).ToArray());
        }
    }
}
