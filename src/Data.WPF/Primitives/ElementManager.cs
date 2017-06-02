using DevZest.Data;
using DevZest.Windows.Controls;
using DevZest.Windows.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace DevZest.Windows.Primitives
{
    internal abstract class ElementManager : RowManager
    {
        internal ElementManager(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy)
        {
            ContainerViewList = emptyContainerViewList ? Primitives.ContainerViewList.Empty : Primitives.ContainerViewList.Create(this);
        }

        List<ContainerView> _cachedContainerViews;
        List<RowView> _cachedRowViews;

        protected void ClearCachedElements()
        {
            if (_cachedContainerViews != null)
                _cachedContainerViews.Clear();
            if (_cachedRowViews != null)
                _cachedRowViews.Clear();
        }

        private static T GetOrCreate<T>(ref List<T> cachedList, Func<T> constructor)
            where T : class
        {
            Debug.Assert(constructor != null);

            if (cachedList == null || cachedList.Count == 0)
                return constructor();

            var last = cachedList.Count - 1;
            var result = cachedList[last];
            cachedList.RemoveAt(last);
            return result;
        }

        private static void Recycle<T>(ref List<T> cachedList, T value)
            where T : class
        {
            Debug.Assert(value != null);

            if (cachedList == null)
                cachedList = new List<T>();
            cachedList.Add(value);
        }

        private ContainerView Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter.View == null);
            return Setup(rowPresenter.Index / FlowCount);
        }

        private ContainerView Setup(int ordinal)
        {
            var result = GetOrCreate(ref _cachedContainerViews, Template.CreateContainerView);
            result.Setup(this, ordinal);
            return result;
        }

        private void Cleanup(ContainerView containerView)
        {
            Debug.Assert(containerView != null);
            containerView.Cleanup();
            Recycle(ref _cachedContainerViews, containerView);
        }

        public ContainerViewList ContainerViewList { get; private set; }

        public ContainerView CurrentContainerView { get; private set; }

        private void ResetCurrentContainerView(ContainerView value)
        {
            Debug.Assert(CurrentContainerViewPlacement == CurrentContainerViewPlacement.None || CurrentContainerViewPlacement == CurrentContainerViewPlacement.Alone);
            CurrentContainerView = value;
            CurrentContainerViewPlacement = value != null ? CurrentContainerViewPlacement.Alone : CurrentContainerViewPlacement.None;
        }

        private void CoerceCurrentContainerView(RowPresenter oldValue)
        {
            var newValue = CurrentRow;
            if (newValue != null)
            {
                if (CurrentContainerView == null)
                {
                    Debug.Assert(ContainerViewList.Count == 0);
                    ResetCurrentContainerView(Setup(newValue));
                    ElementCollection.Insert(HeadScalarElementsCount, CurrentContainerView);
                }
                else if (oldValue != newValue)
                {
                    if (ContainerViewList.Count == 0)
                        CurrentContainerView.ReloadCurrentRow(oldValue);
                    else
                        CoerceCurrentRowView(oldValue.View);
                }
            }
            else if (CurrentContainerView != null)
                ClearCurrentContainerView();
        }

        protected virtual void CoerceCurrentRowView(RowView oldValue)
        {
            Debug.Assert(ContainerViewList.Count > 0);
            Debug.Assert(oldValue != null);
            Debug.Assert(CurrentContainerView == GetContainerView(oldValue));

            var placement = GetContainerViewPlacement(CurrentRow);
            if (IsIsolated(CurrentContainerViewPlacement))
            {
                if (IsIsolated(placement))
                    CurrentContainerView.ReloadCurrentRow(oldValue.RowPresenter);
                else
                {
                    var newValue = CurrentRow.View;
                    Swap(oldValue, newValue);
                    var removalIndex = HeadScalarElementsCount;
                    if (CurrentContainerViewPlacement == CurrentContainerViewPlacement.AfterList)
                        removalIndex += ContainerViewList.Count;
                    Cleanup((ContainerView)Elements[removalIndex]);
                    ElementCollection.RemoveAt(removalIndex);
                }
            }
            else
            {
                if (IsIsolated(placement))
                {
                    Debug.Assert(CurrentRow.View == null);
                    var containerView = Setup(CurrentRow);
                    Debug.Assert(CurrentRow.View != null);
                    var insertIndex = HeadScalarElementsCount;
                    if (placement == CurrentContainerViewPlacement.AfterList)
                        insertIndex += ContainerViewList.Count;
                    ElementCollection.Insert(insertIndex, containerView);
                }
                Swap(oldValue, CurrentRow.View);
            }

            CurrentContainerView = GetContainerView(CurrentRow.View);
            CurrentContainerViewPlacement = placement;
        }

        private CurrentContainerViewPlacement GetContainerViewPlacement(RowPresenter row)
        {
            Debug.Assert(ContainerViewList.Count > 0);
            var containerOrdinal = row.Index / FlowCount;
            if (containerOrdinal < ContainerViewList.First.ContainerOrdinal)
                return CurrentContainerViewPlacement.BeforeList;
            else if (containerOrdinal > ContainerViewList.Last.ContainerOrdinal)
                return CurrentContainerViewPlacement.AfterList;
            else
                return CurrentContainerViewPlacement.WithinList;
        }

        private void Swap(RowView oldValue, RowView newValue)
        {
            var oldRowPresenter = oldValue.RowPresenter;
            var newRowPresenter = newValue.RowPresenter;
            var oldBlockView = oldValue.GetBlockView();
            var newBlockView = newValue.GetBlockView();

            int oldIndex, newIndex;
            var oldCollection = GetPosition(oldValue, out oldIndex);
            var newCollection = GetPosition(newValue, out newIndex);

            if (oldCollection == newCollection && oldIndex < newIndex)
            {
                newCollection.RemoveAt(newIndex);
                oldCollection.RemoveAt(oldIndex);
            }
            else
            {
                oldCollection.RemoveAt(oldIndex);
                newCollection.RemoveAt(newIndex);
            }
            oldValue.Reload(newRowPresenter);
            newValue.Reload(oldRowPresenter);
            newValue.SetBlockView(oldBlockView);
            oldValue.SetBlockView(newBlockView);
            if (oldCollection == newCollection && newIndex < oldIndex)
            {
                newCollection.Insert(newIndex, oldValue);
                oldCollection.Insert(oldIndex, newValue);
            }
            else
            {
                oldCollection.Insert(oldIndex, newValue);
                newCollection.Insert(newIndex, oldValue);
            }
        }

        private IElementCollection GetPosition(RowView rowView, out int index)
        {
            if (Template.ContainerKind == ContainerKind.Row)
            {
                var placement = GetContainerViewPlacement(rowView.RowPresenter);
                if (placement == CurrentContainerViewPlacement.WithinList)
                    index = ContainerViewListStartIndex + GetContainerView(rowView).ContainerOrdinal - ContainerViewList.First.ContainerOrdinal;
                else if (placement == CurrentContainerViewPlacement.BeforeList)
                    index = HeadScalarElementsCount;
                else
                {
                    Debug.Assert(placement == CurrentContainerViewPlacement.AfterList);
                    index = ContainerViewListStartIndex + ContainerViewList.Count;
                }
                return ElementCollection;
            }
            else
            {
                var blockView = rowView.GetBlockView();
                index = blockView.BlockBindingsSplit + (rowView.RowPresenter.Index % FlowCount);
                return blockView.ElementCollection;
            }
        }

        private void ClearCurrentContainerView()
        {
            Debug.Assert(CurrentContainerView != null);
            Cleanup(CurrentContainerView);
            ElementCollection.RemoveAt(HeadScalarElementsCount);
            ResetCurrentContainerView(null);
        }

        public CurrentContainerViewPlacement CurrentContainerViewPlacement { get; private set; }

        internal bool IsCurrentContainerViewIsolated
        {
            get { return IsIsolated(CurrentContainerViewPlacement); }
        }

        private static bool IsIsolated(CurrentContainerViewPlacement placement)
        {
            return placement == CurrentContainerViewPlacement.Alone
                || placement == CurrentContainerViewPlacement.BeforeList
                || placement == CurrentContainerViewPlacement.AfterList;
        }

        protected ContainerView this[RowView rowView]
        {
            get { return this[rowView.ContainerOrdinal]; }
        }

        public ContainerView this[int blockOrdinal]
        {
            get
            {
                if (CurrentContainerView != null && blockOrdinal == CurrentContainerView.ContainerOrdinal)
                    return CurrentContainerView;
                var index = ContainerViewList.IndexOf(blockOrdinal);
                return index == -1 ? null : ContainerViewList[index];
            }
        }

        internal void Realize(int ordinal)
        {
            CurrentContainerViewPlacement = DoRealize(ordinal);
        }

        private CurrentContainerViewPlacement DoRealize(int ordinal)
        {
            Debug.Assert(CurrentContainerView != null);

            if (CurrentContainerView.ContainerOrdinal == ordinal)
                return CurrentContainerViewPlacement.WithinList;

            var index = HeadScalarElementsCount;
            switch (CurrentContainerViewPlacement)
            {
                case CurrentContainerViewPlacement.Alone:
                    if (ordinal > CurrentContainerView.ContainerOrdinal)
                        index++;
                    break;
                case CurrentContainerViewPlacement.BeforeList:
                    index++;
                    if (ordinal > ContainerViewList.Last.ContainerOrdinal)
                        index += ContainerViewList.Count;
                    break;
                case CurrentContainerViewPlacement.WithinList:
                case CurrentContainerViewPlacement.AfterList:
                    if (ordinal > ContainerViewList.Last.ContainerOrdinal)
                        index += ContainerViewList.Count;
                    break;
            }

            var containerView = Setup(ordinal);
            ElementCollection.Insert(index, containerView);
            if (CurrentContainerViewPlacement == CurrentContainerViewPlacement.Alone)
                return ordinal > CurrentContainerView.ContainerOrdinal ? CurrentContainerViewPlacement.BeforeList : CurrentContainerViewPlacement.AfterList;
            return CurrentContainerViewPlacement;
        }

        internal int ContainerViewListStartIndex
        {
            get
            {
                var result = HeadScalarElementsCount;
                if (CurrentContainerViewPlacement == CurrentContainerViewPlacement.BeforeList)
                    result++;
                return result;
            }
        }

        internal virtual void VirtualizeAll()
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

            CurrentContainerViewPlacement = CurrentContainerViewPlacement.Alone;
        }

        internal virtual void VirtualizeFirst()
        {
            var containerView = ContainerViewList.First;
            if (containerView == CurrentContainerView)
                CurrentContainerViewPlacement = CurrentContainerViewPlacement.BeforeList;
            else
            {
                Cleanup(containerView);
                ElementCollection.RemoveAt(ContainerViewListStartIndex);
            }
        }

        internal virtual void VirtualizeLast()
        {
            var startIndex = ContainerViewListStartIndex;
            var containerView = ContainerViewList.Last;
            if (containerView == CurrentContainerView)
                CurrentContainerViewPlacement = CurrentContainerViewPlacement.AfterList;
            else
            {
                Cleanup(containerView);
                ElementCollection.RemoveAt(ContainerViewListStartIndex + ContainerViewList.Count - 1);
            }
        }

        internal RowView Setup(BlockView blockView, RowPresenter row)
        {
            Debug.Assert(blockView != null);
            Debug.Assert(row != null && row.View == null);

            var rowView = GetOrCreate(ref _cachedRowViews, Template.CreateRowView);
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
            Recycle(ref _cachedRowViews, rowView);
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
            BeginSetup(scalarBindings);
            for (int i = 0; i < scalarBindings.Count; i++)
                InsertScalarElementsAfter(scalarBindings[i], Elements.Count - 1, 1);
            scalarBindings.EndSetup();
            HeadScalarElementsCount = Template.ScalarBindingsSplit;
            CoerceCurrentContainerView(null);
            RefreshView();
        }

        private void BeginSetup(IReadOnlyList<ScalarBinding> scalarBindings)
        {
            for (int i = 0; i < scalarBindings.Count; i++)
                scalarBindings[i].BeginSetup(0);
        }

        internal virtual DataPresenter DataPresenter
        {
            get { return null; }
        }

        private void RefreshView()
        {
            if (Elements == null || Elements.Count == 0)
                return;

            DataPresenter?.OnViewRefreshing();

            RefreshScalarElements();
            RefreshContainerViews();

            _isDirty = false;

            DataPresenter?.OnViewRefreshed();
        }

        private void RefreshScalarElements()
        {
            var scalarBindings = Template.ScalarBindings;
            foreach (var scalarBinding in scalarBindings)
            {
                for (int i = 0; i < scalarBinding.FlowCount; i++)
                {
                    var element = scalarBinding[i];
                    scalarBinding.Refresh(element);
                }
            }
        }

        private void RefreshContainerViews()
        {
            if (CurrentContainerView != null && CurrentContainerViewPlacement != CurrentContainerViewPlacement.WithinList)
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
                scalarBinding.CumulativeFlowCountDelta = 0;
                int count = scalarBinding.Flowable ? FlowCount : 1;
                RemoveScalarElementsAfter(scalarBinding, -1, count);
            }
            Debug.Assert(Elements.Count == 0);
            _flowCount = 1;
            ElementCollection = null;
        }

        private int InsertScalarElementsAfter(ScalarBinding scalarBinding, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = scalarBinding.Setup(scalarBinding.FlowCount - count + i);
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

        private int _flowCount = 1;
        internal int FlowCount
        {
            get { return _flowCount; }
            set
            {
                Debug.Assert(value >= 1);

                if (_flowCount == value)
                    return;

                var delta = value - _flowCount;
                _flowCount = value;
                OnFlowCountChanged(delta);
            }
        }

        private void OnFlowCountChanged(int flowCountDelta)
        {
            Debug.Assert(flowCountDelta != 0);

            ContainerViewList.VirtualizeAll();

            var index = -1;
            var delta = 0;
            var scalarBindings = Template.InternalScalarBindings;
            if (flowCountDelta > 0)
                BeginSetup(scalarBindings, flowCountDelta);
            for (int i = 0; i < scalarBindings.Count; i++)
            {
                index++;
                if (i == Template.ScalarBindingsSplit && CurrentContainerView != null)
                    index += 1;
                var scalarBinding = scalarBindings[i];

                var prevCumulativeFlowCountDelta = i == 0 ? 0 : scalarBindings[i - 1].CumulativeFlowCountDelta;
                if (!scalarBinding.Flowable)
                {
                    scalarBinding.CumulativeFlowCountDelta = prevCumulativeFlowCountDelta + (FlowCount - 1);
                    continue;
                }
                scalarBinding.CumulativeFlowCountDelta = prevCumulativeFlowCountDelta;

                if (i < Template.ScalarBindingsSplit)
                    delta += flowCountDelta;

                if (flowCountDelta > 0)
                    index = InsertScalarElementsAfter(scalarBinding, index + FlowCount - flowCountDelta - 1, flowCountDelta);
                else
                    RemoveScalarElementsAfter(scalarBinding, index += FlowCount - 1, -flowCountDelta);
            }

            if (flowCountDelta > 0)
                scalarBindings.EndSetup();

            HeadScalarElementsCount += delta;

            if (CurrentContainerView != null)
                CurrentContainerView.ReloadCurrentRow(CurrentRow);
        }

        private void BeginSetup(IReadOnlyList<ScalarBinding> scalarBindings, int flowCountDelta)
        {
            Debug.Assert(flowCountDelta > 0);
            for (int i = 0; i < scalarBindings.Count; i++)
            {
                var scalarBinding = scalarBindings[i];
                int startOffset = scalarBinding.Flowable ? FlowCount - flowCountDelta : 1;
                scalarBinding.BeginSetup(startOffset);
            }
        }

        internal void OnFocused(RowView rowView)
        {
            if (rowView.RowPresenter != CurrentRow)
            {
                if (!CanChangeCurrentRow)
                    PreventCurrentRowViewFromLosingFocus(rowView);
                else
                    SetCurrentRowFromView(rowView);
            }
            InvalidateView();
        }

        private void PreventCurrentRowViewFromLosingFocus(RowView newFocusedRowView)
        {
            // Focus management is tricky, we choose not to manage focus at all:
            // instead of setting focus back to current RowView, we reload CurrentRow
            // to the newly focused RowView.
            var oldValue = newFocusedRowView.RowPresenter;
            UpdateCurrentContainerView(newFocusedRowView);
            ContainerViewList.VirtualizeAll();
            CurrentContainerView.ReloadCurrentRow(oldValue);
        }

        private ContainerView GetContainerView(RowView rowView)
        {
            if (Template.ContainerKind == ContainerKind.Row)
                return rowView;
            else
                return rowView.GetBlockView();
        }

        private void SetCurrentRowFromView(RowView rowView)
        {
            UpdateCurrentContainerView(rowView);
            _currentRowFromView = true;
            CurrentRow = rowView.RowPresenter;
            _currentRowFromView = false;
        }

        private void UpdateCurrentContainerView(RowView rowView)
        {
            if (CurrentContainerViewPlacement != CurrentContainerViewPlacement.WithinList)
            {
                Cleanup(CurrentContainerView);
                if (CurrentContainerViewPlacement == CurrentContainerViewPlacement.BeforeList)
                    ElementCollection.RemoveAt(HeadScalarElementsCount);
                else
                {
                    Debug.Assert(CurrentContainerViewPlacement == CurrentContainerViewPlacement.AfterList);
                    ElementCollection.RemoveAt(HeadScalarElementsCount + ContainerViewList.Count);
                }
                CurrentContainerViewPlacement = CurrentContainerViewPlacement.WithinList;
            }
            CurrentContainerView = GetContainerView(rowView);
        }

        private bool _currentRowFromView;
        protected override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            if (ElementCollection != null && !_currentRowFromView)
                CoerceCurrentContainerView(oldValue);
        }

        protected override void OnSelectedRowsChanged()
        {
            base.OnSelectedRowsChanged();
            InvalidateView();
        }

        protected override void OnRowsChanged()
        {
            // when oldCurrentRow != CurrentRow, CurrentContainerView should have been reloaded in OnCurrentRowChanged override
            var oldCurrentRow = CurrentRow;
            base.OnRowsChanged();
            ContainerViewList.VirtualizeAll();
            if (CurrentContainerView != null && oldCurrentRow == CurrentRow && CurrentContainerView.AffectedOnRowsChanged)
                CurrentContainerView.ReloadCurrentRow(CurrentRow);
        }

        protected sealed override void OnRowUpdated(RowPresenter row)
        {
            base.OnRowUpdated(row);
            InvalidateView();
        }

        protected override void OnIsEditingChanged()
        {
            base.OnIsEditingChanged();
            InvalidateView();
        }

        private bool _isDirty;
        public void InvalidateView()
        {
            if (_isDirty || ElementCollection == null)
                return;

            _isDirty = true;
            BeginRefreshView();
        }

        private void BeginRefreshView()
        {
            Debug.Assert(ElementCollection != null && _isDirty);

            var panel = ElementCollection.Parent;
            if (panel == null)
                RefreshView();
            else
            {
                panel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    RefreshView();
                }));
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: [{1}]", CurrentContainerViewPlacement, DebugWriteElementsString);
        }

        private string DebugWriteElementsString
        {
            get
            {
                if (CurrentContainerViewPlacement == CurrentContainerViewPlacement.None)
                    return string.Empty;

                if (CurrentContainerViewPlacement == CurrentContainerViewPlacement.Alone)
                    return GetDebugWriteString(CurrentContainerView);

                var result = GetContainerListDebugWriteString();
                if (CurrentContainerViewPlacement == CurrentContainerViewPlacement.BeforeList)
                    result = string.Join(", ", GetDebugWriteString(CurrentContainerView), result);
                else if (CurrentContainerViewPlacement == CurrentContainerViewPlacement.AfterList)
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
