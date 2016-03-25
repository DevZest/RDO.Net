using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowManager
    {
        internal RowManager(DataSet dataSet)
        {
            _template = new Template(this);
            _dataSet = dataSet;
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

        internal virtual void Initialize()
        {
            InitializeRowMappings();
            InitializeHierarchicalRows();
            CoerceValues();
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
            if (BindingSource.Current.RowPresenter == rowPresenter)
                SetStateFlag(rowPresenterState);
        }

        internal void OnSetState(RowPresenter rowPresenter, RowPresenterState rowPresenterState)
        {
            if (GetStateFlag(rowPresenterState))
                Invalidate(rowPresenter);
        }

        internal abstract void Invalidate(RowPresenter rowPresenter);

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

        internal void OnGetState(DataPresenterState dataPresenterState)
        {
            if (BindingSource.Current.RowManager == this)
                SetStateFlag(dataPresenterState);
        }

        internal void OnSetState(DataPresenterState dataPresenterState)
        {
            if (BindingSource.Current.RowManager == this && GetStateFlag(dataPresenterState))
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
            _rowMappings[hierarchicalLevel][ordinal].Dispose();
            _rowMappings[hierarchicalLevel].RemoveAt(ordinal);
            if (!IsHierarchical)
                OnSetState(DataPresenterState.Rows);
        }

        private List<RowPresenter> _hierarchicalRows;

        private void HierarchicalRows_Insert(int ordinal, RowPresenter row)
        {
            Debug.Assert(row != null);

            _hierarchicalRows.Insert(ordinal, row);
            if (IsHierarchical)
            {
                Debug.Assert(row.Ordinal == -1);
                row.Ordinal = ordinal;
            }
            OnSetState(DataPresenterState.Rows);
        }

        private void HierarchicalRows_RemoveAt(int ordinal)
        {
            SetPrevCurrentRowOrdinal(ordinal, 1);
            _hierarchicalRows[ordinal].Ordinal = -1;
            _hierarchicalRows.RemoveAt(ordinal);
            OnSetState(DataPresenterState.Rows);
        }

        private void HierarchicalRows_RemoveRange(int ordinal, int count)
        {
            Debug.Assert(count > 0);

            SetPrevCurrentRowOrdinal(ordinal, count);
            for (int i = 0; i < count; i++)
                _hierarchicalRows[ordinal + i].Ordinal = -1;

            _hierarchicalRows.RemoveRange(ordinal, count);
            OnSetState(DataPresenterState.Rows);
        }

        private int _prevCurrentRowOrdinal = -1;
        private void SetPrevCurrentRowOrdinal(int startRemovalOrdinal, int count)
        {
            if (_currentRow != null)
            {
                var currentRowOrdinal = _currentRow.Ordinal;
                if (currentRowOrdinal >= startRemovalOrdinal && currentRowOrdinal < startRemovalOrdinal + count)
                    _prevCurrentRowOrdinal = currentRowOrdinal;
            }
        }

        private void HierarchicalRows_UpdateOrdinal(int startOrdinal)
        {
            Debug.Assert(IsHierarchical);

            for (int i = startOrdinal; i < _hierarchicalRows.Count; i++)
                _hierarchicalRows[i].Ordinal = i;
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get
            {
                OnGetState(DataPresenterState.Rows);
                return _hierarchicalRows;
            }
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

        private void InitializeHierarchicalRows()
        {
            _hierarchicalRows = IsHierarchical ? new List<RowPresenter>() : _rowMappings[0];
            if (IsHierarchical)
            {
                int hierarchicalOrdinal = 0;
                foreach (var row in _rowMappings[0])
                    hierarchicalOrdinal = InsertHierarchicalRow(hierarchicalOrdinal, row);
            }
        }

        private int InsertHierarchicalRow(int hierarchicalOrdinal, RowPresenter row)
        {
            Debug.Assert(IsHierarchical && !row.IsEof);

            HierarchicalRows_Insert(hierarchicalOrdinal++, row);
            if (row.IsExpanded)
            {
                var children = row.DataRow[Template.HierarchicalModelOrdinal];
                foreach (var childDataRow in children)
                {
                    var childRow = RowMappings_GetRow(childDataRow);
                    hierarchicalOrdinal = InsertHierarchicalRow(hierarchicalOrdinal, childRow);
                }
            }
            return hierarchicalOrdinal;
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
            else if (parentRow.Ordinal >= 0 && parentRow.IsExpanded)
                return prevSiblingRow == null ? parentRow.Ordinal + 1 : NextHierarchicalOrdinalOf(prevSiblingRow);
            else
                return -1;
        }

        private RowPresenter PrevSiblingOf(RowPresenter row)
        {
            return row.DataRow.Index == 0 ? null : RowMappings_GetRow(row.HierarchicalLevel, row.DataRow.Ordinal - 1);
        }

        private int NextHierarchicalOrdinalOf(RowPresenter row)
        {
            Debug.Assert(IsHierarchical && row != null && row.Ordinal >= 0);

            var hierarchicalLevel = row.HierarchicalLevel;
            var result = row.Ordinal + 1;
            for (; result < _hierarchicalRows.Count; result++)
            {
                if (_hierarchicalRows[result].HierarchicalLevel <= hierarchicalLevel)
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
                HierarchicalRows_UpdateOrdinal(hierarchicalOrdinal);
            }
            else
                OnSetState(DataPresenterState.Rows);
            CoerceValues();
        }

        private void OnDataRowRemoved(object sender, DataRowRemovedEventArgs e)
        {
            OnDataRowRemoved(e.Model.GetHierarchicalLevel(), e.Index);
            CoerceValues();
        }

        private void OnDataRowRemoved(int hierarchicalLevel, int ordinal)
        {
            if (IsHierarchical)
            {
                var row = RowMappings_GetRow(hierarchicalLevel, ordinal);
                var hierarchicalOrdinal = row.Ordinal;
                if (hierarchicalOrdinal >= 0)
                {
                    HierarchicalRows_RemoveAt(hierarchicalOrdinal);
                    HierarchicalRows_UpdateOrdinal(hierarchicalOrdinal);
                }
            }
            RowMappings_Remove(hierarchicalLevel, ordinal);
        }

        private void CoerceValues()
        {
            CoerceEofRow();
            CoerceCurrentRow();
        }

        private void CoerceEofRow()
        {
            if (ShouldHaveEofRow)
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

        private bool ShouldHaveEofRow
        {
            get
            {
                var eofRowMapping = Template.EofRowMapping;
                if (IsHierarchical || eofRowMapping == EofRowMapping.Never)
                    return false;
                else if (eofRowMapping == EofRowMapping.NoData)
                    return DataSet.Count == 0;
                else
                    return true;
            }
        }

        private void AddEofRow()
        {
            Debug.Assert(EofRow == null);
            var row = new RowPresenter(this, null);
            HierarchicalRows_Insert(_hierarchicalRows.Count, row);
        }

        private void RemoveEofRow(RowPresenter eofRow)
        {
            Debug.Assert(eofRow == EofRow);
            HierarchicalRows_RemoveAt(eofRow.Ordinal);
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
            get { return _hierarchicalRows.Count == 0 ? null : _hierarchicalRows[_hierarchicalRows.Count - 1]; }
        }

        private void CoerceCurrentRow()
        {
            if (_currentRow == null)
            {
                if (Rows.Count > 0)
                    CurrentRow = Rows[0];
            }
            else
            {
                if (_prevCurrentRowOrdinal != -1)
                {
                    var currentRowOrdinal = Math.Min(Rows.Count - 1, _prevCurrentRowOrdinal);
                    CurrentRow = currentRowOrdinal < 0 ? null : Rows[currentRowOrdinal];
                    _prevCurrentRowOrdinal = -1;
                }
                else if (Rows.Count == 0)
                    CurrentRow = null;
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
        public RowPresenter CurrentRow
        {
            get
            {
                OnGetState(DataPresenterState.CurrentRow);
                return _currentRow;
            }
            set
            {
                if (_currentRow == value)
                    return;

                if (value != null)
                {
                    if (value.RowManager != this || value.Ordinal < 0)
                        throw new ArgumentException(Strings.RowManager_InvalidCurrentRow, nameof(value));
                }

                var oldValue = _currentRow;
                if (_currentRow != null)
                    _currentRow.IsCurrent = false;

                _currentRow = value;

                if (_currentRow != null)
                    _currentRow.IsCurrent = true;

                OnSetState(DataPresenterState.CurrentRow);
            }
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
            CoerceValues();
            Invalidate(eofRow);
        }

        internal void CancelEditEof()
        {
            Debug.Assert(EditingEofRow != null);

            var eofRow = EofRow;
            EditingEofRow.DataRow = null;
            if (eofRow != null)
                RemoveEofRow(eofRow);
            CoerceValues();
            EditingEofRow = null;
        }

        internal void Expand(RowPresenter row)
        {
            Debug.Assert(IsHierarchical && !row.IsExpanded);

            var nextOrdinal = row.Ordinal + 1;
            for (int i = 0; i < row.HierarchicalChildrenCount; i++)
            {
                var childRow = row.GetHierarchicalChild(i);
                nextOrdinal = InsertHierarchicalRow(nextOrdinal, childRow);
            }
            HierarchicalRows_UpdateOrdinal(nextOrdinal);
        }

        internal void Collapse(RowPresenter row)
        {
            Debug.Assert(IsHierarchical && row.IsExpanded);

            var nextOrdinal = row.Ordinal + 1;
            int count = NextHierarchicalOrdinalOf(row) - nextOrdinal;
            if (count == 0)
                return;

            HierarchicalRows_RemoveRange(nextOrdinal, count);
            HierarchicalRows_UpdateOrdinal(nextOrdinal);
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
