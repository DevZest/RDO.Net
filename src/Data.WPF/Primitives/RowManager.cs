using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class RowManager : RowNormalizer, IReadOnlyList<RowPresenter>
    {
        private abstract class _EditHandler
        {
            public static void EnterEditMode(RowManager rowManager)
            {
                Debug.Assert(rowManager.CurrentRow.CanEdit);
                var currentRow = rowManager.CurrentRow;
                if (currentRow.IsPlaceholder)
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
                CoercePlaceholderIndex(rowManager);
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

            public abstract void CoercePlaceholderIndex(RowManager rowManager);

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

                public override void CoercePlaceholderIndex(RowManager rowManager)
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
                Debug.Assert(rowManager.CurrentRow.CanEdit);
                GetEditCommand(rowManager.PlaceholderMode).OpenEdit(rowManager);
            }

            private static InsertHandler GetEditCommand(RowPlaceholderMode placeholderMode)
            {
                if (placeholderMode == RowPlaceholderMode.Head)
                    return Head;
                else if (placeholderMode == RowPlaceholderMode.Tail)
                    return Tail;
                else
                {
                    Debug.Assert(placeholderMode == RowPlaceholderMode.EmptyView);
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
                if (rowManager.Placeholder == null)
                    rowManager.Placeholder = new RowPresenter(rowManager, null);
                insertHandler.CoercePlaceholderIndex(rowManager);

                if (rowManager.CurrentRow != rowManager.Placeholder)
                    rowManager.SetCurrentRow(rowManager.Placeholder);

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

            public sealed override void CoercePlaceholderIndex(RowManager rowManager)
            {
                rowManager.Placeholder.RawIndex = GetPlaceholderIndex(rowManager);
            }

            protected abstract int GetPlaceholderIndex(RowManager rowManager);

            private DataSet GetDataSet(RowManager rowManager)
            {
                return _parent == null ? rowManager.DataSet : _parent.DataSet;
            }

            private int GetInsertIndex(RowManager rowManager)
            {
                var result = GetPlaceholderIndex(rowManager);
                if (Parent != null)
                    result -= Parent.RawIndex + 1;
                return result;
            }

            protected sealed override void BeginEdit(RowManager rowManager)
            {
                Debug.Assert(rowManager.CurrentRow.IsPlaceholder);
                rowManager.CurrentRow.DataRow = GetDataSet(rowManager).BeginAdd();
            }

            protected sealed override void CancelEdit(RowManager rowManager)
            {
                Debug.Assert(rowManager.CurrentRow.IsPlaceholder);
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
                CoercePlaceholderIndex(rowManager);
            }

            private sealed class InsertBeforeHandler : InsertHandler
            {
                public InsertBeforeHandler(RowPresenter parent, RowPresenter reference)
                    : base(parent, reference)
                {
                }

                protected override int GetPlaceholderIndex(RowManager rowManager)
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

                protected override int GetPlaceholderIndex(RowManager rowManager)
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
            var coercedPlaceholderIndex = CoercedPlaceholderIndex;
            if (coercedPlaceholderIndex >= 0)
                Placeholder = new RowPresenter(this, coercedPlaceholderIndex);
            if (Rows.Count > 0)
                _currentRow = Rows[0];
        }

        public RowPresenter Placeholder { get; private set; }

        private int PlaceholderIndex
        {
            get { return Placeholder == null ? -1 : Placeholder.Index; }
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
                if (Placeholder != null)
                    result++;
                return result;
            }
        }

        private RowPresenter GetRow(int index)
        {
            Debug.Assert(index >= 0 && index < RowCount);

            if (index == PlaceholderIndex)
                return Placeholder;

            if (PlaceholderIndex >= 0 && index > PlaceholderIndex)
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

        private RowPlaceholderMode PlaceholderMode
        {
            get { return Template.RowPlaceholderMode; }
        }

        protected override void Reload()
        {
            base.Reload();
            CoercePlaceholder();
            CoerceCurrentRow();
        }

        private void CoercePlaceholder()
        {
            Debug.Assert(!IsEditing);
            var coercedPlaceholderIndex = CoercedPlaceholderIndex;
            if (coercedPlaceholderIndex >= 0)
            {
                if (Placeholder == null)
                    Placeholder = new RowPresenter(this, coercedPlaceholderIndex);
                else
                    Placeholder.RawIndex = coercedPlaceholderIndex;
            }
            else if (Placeholder != null)
            {
                var placeholder = Placeholder;
                Placeholder = null;
                DisposeRow(placeholder);
            }
        }

        private int CoercedPlaceholderIndex
        {
            get
            {
                if (PlaceholderMode == RowPlaceholderMode.Head)
                    return 0;
                else if (PlaceholderMode == RowPlaceholderMode.Tail)
                    return base.Rows.Count;
                else if (PlaceholderMode == RowPlaceholderMode.EmptyView && base.Rows.Count == 0)
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
                    SetCurrentRow(Rows[0]);
            }
            else if (CurrentRow.IsDisposed)
            {
                var index = CurrentRow.RawIndex;
                Debug.Assert(index >= 0);
                if (Placeholder != null && index >= Placeholder.RawIndex)
                    index++;
                index = Math.Min(index, Rows.Count - 1);
                SetCurrentRow(index >= 0 ? Rows[index] : null);
            }
        }

        private void SetCurrentRow(RowPresenter row)
        {
            Debug.Assert(CurrentRow != row);
            CurrentRow = row;
        }

        private RowPresenter _currentRow;
        public RowPresenter CurrentRow
        {
            get { return _currentRow; }
            private set
            {
                Debug.Assert(value == null || value.RowManager == this);
                if (value == _currentRow)
                    return;
                var oldValue = _currentRow;
                _currentRow = value;
                OnCurrentRowChanged(oldValue);
            }
        }

        private _EditHandler _editHandler;
        private _EditHandler EditHandler
        {
            get { return _editHandler; }
            set
            {
                Debug.Assert(_editHandler != value);
                _editHandler = value;
                if (_editHandler == null)
                    CoercePlaceholder();
            }
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
            Debug.Assert(row.CanEdit);
            if (CurrentRow != row)
                SetCurrentRow(row);
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

        protected virtual void OnCurrentRowChanged(RowPresenter oldValue)
        {
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
                CoercePlaceholder();
            CoerceCurrentRow();
        }
    }
}
