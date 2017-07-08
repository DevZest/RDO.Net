using System;
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
                        return new EditVirtualHeadHandler();
                    else
                    {
                        Debug.Assert(virtualRowPlacement == VirtualRowPlacement.Tail);
                        return new EditVirtualTailHandler();
                    }
                }
                else
                    return EditCurrentHandler.Singleton;
            }

            public static void BeginInsertBefore(RowManager rowManager, RowPresenter parent, RowPresenter child)
            {
                new InsertBeforeHandler(parent, child).BeginEdit(rowManager);
            }

            public static void BeginInsertAfter(RowManager rowManager, RowPresenter parent, RowPresenter child)
            {
                new InsertAfterHandler(parent, child).BeginEdit(rowManager);
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
                public override void OnRowsChanged(RowManager rowManager)
                {
                    if (!rowManager.IsRowsChangedSuspended)
                        CoerceVirtualRows(rowManager);
                }

                protected RowPresenter _insertingRow;
                public sealed override RowPresenter InsertingRow
                {
                    get { return _insertingRow; }
                }

                protected virtual DataSet GetDataSet(RowManager rowManager)
                {
                    return rowManager.DataSet;
                }

                protected abstract void CoerceVirtualRows(RowManager rowManager);

                private void RollbackCurrentRow(RowManager rowManager)
                {
                    RollbackCurrentRow(rowManager, rowManager.VirtualRow);
                }

                protected virtual void RollbackCurrentRow(RowManager rowManager, RowPresenter newValue)
                {
                    rowManager.CurrentRow = newValue;
                }

                protected sealed override void RollbackEdit(RowManager rowManager)
                {
                    Debug.Assert(!rowManager.IsEditing);

                    GetDataSet(rowManager).CancelAdd();
                    RollbackCurrentRow(rowManager);
                    DisposeInsertingRow(rowManager);
                    rowManager.OnRowsChanged();
                }

                protected abstract int GetCommitEditIndex(RowManager rowManager);

                protected sealed override void CommitEdit(RowManager rowManager)
                {
                    Debug.Assert(!rowManager.IsEditing);

                    var newDataRow = GetDataSet(rowManager).EndAdd(GetCommitEditIndex(rowManager));
                    var newCurrentRow = rowManager[newDataRow];
                    if (!rowManager.CurrentRowChangeSuspended)
                    {
                        if (newCurrentRow != null)
                            rowManager.CurrentRow = newCurrentRow;
                        else
                            RollbackCurrentRow(rowManager);
                    }
                    DisposeInsertingRow(rowManager);
                    rowManager.OnRowsChanged();
                }

                private void DisposeInsertingRow(RowManager rowManager)
                {
                    var insertingRow = _insertingRow;
                    _insertingRow = null;
                    insertingRow.DataRow = null;
                    insertingRow.RawIndex = -1;
                    insertingRow.Dispose();
                    rowManager.CoerceCurrentRow();
                }
            }

            private abstract class EditVirtualHandler : InsertHandler
            {
                protected override void OpenEdit(RowManager rowManager)
                {
                    Debug.Assert(rowManager.VirtualRow != null);

                    _insertingRow = rowManager.VirtualRow;
                    _insertingRow.DataRow = GetDataSet(rowManager).BeginAdd();
                    rowManager.VirtualRow = new RowPresenter(rowManager, -1);
                    rowManager.Editing = this;
                    rowManager.OnRowsChanged();
                }
            }

            private sealed class EditVirtualHeadHandler : EditVirtualHandler
            {
                protected override int GetCommitEditIndex(RowManager rowManager)
                {
                    return 0;
                }

                protected override void CoerceVirtualRows(RowManager rowManager)
                {
                    Debug.Assert(rowManager.VirtualRow != null);

                    if (InsertingRow != null)
                        InsertingRow.RawIndex = 1;
                }
            }

            private sealed class EditVirtualTailHandler : EditVirtualHandler
            {
                protected override int GetCommitEditIndex(RowManager rowManager)
                {
                    return rowManager.DataSet.Count;
                }

                protected override void CoerceVirtualRows(RowManager rowManager)
                {
                    Debug.Assert(rowManager.VirtualRow != null);

                    if (InsertingRow != null)
                    {
                        InsertingRow.RawIndex = rowManager.DataSet.Count;
                        rowManager.VirtualRow.RawIndex = InsertingRow.RawIndex + 1;
                    }
                    else
                        rowManager.VirtualRow.RawIndex = rowManager.DataSet.Count;
                }
            }

            private abstract class ExplicitInsertHandler : InsertHandler
            {
                protected ExplicitInsertHandler(RowPresenter parent, RowPresenter reference)
                {
                    Debug.Assert(reference != null);
                    Parent = parent;
                    Reference = reference;
                }

                public override void OnRowsChanged(RowManager rowManager)
                {
                    throw new NotImplementedException();
                    //base.OnRowsChanged(rowManager);
                }

                protected sealed override void OpenEdit(RowManager rowManager)
                {
                    Debug.Assert(!rowManager.IsEditing);

                    _insertingRow = new RowPresenter(rowManager, GetDataSet(rowManager).BeginAdd());
                    rowManager.OnRowsChanged();
                    rowManager.CurrentRow = _insertingRow;
                }

                protected RowPresenter Parent { get; private set; }

                protected RowPresenter Reference { get; private set; }
 
                protected override DataSet GetDataSet(RowManager rowManager)
                {
                    return Parent == null ? rowManager.DataSet : Parent.DataSet;
                }
            }

            private sealed class InsertBeforeHandler : ExplicitInsertHandler
            {
                public InsertBeforeHandler(RowPresenter parent, RowPresenter reference)
                    : base(parent, reference)
                {
                }

                protected override void CoerceVirtualRows(RowManager rowManager)
                {
                    throw new NotImplementedException();
                }

                protected override int GetCommitEditIndex(RowManager rowManager)
                {
                    throw new NotImplementedException();
                }
            }

            private sealed class InsertAfterHandler : ExplicitInsertHandler
            {
                public InsertAfterHandler(RowPresenter parent, RowPresenter reference)
                    : base(parent, reference)
                {
                }

                protected override void CoerceVirtualRows(RowManager rowManager)
                {
                    throw new NotImplementedException();
                }

                protected override int GetCommitEditIndex(RowManager rowManager)
                {
                    throw new NotImplementedException();
                }
            }
        }

        protected RowManager(Template template, DataSet dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
            : base(template, dataSet, where, orderBy)
        {
            Template.RowManager = this;
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
            base.Reload();
            CoerceVirtualRow();
            CoerceCurrentRow();
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
                var index = CurrentRow.RawIndex;
                Debug.Assert(index >= 0);
                if (VirtualRow != null && index >= VirtualRow.RawIndex)
                    index++;
                index = Math.Min(index, Rows.Count - 1);
                CurrentRow = index >= 0 ? Rows[index] : null;
            }
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
            set
            {
                var oldValue = CurrentRow;
                if (oldValue == value)
                    return;
                if (IsEditing)
                    throw new InvalidOperationException(Strings.RowManager_ChangeEditingRowNotAllowed);
                _currentRow = value;
                OnCurrentRowChanged(oldValue);
            }
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
                    _selectedRows.Add(value);
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
                        _selectedRows.Add(Rows[i]);
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

        internal void RollbackEdit()
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
    }
}
