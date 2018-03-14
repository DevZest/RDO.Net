using DevZest.Data;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class ElementManager : RowManager
    {
        internal ElementManager(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy)
        {
            ContainerViewList = emptyContainerViewList ? Primitives.ContainerViewList.Empty : Primitives.ContainerViewList.Create(this);
        }

        protected override void Reload()
        {
            base.Reload();
            var elementCollection = ElementCollection;
            ClearElements();
            ElementCollection = elementCollection;
            if (ElementCollection != null)
                InitializeElements();
            if (ContainerViewList != Primitives.ContainerViewList.Empty)
                ContainerViewList = Primitives.ContainerViewList.Create(this);
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
            return Setup(rowPresenter.Index / FlowRepeatCount);
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
                    if (ContainerViewList.Count == 0 || oldValue.IsDisposed)
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
            var containerOrdinal = row.Index / FlowRepeatCount;
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
                index = blockView.BlockBindingsSplit + (rowView.RowPresenter.Index % FlowRepeatCount);
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
            InitializeElements();
        }

        private void InitializeElements()
        {
            var scalarBindings = Template.InternalScalarBindings;
            BeginSetup(scalarBindings, 0);
            for (int i = 0; i < scalarBindings.Count; i++)
                InsertScalarElementsAfter(scalarBindings[i], Elements.Count - 1, 1);
            EndSetup(scalarBindings);
            HeadScalarElementsCount = Template.ScalarBindingsSplit;
            CoerceCurrentContainerView(null);
            RefreshView();
        }

        private static void BeginSetup(IReadOnlyList<ScalarBinding> scalarBindings, int startOffset)
        {
            for (int i = 0; i < scalarBindings.Count; i++)
            {
                var scalarBinding = scalarBindings[i];
                if (scalarBinding.FlowRepeatable)
                    scalarBinding.BeginSetup(startOffset, null);
                else if (startOffset == 0)
                    scalarBinding.BeginSetup(null);
            }
        }

        private static void EndSetup(IReadOnlyList<ScalarBinding> scalarBindings)
        {
            for (int i = 0; i < scalarBindings.Count; i++)
                scalarBindings[i].EndSetup();
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

            DataPresenter?.View?.RefreshScalarValidation();

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
                for (int i = 0; i < scalarBinding.FlowRepeatCount; i++)
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
                scalarBinding.CumulativeFlowRepeatCountDelta = 0;
                int count = scalarBinding.FlowRepeatable ? FlowRepeatCount : 1;
                RemoveScalarElementsAfter(scalarBinding, -1, count);
            }
            Debug.Assert(Elements.Count == 0);
            _flowRepeatCount = 1;
            ElementCollection = null;
        }

        private int InsertScalarElementsAfter(ScalarBinding scalarBinding, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = scalarBinding.Setup(scalarBinding.FlowRepeatCount - count + i);
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

        private int _flowRepeatCount = 1;
        internal int FlowRepeatCount
        {
            get { return _flowRepeatCount; }
            set
            {
                Debug.Assert(value >= 1);

                if (_flowRepeatCount == value)
                    return;

                var delta = value - _flowRepeatCount;
                _flowRepeatCount = value;
                OnFlowRepeatCountChanged(delta);
            }
        }

        private void OnFlowRepeatCountChanged(int flowRepeatCountDelta)
        {
            Debug.Assert(flowRepeatCountDelta != 0);

            ContainerViewList.VirtualizeAll();

            var index = -1;
            var delta = 0;
            var scalarBindings = Template.InternalScalarBindings;
            if (flowRepeatCountDelta > 0)
            {
                Debug.Assert(flowRepeatCountDelta < FlowRepeatCount);
                BeginSetup(scalarBindings, FlowRepeatCount - flowRepeatCountDelta);
            }
            for (int i = 0; i < scalarBindings.Count; i++)
            {
                index++;
                if (i == Template.ScalarBindingsSplit && CurrentContainerView != null)
                    index += 1;
                var scalarBinding = scalarBindings[i];

                var prevCumulativeFlowRepeatCountDelta = i == 0 ? 0 : scalarBindings[i - 1].CumulativeFlowRepeatCountDelta;
                if (!scalarBinding.FlowRepeatable)
                {
                    scalarBinding.CumulativeFlowRepeatCountDelta = prevCumulativeFlowRepeatCountDelta + (FlowRepeatCount - 1);
                    continue;
                }
                scalarBinding.CumulativeFlowRepeatCountDelta = prevCumulativeFlowRepeatCountDelta;

                if (i < Template.ScalarBindingsSplit)
                    delta += flowRepeatCountDelta;

                if (flowRepeatCountDelta > 0)
                    index = InsertScalarElementsAfter(scalarBinding, index + FlowRepeatCount - flowRepeatCountDelta - 1, flowRepeatCountDelta);
                else
                    RemoveScalarElementsAfter(scalarBinding, index += FlowRepeatCount - 1, -flowRepeatCountDelta);
            }

            if (flowRepeatCountDelta > 0)
                EndSetup(scalarBindings);

            HeadScalarElementsCount += delta;

            if (CurrentContainerView != null)
                CurrentContainerView.ReloadCurrentRow(CurrentRow);
        }

        /// <remarks>
        /// <see cref="_focusTo" /> is set in <see cref="OnFocused(RowView)"/> method, then is cleared to null in <see cref="SetCurrentRowFromView"/> method.
        /// NOTE: Besides the direct calling chain in <see cref="OnFocused(RowView)"/>, there is another calling chain to <see cref="SetCurrentRowFromView"/>:
        /// <see cref="OnFocused(RowView)"/> -->
        /// <see cref="CanChangeCurrentRow"/> -->
        /// <see cref="RowManager.EndEdit"/> -->
        /// <see cref="RowManager.CommitEdit"/> -->
        /// <see cref="RowManager.EditHandler.EndEdit(RowManager)"/> -->
        /// <see cref="RowManager.EditHandler.InsertHandler.CommitEdit(RowManager)"/> -->
        /// <see cref="DataSet.EndAdd"/> -->
        /// ... -->
        /// <see cref="OnRowsChanged"/>
        /// ALSO NOTE: During this calling chain, <see cref="CurrentRow"/> is directly set to <see cref="_focusTo"/>, by using the flag <see cref="_currentRowChangedByInsertSuspended"/>.
        /// In this case, <see cref="CurrentRow"/> does not always be the same of the currently editing <see cref="RowPresenter"/>,
        /// use the return value of <see cref="RowManager.CommitEdit"/> instead.
        /// </remarks>
        private RowView _focusTo;
        private bool _currentRowChangedByInsertSuspended;
        internal void OnFocused(RowView rowView)
        {
            if (rowView.RowPresenter != CurrentRow)
            {
                SuspendInvalidateView();
                _currentRowChangedByInsertSuspended = true;
                _focusTo = rowView;
                if (!CanChangeCurrentRow)
                    PreventCurrentRowViewFromLosingFocus(rowView);
                else if (_focusTo != null)  // _focusTo can be null from OnRowsChanged.
                    SetCurrentRowFromView();
                _currentRowChangedByInsertSuspended = false;
                ResumeInvalidateView();
            }
        }

        private bool CanChangeCurrentRow
        {
            get { return IsEditing ? EndEdit() : true; }
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

        private void SetCurrentRowFromView()
        {
            Debug.Assert(_focusTo != null);
            UpdateCurrentContainerView(_focusTo);
            CurrentRow = _focusTo.RowPresenter;
            _focusTo = null;
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

        protected override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            if (ElementCollection != null && _focusTo == null)
                CoerceCurrentContainerView(oldValue);
        }

        protected override void OnRowsChanged()
        {
            if (_focusTo != null)
                SetCurrentRowFromView();

            // when oldCurrentRow != CurrentRow, CurrentContainerView should have been reloaded in OnCurrentRowChanged override
            var oldCurrentRow = CurrentRow;
            ContainerViewList.VirtualizeAll(); // must VirtualizeAll before calling base.OnRowChanged where CurrentRow might be changed.
            base.OnRowsChanged();
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
        public sealed override void InvalidateView()
        {
            if (_isViewInvalid)
                return;

            if (_suspendInvalidateViewCount > 0)
            {
                _isViewInvalid = true;
                return;
            }
            _isViewInvalid = true;
            PerformInvalidateView();
        }

        private void PerformInvalidateView()
        {
            DataPresenter?.OnViewInvalidating();
            _isViewInvalid = false;
            DataPresenter?.OnViewInvalidated();

            if (_isDirty || ElementCollection == null)
                return;

            _isDirty = true;
            BeginRefreshView();
        }

        private int _suspendInvalidateViewCount;
        private bool _isViewInvalid;
        public void SuspendInvalidateView()
        {
            _suspendInvalidateViewCount++;
        }

        public void ResumeInvalidateView()
        {
            Debug.Assert(_suspendInvalidateViewCount > 0);
            _suspendInvalidateViewCount--;
            if (_suspendInvalidateViewCount == 0 && _isViewInvalid)
                PerformInvalidateView();
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

        protected sealed override bool CurrentRowChangeSuspended
        {
            get { return _currentRowChangedByInsertSuspended; }
        }

        public sealed override RowPresenter CurrentRow
        {
            get { return base.CurrentRow; }
            set
            {
                SuspendInvalidateView();
                base.CurrentRow = value;
                ResumeInvalidateView();
            }
        }
    }
}
