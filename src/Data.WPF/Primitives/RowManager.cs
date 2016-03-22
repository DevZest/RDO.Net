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
                InvalidateView();
        }

        internal abstract void InvalidateView();

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
                InvalidateView();
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

        private RowPresenter RowMappings_GetRow(DataRow dataRow)
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
            Debug.Assert(row != null && row.Ordinal == -1);

            row.Ordinal = ordinal;
            _hierarchicalRows.Insert(ordinal, row);
            OnSetState(DataPresenterState.Rows);
        }

        private void HierarchicalRows_RemoveAt(int ordinal)
        {
            _hierarchicalRows[ordinal].Ordinal = -1;
            _hierarchicalRows.RemoveAt(ordinal);
            OnSetState(DataPresenterState.Rows);
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
                foreach (var dataRow in DataSet)
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
            CoerceEofRow();
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

            var parentRow = ParentOf(row);
            var prevRow = PreviousOf(row);
            if (parentRow == null)
                return prevRow == null ? 0 : NextHierarchicalOrdinalOf(prevRow);
            else if (parentRow.Ordinal >= 0 && parentRow.IsExpanded)
                return prevRow == null ? parentRow.Ordinal + 1 : NextHierarchicalOrdinalOf(prevRow);
            else
                return -1;
        }

        private RowPresenter ParentOf(RowPresenter row)
        {
            var parentDataRow = row.DataRow.ParentDataRow;
            return parentDataRow == null ? null : RowMappings_GetRow(parentDataRow);
        }

        private RowPresenter PreviousOf(RowPresenter row)
        {
            var ordinal = row.DataRow.Ordinal;
            return ordinal == 0 ? null : RowMappings_GetRow(row.HierarchicalLevel, ordinal - 1);
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
            CoerceEofRow();
        }

        private void OnDataRowRemoved(object sender, DataRowRemovedEventArgs e)
        {
            OnDataRowRemoved(e.Model.GetHierarchicalLevel(), e.Index);
            CoerceEofRow();
        }

        private void OnDataRowRemoved(int hierarchicalLevel, int ordinal)
        {
            if (IsHierarchical)
            {
                var row = RowMappings_GetRow(hierarchicalLevel, ordinal);
                if (row.Ordinal >= 0)
                {
                    HierarchicalRows_RemoveAt(row.Ordinal);
                    HierarchicalRows_UpdateOrdinal(row.Ordinal);
                }
            }
            RowMappings_Remove(hierarchicalLevel, ordinal);
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
            HierarchicalRows_Insert(_hierarchicalRows.Count - 1, row);
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
            get { return _hierarchicalRows.Count == 0 ? null : _hierarchicalRows[_rowMappings.Count - 1]; }
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
                InvalidateView();
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

                if (value != null && value.RowManager != this)
                    throw new ArgumentException(Strings.RowManager_InvalidCurrentRow, nameof(value));

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

        internal RowPresenter EditingEofRow { get; private set; }

        internal void BeginEditEof()
        {
            var eofRow = EofRow;
            Debug.Assert(eofRow != null);
            EditingEofRow = eofRow;
            EditingEofRow.DataRow = new DataRow();
            DataSet.Add(EditingEofRow.DataRow);
            CoerceEofRow();
            InvalidateView();
        }

        internal void CancelEditEof()
        {
            Debug.Assert(EditingEofRow != null);

            var eofRow = EofRow;
            EditingEofRow.DataRow = null;
            if (eofRow != null)
                RemoveEofRow(eofRow);
            CoerceEofRow();
            EditingEofRow = null;
        }
    }
}
