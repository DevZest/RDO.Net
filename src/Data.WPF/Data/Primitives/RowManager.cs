using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Windows.Controls;
using DevZest.Data;

namespace DevZest.Windows.Data.Primitives
{
    internal abstract class RowManager : RowNormalizer, IReadOnlyList<RowPresenter>
    {
        private abstract class _EditHandler
        {
            public static void EnterEditMode(RowManager rowManager)
            {
                var currentRow = rowManager.CurrentRow;
                if (currentRow.IsVirtual)
                    InsertHandler.EnterEditMode(rowManager);
                else
                    Singleton.OpenEdit(rowManager);
            }

            private static _EditHandler Singleton = new DataRowEditCommand();

            protected _EditHandler()
            {
            }

            public abstract void OnRowsChanged(RowManager rowManager);

            protected void OpenEdit(RowManager rowManager)
            {
                rowManager.EditHandler = this;
                BeginEdit(rowManager);
                CoerceVirtualRowIndex(rowManager);
            }

            public void RollbackEdit(RowManager rowManager)
            {
                CancelEdit(rowManager);
                rowManager.EditHandler = null;
            }

            public void CommitEdit(RowManager rowManager)
            {
                EndEdit(rowManager);
                rowManager.EditHandler = null;
            }

            protected abstract void BeginEdit(RowManager rowManager);

            protected abstract void CancelEdit(RowManager rowManager);

            protected abstract void EndEdit(RowManager rowManager);

            public abstract void CoerceVirtualRowIndex(RowManager rowManager);

            private sealed class DataRowEditCommand : _EditHandler
            {
                protected override void BeginEdit(RowManager rowManager)
                {
                    rowManager.CurrentRow.DataRow.BeginEdit();
                }

                protected override void CancelEdit(RowManager rowManager)
                {
                    rowManager.CurrentRow.DataRow.CancelEdit();
                }

                protected override void EndEdit(RowManager rowManager)
                {
                    rowManager.CurrentRow.DataRow.EndEdit();
                }

                public override void CoerceVirtualRowIndex(RowManager rowManager)
                {
                }

                public override void OnRowsChanged(RowManager rowManager)
                {
                    if (rowManager.CurrentRow.IsDisposed)
                        RollbackEdit(rowManager);
                }
            }
        }

        private abstract class InsertHandler : _EditHandler
        {
            public static new void EnterEditMode(RowManager rowManager)
            {
                GetEditCommand(rowManager.VirtualRowPlacement).OpenEdit(rowManager);
            }

            private static InsertHandler GetEditCommand(VirtualRowPlacement virtualRowPlacement)
            {
                if (virtualRowPlacement == VirtualRowPlacement.Head)
                    return Head;
                else if (virtualRowPlacement == VirtualRowPlacement.Tail)
                    return Tail;
                else
                {
                    Debug.Assert(virtualRowPlacement == VirtualRowPlacement.Exclusive);
                    return Tail;
                }
            }

            public static void BeginInsertBefore(RowManager rowManager, RowPresenter parent, RowPresenter child)
            {
                BeginInsert(rowManager, GetInsertBeforeHandler(parent, child));
            }

            private static InsertHandler GetInsertBeforeHandler(RowPresenter parent, RowPresenter child)
            {
                return parent == null && child == null ? Tail : new InsertBeforeHandler(parent, child);
            }

            public static void BeginInsertAfter(RowManager rowManager, RowPresenter parent, RowPresenter child)
            {
                BeginInsert(rowManager, GetInsertAfterHandler(parent, child));
            }

            private static InsertHandler GetInsertAfterHandler(RowPresenter parent, RowPresenter child)
            {
                return parent == null && child == null ? Tail : new InsertAfterHandler(parent, child);
            }

            private static void BeginInsert(RowManager rowManager, InsertHandler insertHandler)
            {
                if (rowManager.VirtualRow == null)
                    rowManager.VirtualRow = new RowPresenter(rowManager, null);
                insertHandler.CoerceVirtualRowIndex(rowManager);

                if (rowManager.CurrentRow != rowManager.VirtualRow)
                    rowManager.CurrentRow = rowManager.VirtualRow;

                insertHandler.OpenEdit(rowManager);
            }

            private static readonly InsertHandler Head = new InsertBeforeHandler(null, null);
            private static readonly InsertHandler Tail = new InsertAfterHandler(null, null);

            protected InsertHandler(RowPresenter parent, RowPresenter reference)
            {
                _parent = parent;
                Reference = reference;
            }

            private readonly RowPresenter _parent;
            protected RowPresenter Parent { get; }

            protected RowPresenter Reference { get; private set; }

            public sealed override void CoerceVirtualRowIndex(RowManager rowManager)
            {
                rowManager.VirtualRow.RawIndex = GetVirtualRowIndex(rowManager);
            }

            protected abstract int GetVirtualRowIndex(RowManager rowManager);

            private DataSet GetDataSet(RowManager rowManager)
            {
                return _parent == null ? rowManager.DataSet : _parent.DataSet;
            }

            private int GetInsertIndex(RowManager rowManager)
            {
                var result = GetVirtualRowIndex(rowManager);
                if (Parent != null)
                    result -= Parent.RawIndex + 1;
                return result;
            }

