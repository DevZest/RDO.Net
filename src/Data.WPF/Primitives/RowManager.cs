using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class RowManager
    {
        internal RowManager(Template template, DataSet dataSet)
        {
            Debug.Assert(template != null && template.RowManager == null);
            Debug.Assert(dataSet != null);
            _template = template;
            _template.RowManager = this;
            _dataSet = dataSet;

            InitializeRowMappings();
            InitializeRows();
            CoerceEofRow();
            SetCurrentRow(CoercedCurrentRow);
        }

        private readonly Template _template;
        public Template Template
        {
            get { return _template; }
        }

        public bool IsHierarchical
        {
            get { return Template.HierarchicalModelOrdinal >= 0; }
        }

        private readonly DataSet _dataSet;
        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        int _rowPresenterStateFlags;

        private static int GetMask(RowPresenterState rowPresenterState)
        {
            return 1 << (int)rowPresenterState;
        }

        private bool GetStateFlag(RowPresenterState rowPresenterState)
        {
            return (_rowPresenterStateFlags & GetMask(rowPresenterState)) != 0;
        }

        private void SetStateFlag(RowPresenterState rowPresenterState)
        {
            _rowPresenterStateFlags |= GetMask(rowPresenterState);

        }

        internal void OnGetState(RowPresenter rowPresenter, RowPresenterState rowPresenterState)
        {
            if (BindingContext.Current.RowPresenter == rowPresenter)
                SetStateFlag(rowPresenterState);
        }

        internal void OnSetState(RowPresenter rowPresenter, RowPresenterState rowPresenterState)
        {
            if (GetStateFlag(rowPresenterState))
                Invalidate(rowPresenter);
        }

        protected abstract void Invalidate(RowPresenter rowPresenter);

        int _dataPresenterStateFlags;

        private static int GetMask(DataPresenterState dataPresenterState)
        {
            return 1 << (int)dataPresenterState;
        }

        private bool GetStateFlag(DataPresenterState dataPresenterState)
        {
            return (_dataPresenterStateFlags & GetMask(dataPresenterState)) != 0;
        }

        private void SetStateFlag(DataPresenterState dataPresenterState)
        {
            _dataPresenterStateFlags |= GetMask(dataPresenterState);

        }

        protected void OnGetState(DataPresenterState dataPresenterState)
        {
            if (BindingContext.Current.RowManager == this)
                SetStateFlag(dataPresenterState);
        }

        protected virtual void OnSetState(DataPresenterState dataPresenterState)
        {
            if (BindingContext.Current.RowManager == this && GetStateFlag(dataPresenterState))
                Invalidate(null);
        }

        private List<List<RowPresenter>> _rowMappings;

        private RowPresenter RowMappings_CreateRow(DataRow dataRow)
        {
            Debug.Assert(dataRow != null);

            var rows = _rowMappings[dataRow.Model.GetHierarchicalLevel()];
            var row = new RowPresenter(this, dataRow);
            var ordinal = dataRow.Ordinal;
            rows.Insert(ordinal, row);
            return row;
        }

        internal RowPresenter RowMappings_GetRow(DataRow dataRow)
        {
            Debug.Assert(dataRow != null);
            return RowMappings_GetRow(dataRow.Model.GetHierarchicalLevel(), dataRow.Ordinal);
        }

        private RowPresenter RowMappings_GetRow(int hierarchicalLevel, int ordinal)
        {
            return _rowMappings[hierarchicalLevel][ordinal];
        }

        private void RowMappings_Remove(int hierarchicalLevel, int ordinal)
        {
            DisposeRow(_rowMappings[hierarchicalLevel][ordinal]);
            _rowMappings[hierarchicalLevel].RemoveAt(ordinal);
            if (!IsHierarchical)
                OnSetState(DataPresenterState.Rows);
        }

        protected virtual void DisposeRow(RowPresenter row)
        {
            row.Dispose();
        }

        private List<RowPresenter> _rows;
        public IReadOnlyList<RowPresenter> Rows
        {
            get
            {
                OnGetState(DataPresenterState.Rows);
                return _rows;
            }
        }

        private void Rows_Insert(int index, RowPresenter row)
        {
            Debug.Assert(row != null);

            _rows.Insert(index, row);
            if (IsHierarchical)
            {
                Debug.Assert(row.Index == -1);
                row.Index = index;
            }
        }

        private void Rows_RemoveAt(int index)
        {
            SetPrevCurrentRowIndex(index, 1);
            _rows[index].Index = -1;
            _rows.RemoveAt(index);
        }

        private void Rows_RemoveRange(int startIndex, int count)
        {
            Debug.Assert(count > 0);

            SetPrevCurrentRowIndex(startIndex, count);
            for (int i = 0; i < count; i++)
                _rows[startIndex + i].Index = -1;

            _rows.RemoveRange(startIndex, count);
        }

        private int _prevCurrentRowIndex = -1;
        private void SetPrevCurrentRowIndex(int startRemovalIndex, int count)
        {
            if (_currentRow != null)
            {
                var currentRowViewOrdinal = _currentRow.Index;
                if (currentRowViewOrdinal >= startRemovalIndex && currentRowViewOrdinal < startRemovalIndex + count)
                    _prevCurrentRowIndex = currentRowViewOrdinal;
            }
        }

        private void Rows_UpdateIndex(int startIndex)
        {
            Debug.Assert(IsHierarchical);

            for (int i = startIndex; i < _rows.Count; i++)
                _rows[i].Index = i;
        }

        private void InitializeRowMappings()
        {
            _rowMappings = new List<List<RowPresenter>>();
            for (var dataSet = DataSet; dataSet != null; dataSet = GetChildDataSet(dataSet))
            {
                _rowMappings.Add(new List<RowPresenter>());
                foreach (var dataRow in dataSet)
                    RowMappings_CreateRow(dataRow);

                dataSet.RowAdded += OnDataRowAdded;
                dataSet.RowRemoved += OnDataRowRemoved;
                dataSet.RowUpdated += OnDataRowUpdated;
            }
        }

        private DataSet GetChildDataSet(DataSet dataSet)
        {
            return IsHierarchical && dataSet.Count > 0 ? dataSet.Model.GetChildModels()[Template.HierarchicalModelOrdinal].GetDataSet() : null;
        }

        private void InitializeRows()
        {
            _rows = IsHierarchical ? new List<RowPresenter>() : _rowMappings[0];
            if (IsHierarchical)
            {
                int index = 0;
                foreach (var row in _rowMappings[0])
                    index = InsertHierarchicalRow(index, row);
            }
        }

        private int InsertHierarchicalRow(int index, RowPresenter row)
        {
            Debug.Assert(IsHierarchical && !row.IsEof);

            Rows_Insert(index++, row);
            if (row.IsExpanded)
            {
                var children = row.DataRow[Template.HierarchicalModelOrdinal];
                foreach (var childDataRow in children)
                {
                    var childRow = RowMappings_GetRow(childDataRow);
                    index = InsertHierarchicalRow(index, childRow);
                }
            }
            return index;
        }

        private int GetHierarchicalOrdinal(RowPresenter row)
        {
            Debug.Assert(!row.IsEof);

            if (!IsHierarchical)
                return -1;

            var parentRow = row.HierarchicalParent;
            var prevSiblingRow = PrevSiblingOf(row);
            if (parentRow == null)
                return prevSiblingRow == null ? 0 : NextHierarchicalOrdinalOf(prevSiblingRow);
            else if (parentRow.Index >= 0 && parentRow.IsExpanded)
                return prevSiblingRow == null ? parentRow.Index + 1 : NextHierarchicalOrdinalOf(prevSiblingRow);
            else
                return -1;
        }

        private RowPresenter PrevSiblingOf(RowPresenter row)
        {
            return row.DataRow.Index == 0 ? null : RowMappings_GetRow(row.HierarchicalLevel, row.DataRow.Ordinal - 1);
        }

        private int NextHierarchicalOrdinalOf(RowPresenter row)
        {
            Debug.Assert(IsHierarchical && row != null && row.Index >= 0);

            var hierarchicalLevel = row.HierarchicalLevel;
            var result = row.Index + 1;
            for (; result < _rows.Count; result++)
            {
                if (_rows[result].HierarchicalLevel <= hierarchicalLevel)
                    break;
            }
            return result;
        }

        private void OnDataRowAdded(object sender, DataRow dataRow)
        {
            if (IsHierarchical && dataRow.Model.GetHierarchicalLevel() == _rowMappings.Count - 1)
                _rowMappings.Add(new List<RowPresenter>());

            if (EditingEofRow != null && EditingEofRow.DataRow == dataRow)
                return;

            var row = RowMappings_CreateRow(dataRow);
            var hierarchicalOrdinal = GetHierarchicalOrdinal(row);
            if (hierarchicalOrdinal >= 0)
            {
                hierarchicalOrdinal = InsertHierarchicalRow(hierarchicalOrdinal, row);
                Rows_UpdateIndex(hierarchicalOrdinal);
            }
            else
                OnSetState(DataPresenterState.Rows);
            OnRowsChanged();
        }

        private void OnDataRowRemoved(object sender, DataRowRemovedEventArgs e)
        {
            OnDataRowRemoved(e.Model.GetHierarchicalLevel(), e.Index);
            OnRowsChanged();
        }

        private void OnDataRowRemoved(int hierarchicalLevel, int ordinal)
        {
            if (IsHierarchical)
            {
                var row = RowMappings_GetRow(hierarchicalLevel, ordinal);
                var hierarchicalOrdinal = row.Index;
                if (hierarchicalOrdinal >= 0)
                {
                    Rows_RemoveAt(hierarchicalOrdinal);
                    Rows_UpdateIndex(hierarchicalOrdinal);
                }
            }
            RowMappings_Remove(hierarchicalLevel, ordinal);
        }

        private void OnRowsChanged()
        {
            CoerceEofRow();
            CurrentRow = CoercedCurrentRow;
            OnSetState(DataPresenterState.Rows);
        }

        private void CoerceEofRow()
        {
            if (HasEof)
            {
                if (EofRow == null)
                    AddEofRow();
            }
            else
            {
                var eofRow = EofRow;
                if (eofRow != null)
                    RemoveEofRow(eofRow);
            }
        }

        private bool HasEof
        {
            get
            {
                var eofVisibility = Template.EofVisibility;
                if (eofVisibility == EofVisibility.Never)
                    return false;
                else if (eofVisibility == EofVisibility.NoData)
                    return DataSet.Count == 0;
                else
                    return true;
            }
        }

        private void AddEofRow()
        {
            Debug.Assert(EofRow == null);
            var row = new RowPresenter(this, null);
            Rows_Insert(_rows.Count, row);
        }

        private void RemoveEofRow(RowPresenter eofRow)
        {
            Debug.Assert(eofRow == EofRow);
            Rows_RemoveAt(eofRow.Index);
            eofRow.Dispose();
        }

        private RowPresenter EofRow
        {
            get
            {
                var lastHierarchicalRow = LastHierarchicalRow;
                if (lastHierarchicalRow == null)
                    return null;
                return lastHierarchicalRow.IsEof ? lastHierarchicalRow : null;
            }
        }

        private RowPresenter LastHierarchicalRow
        {
            get { return _rows.Count == 0 ? null : _rows[_rows.Count - 1]; }
        }

        private RowPresenter CoercedCurrentRow
        {
            get
            {
                if (_currentRow == null)
                {
                    if (Rows.Count > 0)
                        return Rows[0];
                }
                else
                {
                    if (_prevCurrentRowIndex != -1)
                    {
                        var currentRowOrdinal = Math.Min(Rows.Count - 1, _prevCurrentRowIndex);
                        _prevCurrentRowIndex = -1;
                        return currentRowOrdinal < 0 ? null : Rows[currentRowOrdinal];
                    }
                    else if (Rows.Count == 0)
                        return null;
                }
                return _currentRow;
            }
        }

        private DataRow _viewUpdateSuppressed;

        internal void SuppressViewUpdate(DataRow dataRow)
        {
            Debug.Assert(_viewUpdateSuppressed == null);
            _viewUpdateSuppressed = dataRow;
        }

        internal void ResumeViewUpdate()
        {
            Debug.Assert(_viewUpdateSuppressed != null);
            _viewUpdateSuppressed = null;
        }

        private void OnDataRowUpdated(object sender, DataRow dataRow)
        {
            if (_viewUpdateSuppressed != dataRow)
            {
                var row = RowMappings_GetRow(dataRow);
                Invalidate(row);
            }
        }

        private RowPresenter _currentRow;
        public virtual RowPresenter CurrentRow
        {
            get
            {
                OnGetState(DataPresenterState.CurrentRow);
                return _currentRow;
            }
            set
            {
                SetCurrentRow(value);
                OnSetState(DataPresenterState.CurrentRow);
            }
        }

        private void SetCurrentRow(RowPresenter value)
        {
            if (_currentRow == value)
                return;

            if (value != null)
            {
                if (value.RowManager != this || value.Index < 0)
                    throw new ArgumentException(Strings.RowManager_InvalidCurrentRow, nameof(value));
            }

            var oldValue = _currentRow;
            if (_currentRow != null)
                _currentRow.IsCurrent = false;

            _currentRow = value;

            if (_currentRow != null)
                _currentRow.IsCurrent = true;
        }

        private HashSet<RowPresenter> _selectedRows = new HashSet<RowPresenter>();
        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get
            {
                OnGetState(DataPresenterState.SelectedRows);
                return _selectedRows;
            }
        }

        internal void AddSelectedRow(RowPresenter row)
        {
            _selectedRows.Add(row);
            OnSetState(DataPresenterState.SelectedRows);
        }

        internal void RemoveSelectedRow(RowPresenter row)
        {
            _selectedRows.Remove(row);
            OnSetState(DataPresenterState.SelectedRows);
        }

        private RowPresenter _editingRow;
        public virtual RowPresenter EditingRow
        {
            get
            {
                OnGetState(DataPresenterState.EditingRow);
                return _editingRow;
            }
            internal set
            {
                _editingRow = value;
                OnSetState(DataPresenterState.EditingRow);
            }
        }

        internal RowPresenter EditingEofRow { get; private set; }

        internal void BeginEditEof()
        {
            var eofRow = EofRow;
            Debug.Assert(eofRow != null);
            EditingEofRow = eofRow;
            EditingEofRow.DataRow = new DataRow();
            DataSet.Add(EditingEofRow.DataRow);
            OnRowsChanged();
            Invalidate(eofRow);
        }

        internal void CancelEditEof()
        {
            Debug.Assert(EditingEofRow != null);

            var eofRow = EofRow;
            EditingEofRow.DataRow = null;
            if (eofRow != null)
            {
                RemoveEofRow(eofRow);
                OnRowsChanged();
            }
            EditingEofRow = null;
        }

        internal void Expand(RowPresenter row)
        {
            Debug.Assert(IsHierarchical && !row.IsExpanded);

            var nextOrdinal = row.Index + 1;
            for (int i = 0; i < row.HierarchicalChildrenCount; i++)
            {
                var childRow = row.GetHierarchicalChild(i);
                nextOrdinal = InsertHierarchicalRow(nextOrdinal, childRow);
            }
            Rows_UpdateIndex(nextOrdinal);
            OnSetState(DataPresenterState.Rows);
        }

        internal void Collapse(RowPresenter row)
        {
            Debug.Assert(IsHierarchical && row.IsExpanded);

            var nextOrdinal = row.Index + 1;
            int count = NextHierarchicalOrdinalOf(row) - nextOrdinal;
            if (count == 0)
                return;

            Rows_RemoveRange(nextOrdinal, count);
            Rows_UpdateIndex(nextOrdinal);
            OnSetState(DataPresenterState.Rows);
        }

        public RowPresenter InsertRow(int ordinal)
        {
            if (ordinal < 0 || ordinal > Rows.Count)
                throw new ArgumentOutOfRangeException(nameof(ordinal));


            var row = ordinal < Rows.Count ? Rows[ordinal] : null;
            if (row != null && row.HierarchicalLevel != 0)
                throw new ArgumentException(Strings.RowManager_OrdinalNotTopLevel, nameof(ordinal));

            var index = row == null ? DataSet.Count : row.DataRow.Index;
            DataSet.Insert(index, new DataRow());
            var result = Rows[ordinal];
            result.BeginEdit(true);
            return result;
        }
    }
}
