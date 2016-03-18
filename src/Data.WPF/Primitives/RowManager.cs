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

        internal virtual void Initialize()
        {
            _rows = new List<RowPresenter>(DataSet.Count);
            foreach (var dataRow in DataSet)
                InsertRowPresenter(dataRow);
            CoerceEofRow();

            DataSet.RowAdded += OnDataRowAdded;
            DataSet.RowRemoved += OnDataRowRemoved;
            DataSet.RowUpdated += OnDataRowUpdated;
        }

        private readonly Template _template;
        public Template Template
        {
            get { return _template; }
        }

        private readonly DataSet _dataSet;
        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        public Model Model
        {
            get { return DataSet.Model; }
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
            if (BindingSource.Current.RowPresenter == rowPresenter && GetStateFlag(rowPresenterState))
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

        private List<RowPresenter> _rows;
        public IReadOnlyList<RowPresenter> Rows
        {
            get
            {
                OnGetState(DataPresenterState.Rows);
                return _rows;
            }
        }

        private List<RowPresenter> _flattenedRows;
        public IReadOnlyList<RowPresenter> FlattenedRows
        {
            get
            {
                OnGetState(DataPresenterState.FlattenedRows);
                return _flattenedRows;
            }
        }

        private void OnDataRowAdded(object sender, DataRowEventArgs e)
        {
            if (EditingEofRow.DataRow == e.DataRow)
                return;

            InsertRowPresenter(e.DataRow);
            CoerceEofRow();
        }

        private void InsertRowPresenter(DataRow dataRow)
        {
            var rowPresenter = new RowPresenter(this, dataRow);
            var index = dataRow == null ? _rows.Count : dataRow.Index;
            _rows.Insert(index, rowPresenter);
            OnSetState(DataPresenterState.Rows);
        }

        private void OnDataRowRemoved(object sender, DataRowRemovedEventArgs e)
        {
            RemoveRowPresenter(e.Index);
            CoerceEofRow();
        }

        private void RemoveRowPresenter(int index)
        {
            var row = _rows[index];
            _rows[index].Dispose();
            _rows.RemoveAt(index);
            OnSetState(DataPresenterState.Rows);
        }

        private bool ShouldHaveEofRow
        {
            get
            {
                var eofRowStrategy = Template.EofRowStrategy;
                if (eofRowStrategy == EofRowStrategy.Never)
                    return false;
                else if (eofRowStrategy == EofRowStrategy.NoData)
                    return DataSet.Count == 0;
                else
                    return true;
            }
        }

        private RowPresenter LastRow
        {
            get { return _rows.Count == 0 ? null : _rows[_rows.Count - 1]; }
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

        private void CoerceEofRow()
        {
            if (ShouldHaveEofRow)
            {
                if (EofRow == null)
                    InsertRowPresenter(null);
            }
            else
            {
                if (EofRow != null)
                    RemoveRowPresenter(_rows.Count - 1);
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

        private void OnDataRowUpdated(object sender, DataRowEventArgs e)
        {
            if (_viewUpdateSuppressed != e.DataRow)
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
                RemoveRowPresenter(_rows.Count - 1);
            CoerceEofRow();
            EditingEofRow = null;
        }
    }
}