            protected sealed override void BeginEdit(RowManager rowManager)
            {
                Debug.Assert(rowManager.CurrentRow.IsVirtual);
                rowManager.CurrentRow.DataRow = GetDataSet(rowManager).BeginAdd();
            }

            protected sealed override void CancelEdit(RowManager rowManager)
            {
                Debug.Assert(rowManager.CurrentRow.IsVirtual);
                rowManager.CurrentRow.DataRow = null;
                GetDataSet(rowManager).CancelAdd();
            }

            protected sealed override void EndEdit(RowManager rowManager)
            {
                rowManager.CurrentRow.DataRow = null;
                GetDataSet(rowManager).EndAdd(GetInsertIndex(rowManager));
            }

            public override void OnRowsChanged(RowManager rowManager)
            {
                CoerceVirtualRowIndex(rowManager);
            }

            private sealed class InsertBeforeHandler : InsertHandler
            {
                public InsertBeforeHandler(RowPresenter parent, RowPresenter reference)
                    : base(parent, reference)
                {
                }

                protected override int GetVirtualRowIndex(RowManager rowManager)
                {
                    if (Reference != null)
                        return Reference.RawIndex;
                    else
                        return Parent != null ? Parent.RawIndex + 1 : 0;
                }
            }

            private sealed class InsertAfterHandler : InsertHandler
            {
                public InsertAfterHandler(RowPresenter parent, RowPresenter reference)
                    : base(parent, reference)
                {
                }

                protected override int GetVirtualRowIndex(RowManager rowManager)
                {
                    if (Reference != null)
                        return Reference.RawIndex + 1;
                    else
                        return Parent != null ? Parent.RawIndex + Parent.Children.Count + 1 : rowManager.BaseRows.Count;
                }
            }
        }

        protected RowManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy)
        {
            Template.RowManager = this;
            var coercedVirtualRowIndex = CoercedVirtualRowIndex;
            if (coercedVirtualRowIndex >= 0)
                VirtualRow = new RowPresenter(this, coercedVirtualRowIndex);
            if (Rows.Count > 0)
                _currentRow = Rows[0];
        }

        public RowPresenter VirtualRow { get; private set; }

        private int VirtualRowIndex
        {
            get { return VirtualRow == null ? -1 : VirtualRow.Index; }
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
                return result;
            }
        }

        private RowPresenter GetRow(int index)
        {
            Debug.Assert(index >= 0 && index < RowCount);

            if (index == VirtualRowIndex)
                return VirtualRow;

            if (VirtualRowIndex >= 0 && index > VirtualRowIndex)
                index--;
            return base.Rows[index];
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
                    CurrentRow = Rows[0];
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

        private RowPresenter _currentRow;
        public RowPresenter CurrentRow
        {
            get { return _currentRow; }
            set
            {
                var oldValue = CurrentRow;
                if (oldValue == value)
                    return;
                if (!CanChangeCurrentRow)
                    throw new InvalidOperationException(Strings.RowManager_ChangeCurrentRowNotAllowed);
                _currentRow = value;
                OnCurrentRowChanged(oldValue);
            }
        }

        public virtual bool CanChangeCurrentRow
        {
            get { return !IsEditing; }
        }

        protected virtual void OnCurrentRowChanged(RowPresenter oldValue)
        {
        }

        private RowPresenter _lastExtnedSelection;
        public void Select(RowPresenter value, SelectionMode selectionMode, RowPresenter currentRow)
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
                        _lastExtnedSelection = currentRow;
                    _selectedRows.Clear();
                    var min = Math.Min(_lastExtnedSelection.Index, value.Index);
                    var max = Math.Max(_lastExtnedSelection.Index, value.Index);
                    for (int i = min; i <= max; i++)
                        _selectedRows.Add(Rows[i]);
                    break;
            }

            OnSelectedRowsChanged();
        }

        private _EditHandler _editHandler;
        private _EditHandler EditHandler
        {
            get { return _editHandler; }
            set
            {
                Debug.Assert(_editHandler != value);
                var oldIsEditing = IsEditing;
                _editHandler = value;
                if (_editHandler == null)
                    CoerceVirtualRow();
                var newIsEditing = IsEditing;
                if (oldIsEditing != newIsEditing)
                    OnIsEditingChanged();
            }
        }

        protected virtual void OnIsEditingChanged()
        {
        }

        public bool IsEditing
        {
            get { return EditHandler != null; }
        }

        internal void BeginInsertBefore(RowPresenter parent, RowPresenter child)
        {
            Debug.Assert(!IsEditing);
            InsertHandler.BeginInsertBefore(this, parent, child);
        }

        internal void BeginInsertAfter(RowPresenter parent, RowPresenter child)
        {
            Debug.Assert(!IsEditing);
            InsertHandler.BeginInsertAfter(this, parent, child);
        }

        internal void BeginEdit(RowPresenter row)
        {
            _EditHandler.EnterEditMode(this);
        }

        internal void CommitEdit()
        {
            Debug.Assert(IsEditing);
            EditHandler.CommitEdit(this);
        }

        internal void RollbackEdit()
        {
            Debug.Assert(IsEditing);
            EditHandler.RollbackEdit(this);
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
            if (EditHandler != null)
                EditHandler.OnRowsChanged(this);
            else
                CoerceVirtualRow();
            CoerceCurrentRow();
        }
    }
}
