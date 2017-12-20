﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class RowManager : RowNormalizer, IReadOnlyList<RowPresenter>
    {
        private abstract class EditHandler
        {
            public static void EnterEditMode(RowManager rowManager)
            {
                GetCurrentRowEditHandler(rowManager).BeginEdit(rowManager);
            }

            private static EditHandler GetCurrentRowEditHandler(RowManager rowManager)
            {
                var currentRow = rowManager.CurrentRow;
                if (currentRow.IsVirtual)
                {
                    var virtualRowPlacement = rowManager.VirtualRowPlacement;
                    if (virtualRowPlacement == VirtualRowPlacement.Head)
                        return InsertHandler.Before(rowManager, null);
                    else
                    {
                        Debug.Assert(virtualRowPlacement == VirtualRowPlacement.Tail);
                        return InsertHandler.After(rowManager, null);
                    }
                }
                else
                    return EditCurrentHandler.Singleton;
            }

            public static void BeginInsertBefore(RowManager rowManager, RowPresenter parent, RowPresenter child)
            {
                GetInsertBeforeHandler(rowManager, parent, child).BeginEdit(rowManager);
            }

            private static EditHandler GetInsertBeforeHandler(RowManager rowManager, RowPresenter parent, RowPresenter child)
            {
                return parent == null ? InsertHandler.Before(rowManager, child) : InsertChildHandler.Before(parent, child);
            }

            public static void BeginInsertAfter(RowManager rowManager, RowPresenter parent, RowPresenter child)
            {
                GetInsertAfterHandler(rowManager, parent, child).BeginEdit(rowManager);
            }

            private static EditHandler GetInsertAfterHandler(RowManager rowManager, RowPresenter parent, RowPresenter child)
            {
                return parent == null ? InsertHandler.After(rowManager, child) : InsertChildHandler.After(parent, child);
            }

            protected EditHandler()
            {
            }

            public abstract void OnRowsChanged(RowManager rowManager);

            public abstract RowPresenter InsertingRow { get; }

            protected void BeginEdit(RowManager rowManager)
            {
                Debug.Assert(!rowManager.IsEditing);

                OpenEdit(rowManager);
                if (rowManager.Editing == null)
                    rowManager.Editing = this;
            }

            public void CancelEdit(RowManager rowManager)
            {
                rowManager.Editing = null;
                RollbackEdit(rowManager);
            }

            public void EndEdit(RowManager rowManager)
            {
                rowManager.Editing = null;
                CommitEdit(rowManager);
            }

            protected abstract void OpenEdit(RowManager rowManager);

            protected abstract void RollbackEdit(RowManager rowManager);

            protected abstract void CommitEdit(RowManager rowManager);

            private sealed class EditCurrentHandler : EditHandler
            {
                public static readonly EditCurrentHandler Singleton = new EditCurrentHandler();

                private EditCurrentHandler()
                {
                }

                public override RowPresenter InsertingRow
                {
                    get { return null; }
                }

                protected override void OpenEdit(RowManager rowManager)
                {
                    rowManager.CurrentRow.DataRow.BeginEdit();
                }

                protected override void RollbackEdit(RowManager rowManager)
                {
                    rowManager.CurrentRow.DataRow.CancelEdit();
                }

                protected override void CommitEdit(RowManager rowManager)
                {
                    rowManager.CurrentRow.DataRow.EndEdit();
                }

                public override void OnRowsChanged(RowManager rowManager)
                {
                    if (rowManager.CurrentRow.IsDisposed)
                        CancelEdit(rowManager);
                }
            }

            private abstract class InsertHandler : EditHandler
            {
                public static InsertHandler Before(RowManager rowManager, RowPresenter reference)
                {
                    return new InsertBeforeHandler(rowManager, reference);
                }

                public static InsertHandler After(RowManager rowManager, RowPresenter reference)
                {
                    return new InsertAfterHandler(rowManager, reference);
                }

                protected InsertHandler(RowManager rowManager, RowPresenter reference)
                {
                    Debug.Assert(rowManager != null);
                    Debug.Assert(reference == null || reference.RowManager == rowManager);
                    RowManager = rowManager;
                    Reference = reference;
                }

                protected RowManager RowManager { get; private set; }

                private RowPresenter _reference;
                protected RowPresenter Reference
                {
                    get { return _reference; }
                    set
                    {
                        Debug.Assert(value == null || !value.IsVirtual);
                        _reference = value;
                        RefreshReferenceRawIndex();
                    }
                }

                protected int ReferenceRawIndex { get; private set; }

                private void RefreshReferenceRawIndex()
                {
                    ReferenceRawIndex = _reference == null ? -1 : _reference.RawIndex;
                }

                private RowPresenter _insertingRow;
                public sealed override RowPresenter InsertingRow
                {
                    get { return _insertingRow; }
                }

                protected virtual DataSet DataSet
                {
                    get { return RowManager.DataSet; }
                }

                protected sealed override void RollbackEdit(RowManager rowManager)
                {
                    Debug.Assert(!rowManager.IsEditing);
                    Debug.Assert(rowManager == RowManager);

                    DataSet.CancelAdd();
                    DisposeInsertingRow(true);
                }

                protected abstract int CommitEditIndex { get; }

                protected sealed override void CommitEdit(RowManager rowManager)
                {
                    Debug.Assert(!rowManager.IsEditing);
                    Debug.Assert(rowManager == RowManager);

                    DisposeInsertingRow(false);
                    var newDataRow = DataSet.EndAdd(CommitEditIndex);
                    var newCurrentRow = rowManager[newDataRow];
                    if (!rowManager.CurrentRowChangeSuspended)
                    {
                        if (newCurrentRow != null)
                            rowManager.CurrentRow = newCurrentRow;
                    }
                }

                protected virtual RowPresenter CurrentRowAfterRollback
                {
                    get { return Reference ?? FirstOrLastOf(RowManager.Rows); }
                }

                protected enum InsertMode
                {
                    Before,
                    After
                }

                protected abstract InsertMode Mode { get; }

                protected RowPresenter FirstOrLastOf(IReadOnlyList<RowPresenter> rows)
                {
                    return rows.Count == 0 ? null : rows[Mode == InsertMode.Before ? 0 : rows.Count - 1];
                }

                private void DisposeInsertingRow(bool isRollback)
                {
                    _insertingRow.DataRow = null;
                    _insertingRow.Dispose();
                    if (isRollback)
                    {
                        RowManager._suggestedCurrentRow = CurrentRowAfterRollback;
                        RowManager.OnRowsChanged();
                    }
                }

                protected sealed override void OpenEdit(RowManager rowManager)
                {
                    Debug.Assert(rowManager == RowManager);

                    if (rowManager.VirtualRow != null)
                    {
                        _insertingRow = rowManager.VirtualRow;
                        _insertingRow.DataRow = DataSet.BeginAdd();
                    }
                    else
                        _insertingRow = new RowPresenter(rowManager, DataSet.BeginAdd());
                    _insertingRow.RawIndex = InsertingRowRawIndex;

                    if (rowManager.VirtualRowPlacement == VirtualRowPlacement.Head)
                        rowManager.VirtualRow = new RowPresenter(rowManager, 0);
                    else if (rowManager.VirtualRowPlacement == VirtualRowPlacement.Tail)
                        rowManager.VirtualRow = new RowPresenter(rowManager, -1);
                    else if (rowManager.VirtualRow != null)
                        rowManager.VirtualRow = null;
                    rowManager.Editing = this;
                    rowManager.OnRowsChanged();
                    rowManager.SetCurrentRow(_insertingRow, false);
                }

                public override void OnRowsChanged(RowManager rowManager)
                {
                    if (Reference != null)
                    {
                        if (Reference.IsDisposed)
                            Reference = ReferenceForDisposed;
                        else
                            RefreshReferenceRawIndex();
                    }
                    if (rowManager.VirtualRowPlacement == VirtualRowPlacement.Tail)
                        rowManager.VirtualRow.RawIndex = rowManager.Rows.Count - 1;
                    if (InsertingRow != null)
                        InsertingRow.RawIndex = InsertingRowRawIndex;
                }

                protected virtual RowPresenter ReferenceForDisposed
                {
                    get
                    {
                        Debug.Assert(Reference.IsDisposed);
                        var baseRows = RowManager.BaseRows;
                        var rawIndex = Math.Min(ReferenceRawIndex, baseRows.Count - 1);
                        return rawIndex >= 0 ? baseRows[rawIndex] : null;
                    }
                }

                protected abstract int InsertingRowRawIndex { get; }

                private sealed class InsertBeforeHandler : InsertHandler
                {
                    public InsertBeforeHandler(RowManager rowManager, RowPresenter reference)
                        : base(rowManager, reference)
                    {
                    }

                    protected override int InsertingRowRawIndex
                    {
                        get
                        {
                            if (Reference == null)
                                return RowManager.VirtualRowPlacement == VirtualRowPlacement.Head ? 1 : 0;
                            else if (RowManager.IsEditing)
                                return Reference.Index - 1;
                            else
                                return Reference.Index;
                        }
                    }

                    protected override int CommitEditIndex
                    {
                        get { return Reference == null ? 0 : Reference.DataRow.Index; }
                    }

                    protected override InsertMode Mode
                    {
                        get { return InsertMode.Before; }
                    }
                }

                private sealed class InsertAfterHandler : InsertHandler
                {
                    public InsertAfterHandler(RowManager rowManager, RowPresenter reference)
                        : base(rowManager, reference)
                    {
                    }

                    protected override InsertMode Mode
                    {
                        get { return InsertMode.After; }
                    }

                    protected override int InsertingRowRawIndex
                    {
                        get
                        {
                            return Reference == null
                                ? (RowManager.VirtualRowPlacement == VirtualRowPlacement.Tail ? RowManager.Rows.Count - 2 : RowManager.Rows.Count - 1)
                                : Reference.Index + 1;
                        }
                    }

                    protected override int CommitEditIndex
                    {
                        get { return Reference == null ? DataSet.Count : Reference.DataRow.Index + 1; }
                    }
                }
            }

            private abstract class InsertChildHandler : InsertHandler
            {
                public static InsertChildHandler Before(RowPresenter parentRow, RowPresenter reference)
                {
                    return new InsertBeforeChildHandler(parentRow, reference);
                }

                public static InsertChildHandler After(RowPresenter parentRow, RowPresenter reference)
                {
                    return new InsertAfterChildHandler(parentRow, reference);
                }

                protected InsertChildHandler(RowPresenter parentRow, RowPresenter reference)
                    : base(parentRow.RowManager, reference)
                {
                    Debug.Assert(parentRow != null && !parentRow.IsVirtual);
                    Debug.Assert(reference == null || reference.Parent == parentRow);
                    ParentRow = parentRow;
                }

                private RowPresenter _parentRow;
                public RowPresenter ParentRow
                {
                    get { return _parentRow; }
                    private set
                    {
                        Debug.Assert(_parentRow == null && value != null && !value.IsVirtual);
                        _parentRow = value;
                        RefreshParentRowRawIndex();
                    }
                }

                public int ParentRowRawIndex { get; private set; } = -1;
                private void RefreshParentRowRawIndex()
                {
                    Debug.Assert(ParentRow != null);
                    ParentRowRawIndex = ParentRow.RawIndex;
                }

                protected override DataSet DataSet
                {
                    get { return ParentRow.DataRow[RowManager.Template.RecursiveModelOrdinal]; }
                }

                public override void OnRowsChanged(RowManager rowManager)
                {
                    if (ParentRow.IsDisposed)
                    {
                        CancelEdit(rowManager);
                        base.OnRowsChanged(rowManager);
                    }
                    else
                    {
                        base.OnRowsChanged(rowManager);
                        RefreshParentRowRawIndex();
                    }
                }

                protected override RowPresenter ReferenceForDisposed
                {
                    get
                    {
                        Debug.Assert(Reference.IsDisposed);
                        var children = ParentRow.Children;
                        if (children.Count == 0)
                            return null;
                        var index = Math.Min(ReferenceRawIndex - children[0].RawIndex, children.Count - 1);
                        return children[index];
                    }
                }

                protected sealed override RowPresenter CurrentRowAfterRollback
                {
                    get
                    {
                        if (Reference != null)
                            return Reference;
                        if (ParentRow.IsDisposed)
                        {
                            var baseRows = RowManager.BaseRows;
                            var index = Math.Min(ParentRowRawIndex, baseRows.Count - 1);
                            return index >= 0 ? baseRows[index] : null;
                        }
                        else
                        {
                            var rows = ParentRow.Children;
                            return rows.Count == 0 ? ParentRow : FirstOrLastOf(rows);
                        }
                    }
                }

                private sealed class InsertBeforeChildHandler : InsertChildHandler
                {
                    public InsertBeforeChildHandler(RowPresenter parentRow, RowPresenter reference)
                        : base(parentRow, reference)
                    {
                    }

                    protected override int InsertingRowRawIndex
                    {
                        get
                        {
                            if (Reference == null)
                                return 0;
                            else if (RowManager.IsEditing)
                                return Reference.Index - 1;
                            else
                                return Reference.Index;
                        }
                    }

                    protected override int CommitEditIndex
                    {
                        get { return Reference == null ? 0 : Reference.DataRow.Index; }
                    }

                    protected override InsertMode Mode
                    {
                        get { return InsertMode.Before; }
                    }
                }

                private sealed class InsertAfterChildHandler : InsertChildHandler
                {
                    public InsertAfterChildHandler(RowPresenter parentRow, RowPresenter reference)
                        : base(parentRow, reference)
                    {
                    }

                    protected override int InsertingRowRawIndex
                    {
                        get { return Reference == null ? ParentRow.Children.Count - 1 : Reference.Index + 1; }
                    }

                    protected override int CommitEditIndex
                    {
                        get { return Reference == null ? DataSet.Count : Reference.DataRow.Index + 1; }
                    }

                    protected override InsertMode Mode
                    {
                        get { return InsertMode.After; }
                    }
                }
            }
        }

        protected RowManager(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
            : base(template, dataSet, where, orderBy)
        {
            Template.RowManager = this;
            Initialize();
        }

        private void Initialize()
        {
            var coercedVirtualRowIndex = CoercedVirtualRowIndex;
            if (coercedVirtualRowIndex >= 0)
                VirtualRow = new RowPresenter(this, coercedVirtualRowIndex);
            if (Rows.Count > 0)
            {
                _currentRow = Rows[0];
                InitSelection();
            }
        }

        public RowPresenter VirtualRow { get; private set; }

        public RowPresenter InsertingRow
        {
            get { return _editing == null ? null : _editing.InsertingRow; }
        }

        private int VirtualRowIndex
        {
            get { return VirtualRow == null ? -1 : VirtualRow.Index; }
        }

        private int InsertingRowIndex
        {
            get { return InsertingRow == null ? -1 : InsertingRow.Index; }
        }

        public sealed override IReadOnlyList<RowPresenter> Rows
        {
            get { return this; }
        }

        private IReadOnlyList<RowPresenter> BaseRows
        {
            get { return base.Rows; }
        }

        #region IReadOnlyList<RowPresenter>

        private int RowCount
        {
            get
            {
                var result = base.Rows.Count;
                if (VirtualRow != null)
                    result++;
                if (InsertingRow != null)
                    result++;
                return result;
            }
        }

        private RowPresenter GetRow(int index)
        {
            Debug.Assert(index >= 0 && index < RowCount);

            var virtualRowIndex = VirtualRowIndex;
            if (index == virtualRowIndex)
                return VirtualRow;

            var insertingRowIndex = InsertingRowIndex;
            if (index == insertingRowIndex)
                return InsertingRow;

            var result = index;
            if (virtualRowIndex >= 0 && result > virtualRowIndex)
                result--;
            if (insertingRowIndex >= 0 && result > insertingRowIndex)
                result--;
            return BaseRows[result];
        }

        int IReadOnlyCollection<RowPresenter>.Count
        {
            get { return RowCount; }
        }

        RowPresenter IReadOnlyList<RowPresenter>.this[int index]
        {
            get
            {
                if (index < 0 || index >= RowCount)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return GetRow(index);
            }
        }

        IEnumerator<RowPresenter> IEnumerable<RowPresenter>.GetEnumerator()
        {
            for (int i = 0; i < RowCount; i++)
                yield return GetRow(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < RowCount; i++)
                yield return GetRow(i);
        }

        #endregion

        private VirtualRowPlacement VirtualRowPlacement
        {
            get { return Template.VirtualRowPlacement; }
        }

        protected override void Reload()
        {
            _currentRow = null;
            _selectedRows.Clear();
            _lastExtnedSelection = null;
            _editing = null;
            VirtualRow = null;
            base.Reload();
            Initialize();
        }

        private void CoerceVirtualRow()
        {
            Debug.Assert(!IsEditing);
            var coercedVirtualRowIndex = CoercedVirtualRowIndex;
            if (coercedVirtualRowIndex >= 0)
            {
                if (VirtualRow == null)
                    VirtualRow = new RowPresenter(this, coercedVirtualRowIndex);
                else
                    VirtualRow.RawIndex = coercedVirtualRowIndex;
            }
            else if (VirtualRow != null)
            {
                var virtualRow = VirtualRow;
                VirtualRow = null;
                DisposeRow(virtualRow);
            }
        }

        private int CoercedVirtualRowIndex
        {
            get
            {
                if (VirtualRowPlacement == VirtualRowPlacement.Head)
                    return 0;
                else if (VirtualRowPlacement == VirtualRowPlacement.Tail)
                    return base.Rows.Count;
                else if (VirtualRowPlacement == VirtualRowPlacement.Exclusive && base.Rows.Count == 0)
                    return 0;
                else
                    return -1;
            }
        }

        private RowPresenter _suggestedCurrentRow;
        private void CoerceCurrentRow()
        {
            if (CurrentRow == null)
            {
                if (Rows.Count > 0)
                {
                    CurrentRow = Rows[0];
                    InitSelection();
                }
            }
            else if (CurrentRow.IsDisposed)
            {
                if (_suggestedCurrentRow != null && !_suggestedCurrentRow.IsDisposed)
                    CurrentRow = _suggestedCurrentRow;
                else
                {
                    var index = CurrentRow.RawIndex;
                    Debug.Assert(index >= 0);
                    if (VirtualRow != null && index >= VirtualRow.RawIndex)
                        index++;
                    index = Math.Min(index, Rows.Count - 1);
                    CurrentRow = index >= 0 ? Rows[index] : null;
                }
            }
            _suggestedCurrentRow = null;
        }

        private void InitSelection()
        {
            if (Template.SelectionMode == SelectionMode.Single || Template.SelectionMode == SelectionMode.Extended)
                SelectCore(CurrentRow, SelectionMode.Single, null);
        }

        private RowPresenter _currentRow;
        public RowPresenter CurrentRow
        {
            get { return _currentRow; }
            set { SetCurrentRow(value, true); }
        }

        private void SetCurrentRow(RowPresenter value, bool verifyIsEditing)
        {
            var oldValue = CurrentRow;
            if (oldValue == value)
                return;
            if (verifyIsEditing && IsEditing)
                throw new InvalidOperationException(DiagnosticMessages.RowManager_ChangeEditingRowNotAllowed);
            _currentRow = value;
            OnCurrentRowChanged(oldValue);
        }

        internal override void Collapse(RowPresenter row)
        {
            if (CurrentRow.IsDescendantOf(row))
            {
                CurrentRow = row;
                if (Template.SelectionMode == SelectionMode.Single)
                    Select(row, SelectionMode.Single, row);
            }
            base.Collapse(row);
        }

        protected virtual void OnCurrentRowChanged(RowPresenter oldValue)
        {
        }

        private RowPresenter _lastExtnedSelection;
        public void Select(RowPresenter value, SelectionMode selectionMode, RowPresenter oldCurrentRow)
        {
            SelectCore(value, selectionMode, oldCurrentRow);
            OnSelectedRowsChanged();
        }

        private void SelectCore(RowPresenter value, SelectionMode selectionMode, RowPresenter oldCurrentRow)
        {
            switch (selectionMode)
            {
                case SelectionMode.Single:
                    _lastExtnedSelection = null;
                    _selectedRows.Clear();
                    value.IsSelected = true;
                    break;
                case SelectionMode.Multiple:
                    _lastExtnedSelection = null;
                    value.IsSelected = !value.IsSelected;
                    break;
                case SelectionMode.Extended:
                    if (_lastExtnedSelection == null)
                        _lastExtnedSelection = oldCurrentRow;
                    _selectedRows.Clear();
                    var min = Math.Min(_lastExtnedSelection.Index, value.Index);
                    var max = Math.Max(_lastExtnedSelection.Index, value.Index);
                    for (int i = min; i <= max; i++)
                        Rows[i].IsSelected = true;
                    break;
            }
        }

        private EditHandler _editing;
        private EditHandler Editing
        {
            get { return _editing; }
            set
            {
                Debug.Assert(_editing != value);
                var oldIsEditing = IsEditing;
                _editing = value;
                if (!IsEditing)
                    CoerceVirtualRow();
                if (oldIsEditing != IsEditing)
                    OnIsEditingChanged();
            }
        }

        protected virtual void OnIsEditingChanged()
        {
        }

        public bool IsEditing
        {
            get { return Editing != null; }
        }

        internal void BeginInsertBefore(RowPresenter parent, RowPresenter child)
        {
            Debug.Assert(!IsEditing);
            EditHandler.BeginInsertBefore(this, parent, child);
        }

        internal void BeginInsertAfter(RowPresenter parent, RowPresenter child)
        {
            Debug.Assert(!IsEditing);
            EditHandler.BeginInsertAfter(this, parent, child);
        }

        internal void BeginEdit(RowPresenter row)
        {
            EditHandler.EnterEditMode(this);
        }

        internal virtual bool EndEdit()
        {
            Debug.Assert(IsEditing);
            Editing.EndEdit(this);
            return true;
        }

        internal virtual void RollbackEdit()
        {
            Debug.Assert(IsEditing);
            Editing.CancelEdit(this);
        }

        protected virtual void OnSelectedRowsChanged()
        {
        }

        private HashSet<RowPresenter> _selectedRows = new HashSet<RowPresenter>();
        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return _selectedRows; }
        }

        internal bool IsSelected(RowPresenter row)
        {
            return _selectedRows.Contains(row);
        }

        internal void AddSelectedRow(RowPresenter row)
        {
            _selectedRows.Add(row);
            OnSelectedRowsChanged();
        }

        internal void RemoveSelectedRow(RowPresenter row)
        {
            _selectedRows.Remove(row);
            OnSelectedRowsChanged();
        }

        protected override void OnRowsChanged()
        {
            if (Editing != null)
                Editing.OnRowsChanged(this);
            else
                CoerceVirtualRow();
            CoerceCurrentRow();
        }

        protected virtual bool CurrentRowChangeSuspended
        {
            get { return false; }
        }

        protected override void DisposeRow(RowPresenter row)
        {
            row.IsSelected = false;
            base.DisposeRow(row);
        }
    }
}
