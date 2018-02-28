using DevZest.Data.Primitives;
using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public sealed class RowPresenter : ElementPresenter
    {
        internal RowPresenter(RowMapper rowMapper, DataRow dataRow)
            : this(rowMapper, dataRow, -1)
        {
        }

        internal RowPresenter(RowMapper rowMapper, int rawIndex)
            : this(rowMapper, null, rawIndex)
        {
        }

        private RowPresenter(RowMapper rowMapper, DataRow dataRow, int rawIndex)
        {
            Debug.Assert(rowMapper != null);
            _rowMapper = rowMapper;
            DataRow = dataRow;
            RawIndex = rawIndex;
        }

        internal void Dispose()
        {
            _rowMapper = null;
            Parent = null;
        }

        internal bool IsDisposed
        {
            get { return _rowMapper == null; }
        }

        private void VerifyDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private RowMapper _rowMapper;

        private RowMapper RowMapper
        {
            get
            {
                VerifyDisposed();
                return _rowMapper;
            }
        }

        private RowNormalizer RowNormalizer
        {
            get { return RowMapper as RowNormalizer; }
        }

        internal RowManager RowManager
        {
            get { return RowMapper as RowManager; }
        }

        internal InputManager InputManager
        {
            get { return RowMapper as InputManager; }
        }

        internal ElementManager ElementManager
        {
            get { return RowMapper as ElementManager; }
        }

        internal LayoutManager LayoutManager
        {
            get { return RowMapper as LayoutManager; }
        }

        private bool IsRecursive
        {
            get { return !IsVirtual && Template.IsRecursive; }
        }

        public sealed override Template Template
        {
            get { return IsDisposed ? null : _rowMapper.Template; }
        }

        public DataRow DataRow { get; internal set; }

        public bool IsVirtual
        {
            get { return RowManager == null ? false : (RowManager.VirtualRow == this || RowManager.InsertingRow == this); }
        }

        public bool IsEditing
        {
            get { return RowManager == null ? false : (RowManager.CurrentRow == this && RowManager.IsEditing); }
        }

        public bool IsInserting
        {
            get { return IsVirtual && IsEditing; }
        }

        public RowPresenter Parent { get; internal set; }

        internal bool IsDescendantOf(RowPresenter rowPresenter)
        {
            if (rowPresenter == null)
                return false;

            for (var parent = Parent; parent != null; parent = parent.Parent)
            {
                if (parent == rowPresenter)
                    return true;
            }
            return false;
        }

        private List<RowPresenter> _children;

        public IReadOnlyList<RowPresenter> Children
        {
            get
            {
                if (_children != null)
                    return _children;
                return Array<RowPresenter>.Empty;
            }
        }

        internal void InitializeChildren()
        {
            Debug.Assert(IsRecursive);

            _children = _rowMapper.GetOrCreateChildren(this);
            foreach (var child in Children)
                child.InitializeChildren();
        }

        internal void InsertChild(int index, RowPresenter child)
        {
            Debug.Assert(index >= 0 && index <= Children.Count);
            if (_children == null)
                _children = new List<RowPresenter>();
            _children.Insert(index, child);
        }

        internal void RemoveChild(int index)
        {
            Debug.Assert(index >= 0 && index < Children.Count);
            _children.RemoveAt(index);
            if (_children.Count == 0)
                _children = null;
        }

        private void Invalidate()
        {
            var elementManager = ElementManager;
            if (elementManager != null)
                elementManager.InvalidateView();
        }

        internal int RawIndex { get; set; }

        public int Index
        {
            get
            {
                if (IsDisposed)
                    return -1;

                if (IsVirtual)
                    return RawIndex;

                var result = RawIndex;

                var virtualRow = RowManager.VirtualRow;
                if (virtualRow != null && result >= virtualRow.Index)
                    result++;
                var insertingRow = RowManager.InsertingRow;
                if (insertingRow != null && result >= insertingRow.Index)
                    result++;
                return result;
            }
        }

        public int Depth
        {
            get { return Parent == null ? 0 : RowMapper.GetDepth(Parent.DataRow) + 1; }
        }

        public bool HasChildren
        {
            get
            {
                var dataPresenter = DataPresenter;
                return dataPresenter != null ? dataPresenter.HasChildren(this) : InternalHasChildren;
            }
        }

        internal bool InternalHasChildren
        {
            get { return Children.Count > 0; }
        }

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            private set
            {
                _isExpanded = value;
                Invalidate();
            }
        }

        public void ToggleExpandState()
        {
            var dataPresenter = DataPresenter;
            if (dataPresenter != null)
                dataPresenter.ToggleExpandState(this);
            else
                InternalToggleExpandState();
        }

        internal void InternalToggleExpandState()
        {
            if (IsExpanded)
                Collapse();
            else
                Expand();
        }

        internal void Expand()
        {
            if (IsExpanded)
                return;

            if (IsRecursive)
                RowNormalizer.Expand(this);
            IsExpanded = true;
        }

        internal void Collapse()
        {
            if (!IsExpanded)
                return;

            if (IsRecursive)
                RowNormalizer.Collapse(this);
            IsExpanded = false;
        }

        public bool IsCurrent
        {
            get { return RowManager.CurrentRow == this; }
        }

        public bool IsSelected
        {
            get { return RowManager.IsSelected(this); }
            set
            {
                if (IsVirtual)
                    return;

                var oldValue = IsSelected;
                if (oldValue == value)
                    return;

                if (oldValue)
                    RowManager.RemoveSelectedRow(this);
                if (value)
                    RowManager.AddSelectedRow(this);
            }
        }

        public T GetValue<T>(Column<T> column, bool beforeEdit = false)
        {
            if (Depth > 0)
            {
                var model = DataRow.Model;
                IReadOnlyList<Column> columns = column.IsLocal ? model.GetLocalColumns() : model.GetColumns();
                column = (Column<T>)columns[column.Ordinal];
            }
            return DataRow == null ? default(T) : column[DataRow, beforeEdit];
        }

        public object GetValue(Column column, bool beforeEdit = false)
        {
            if (Depth > 0)
            {
                var model = DataRow.Model;
                IReadOnlyList<Column> columns = column.IsLocal ? model.GetLocalColumns() : model.GetColumns();
                column = columns[column.Ordinal];
            }
            return DataRow == null ? null : column.GetValue(DataRow, beforeEdit);
        }

        private void VerifyColumn(Column column, string paramName)
        {
            if (column == null)
                throw new ArgumentNullException(paramName);

            if (column.GetParentModel() != RowMapper.DataSet.Model)
                throw new ArgumentException(DiagnosticMessages.RowPresenter_VerifyColumn, paramName);
        }

        public object this[Column column]
        {
            get
            {
                VerifyColumn(column, nameof(column));

                if (Depth > 0)
                    column = DataRow.Model.GetColumns()[column.Ordinal];
                return DataRow == null ? column.GetDefaultValue() : column.GetValue(DataRow);
            }
            set
            {
                VerifyColumn(column, nameof(column));

                if (Depth > 0)
                    column = DataRow.Model.GetColumns()[column.Ordinal];
                BeginEdit();
                column.SetValue(DataRow, value);
                Invalidate();
            }
        }

        public bool IsNull(Column column)
        {
            VerifyColumn(column, nameof(column));
            var dataRow = DataRow;
            return dataRow == null ? false : column.IsNull(dataRow);
        }

        private bool HasPendingEdit
        {
            get { return RowManager.IsEditing || DataSet.EditingRow != null; }
        }

        private void VerifyNoPendingEdit()
        {
            if (HasPendingEdit)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_VerifyNoPendingEdit);
        }

        internal void VerifyIsCurrent()
        {
            if (!IsCurrent)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_VerifyIsCurrent);
        }

        public void BeginEdit()
        {
            if (IsEditing)
                return;

            VerifyIsCurrent();
            VerifyNoPendingEdit();
            if (IsVirtual && RowManager.Template.VirtualRowPlacement == VirtualRowPlacement.Exclusive)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_BeginEditExclusiveVirtual);
            DataPresenter?.SuspendInvalidateView();
            RowManager.BeginEdit();
            DataPresenter?.ResumeInvalidateView();
        }

        public void EditValue<T>(Column<T> column, T value)
        {
            VerifyColumn(column, nameof(column));

            if (Depth > 0)
            {
                var model = DataRow.Model;
                if (column.IsLocal)
                    column = (Column<T>)model.GetLocalColumns()[column.Ordinal];
                else
                    column = (Column<T>)DataRow.Model.GetColumns()[column.Ordinal];
            }

            DataPresenter?.SuspendInvalidateView();
            BeginEdit();
            column[DataRow] = value;
            Invalidate();
            DataPresenter?.ResumeInvalidateView();
        }

        public void CancelEdit()
        {
            if (!IsEditing)
                throw new InvalidOperationException(DiagnosticMessages._VerifyIsEditing);

            DataPresenter?.SuspendInvalidateView();
            RowManager.CancelEdit();
            DataPresenter?.ResumeInvalidateView();
        }

        public bool EndEdit()
        {
            if (!IsEditing)
                throw new InvalidOperationException(DiagnosticMessages._VerifyIsEditing);

            DataPresenter?.SuspendInvalidateView();
            var result = RowManager.EndEdit();
            DataPresenter?.ResumeInvalidateView();
            return result;
        }

        private bool ConfirmEndEdit()
        {
            return DataPresenter == null ? true : DataPresenter.ConfirmEndEdit();
        }

        public void BeginInsertBefore(RowPresenter child = null)
        {
            VerifyInsert(child);
            DataPresenter?.SuspendInvalidateView();
            RowManager.BeginInsertBefore(this, child);
            DataPresenter?.ResumeInvalidateView();
        }

        public void BeginInsertAfter(RowPresenter child = null)
        {
            VerifyInsert(child);
            DataPresenter?.SuspendInvalidateView();
            RowManager.BeginInsertAfter(this, child);
            DataPresenter?.ResumeInvalidateView();
        }

        private void VerifyInsert(RowPresenter child)
        {
            if (child == null)
                VerifyNoPendingEdit();
            else
            {
                if (child.Parent != this)
                    throw new ArgumentException(DiagnosticMessages.RowPresenter_InvalidChildRow, nameof(child));
                child.VerifyNoPendingEdit();
            }
        }

        public DataSet DataSet
        {
            get { return Parent == null ? RowManager.DataSet : Parent.DataRow[Template.RecursiveModelOrdinal]; }
        }

        public void Delete()
        {
            VerifyDisposed();
            if (IsVirtual)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_DeleteVirtualRow);

            DataRow.DataSet.Remove(DataRow);
        }

        public RowView View { get; internal set; }

        internal RowBindingCollection RowBindings
        {
            get { return Template.InternalRowBindings; }
        }

        public void SetValue(ColumnValueBag valueBag, Column column)
        {
            if (valueBag == null)
                throw new ArgumentNullException(nameof(valueBag));
            VerifyColumn(column, nameof(column));
            if (DataRow != null)
                valueBag.SetValue(column, DataRow);
            else
                valueBag[column] = column.GetDefaultValue();
        }

        public void SetValueBag(ColumnValueBag valueBag, PrimaryKey key, ModelExtender extender)
        {
            if (valueBag == null)
                throw new ArgumentNullException(nameof(valueBag));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            for (int i = 0; i < key.Count; i++)
            {
                var column = key[i].Column;
                SetValue(valueBag, column);
            }

            if (extender != null)
            {
                var columns = extender.Columns;
                for (int i = 0; i < columns.Count; i++)
                    SetValue(valueBag, columns[i]);
            }
        }

        public ColumnValueBag AutoSelect(PrimaryKey key, ModelExtender extender)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var result = new ColumnValueBag();
            result.AutoSelect(key, DataRow);
            if (extender != null)
                result.AutoSelect(extender, DataRow);
            return result;
        }

        public void Validate(bool invalidateView = true)
        {
            var rowValidation = InputManager.RowValidation;
            rowValidation.Validate(this, true);
            if (invalidateView)
                InputManager.InvalidateView();
        }

        public ValidationInfo GetValidationInfo(Input<RowBinding, IColumns> input)
        {
            Check.NotNull(input, nameof(input));
            return InputManager.GetValidationInfo(this, input);
        }

        public bool HasValidationError(Input<RowBinding, IColumns> input)
        {
            Check.NotNull(input, nameof(input));
            return InputManager.HasValidationError(this, input);
        }

        public bool IsValidating(Input<RowBinding, IColumns> input)
        {
            Check.NotNull(input, nameof(input));
            return InputManager.IsValidating(this, input);
        }

        public IValidationErrors VisibleValidationErrors
        {
            get { return InputManager.GetVisibleValidationErrors(this); }
        }

        public bool HasVisibleValidationError
        {
            get { return InputManager.HasVisibleValidationError(this); }
        }
    }
}
