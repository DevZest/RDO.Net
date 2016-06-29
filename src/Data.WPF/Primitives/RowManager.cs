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
            AutoBeginEdit = true;
        }

        private readonly Template _template;
        public Template Template
        {
            get { return _template; }
        }

        public bool IsRecursive
        {
            get { return Template.IsRecursive; }
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
            SetStateFlag(dataPresenterState);
        }

        protected virtual void OnSetState(DataPresenterState dataPresenterState)
        {
            if (GetStateFlag(dataPresenterState))
                Invalidate(null);
        }

        private List<List<RowPresenter>> _rowMappings;

        private RowPresenter RowMappings_CreateRow(DataRow dataRow)
        {
            Debug.Assert(dataRow != null);

            var rows = _rowMappings[dataRow.Model.GetDepth()];
            var row = new RowPresenter(this, dataRow);
            var ordinal = dataRow.Ordinal;
            rows.Insert(ordinal, row);
            return row;
        }

        internal RowPresenter RowMappings_GetRow(DataRow dataRow)
        {
            Debug.Assert(dataRow != null);
            return RowMappings_GetRow(dataRow.Model.GetDepth(), dataRow.Ordinal);
        }

        private RowPresenter RowMappings_GetRow(int depth, int ordinal)
        {
            return _rowMappings[depth][ordinal];
        }

        private void RowMappings_Remove(int depth, int ordinal)
        {
            DisposeRow(_rowMappings[depth][ordinal]);
            _rowMappings[depth].RemoveAt(ordinal);
            if (!IsRecursive)
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
            if (IsRecursive)
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
            Debug.Assert(IsRecursive);

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
            return IsRecursive && dataSet.Count > 0 ? dataSet.Model.GetChildModels()[Template.RecursiveModelOrdinal].GetDataSet() : null;
        }

        private void InitializeRows()
        {
            _rows = IsRecursive ? new List<RowPresenter>() : _rowMappings[0];
            if (IsRecursive)
            {
                int index = 0;
                foreach (var row in _rowMappings[0])
                    index = InsertRowRecursively(index, row);
            }
        }

        private int InsertRowRecursively(int index, RowPresenter row)
        {
            Debug.Assert(IsRecursive && !row.IsEof);

            Rows_Insert(index++, row);
            if (row.IsExpanded)
            {
                var children = row.DataRow[Template.RecursiveModelOrdinal];
                foreach (var childDataRow in children)
                {
                    var childRow = RowMappings_GetRow(childDataRow);
                    index = InsertRowRecursively(index, childRow);
                }
            }
            return index;
        }

        private int GetIndex(RowPresenter row)
        {
            Debug.Assert(!row.IsEof);

            if (!IsRecursive)
                return -1;

            var parentRow = row.RecursiveParent;
            var prevSiblingRow = PrevSiblingOf(row);
            if (parentRow == null)
                return prevSiblingRow == null ? 0 : NextIndexOf(prevSiblingRow);
            else if (parentRow.Index >= 0 && parentRow.IsExpanded)
                return prevSiblingRow == null ? parentRow.Index + 1 : NextIndexOf(prevSiblingRow);
            else
                return -1;
        }

        private RowPresenter PrevSiblingOf(RowPresenter row)
        {
            return row.DataRow.Index == 0 ? null : RowMappings_GetRow(row.Depth, row.DataRow.Ordinal - 1);
        }

        private int NextIndexOf(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row != null && row.Index >= 0);

            var depth = row.Depth;
            var result = row.Index + 1;
            for (; result < _rows.Count; result++)
            {
                if (_rows[result].Depth <= depth)
                    break;
            }
            return result;
        }

        private void OnDataRowAdded(object sender, DataRow dataRow)
        {
            if (IsRecursive && dataRow.Model.GetDepth() == _rowMappings.Count - 1)
                _rowMappings.Add(new List<RowPresenter>());

            var row = RowMappings_CreateRow(dataRow);
            var index = GetIndex(row);
            if (index >= 0)
            {
                index = InsertRowRecursively(index, row);
                Rows_UpdateIndex(index);
            }
            else
                OnSetState(DataPresenterState.Rows);
            OnRowsChanged();
        }

        private void OnDataRowRemoved(object sender, DataRowRemovedEventArgs e)
        {
            OnDataRowRemoved(e.Model.GetDepth(), e.Index);
            OnRowsChanged();
        }

        private void OnDataRowRemoved(int depth, int ordinal)
        {
            if (IsRecursive)
            {
                var row = RowMappings_GetRow(depth, ordinal);
                var index = row.Index;
                if (index >= 0)
                {
                    Rows_RemoveAt(index);
                    Rows_UpdateIndex(index);
                }
            }
            RowMappings_Remove(depth, ordinal);
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
                var lastRow = LastRow;
                if (lastRow == null)
                    return null;
                return lastRow.IsEof ? lastRow : null;
            }
        }

        private RowPresenter LastRow
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

        public bool AutoBeginEdit { get; set; }

        internal void Expand(RowPresenter row)
        {
            Debug.Assert(IsRecursive && !row.IsExpanded);

            var nextOrdinal = row.Index + 1;
            for (int i = 0; i < row.RecursiveChildrenCount; i++)
            {
                var childRow = row.GetRecursiveChild(i);
                nextOrdinal = InsertRowRecursively(nextOrdinal, childRow);
            }
            Rows_UpdateIndex(nextOrdinal);
            OnSetState(DataPresenterState.Rows);
        }

        internal void Collapse(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row.IsExpanded);

            var nextOrdinal = row.Index + 1;
            int count = NextIndexOf(row) - nextOrdinal;
            if (count == 0)
                return;

            Rows_RemoveRange(nextOrdinal, count);
            Rows_UpdateIndex(nextOrdinal);
            OnSetState(DataPresenterState.Rows);
        }

        public RowPresenter InsertRow(int ordinal, Action<DataRow> updateAction = null)
        {
            if (ordinal < 0 || ordinal > DataSet.Count)
                throw new ArgumentOutOfRangeException(nameof(ordinal));

            DataSet.Insert(ordinal, new DataRow(), updateAction);
            return _rowMappings[0][ordinal];
        }
    }
}
