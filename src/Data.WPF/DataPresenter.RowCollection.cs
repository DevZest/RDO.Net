using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    partial class DataPresenter
    {
        private class RowCollection : IReadOnlyList<RowPresenter>
        {
            public RowCollection(DataPresenter owner)
            {
                Debug.Assert(owner != null);
                _owner = owner;

                _rows = new List<RowPresenter>(DataSet.Count);
                foreach (var dataRow in DataSet)
                    _rows.Add(RowPresenter.Create(_owner, dataRow));

                if (_owner.IsEofVisible)
                    _virtualRow = RowPresenter.CreateEof(_owner);
                else if (_owner.IsEmptySetVisible && _rows.Count == 0)
                    _virtualRow = RowPresenter.CreateEmptySet(_owner);

                AddRowsChangedListener();
            }

            private void AddRowsChangedListener()
            {
                DataSet.RowAdded += OnDataRowAdded;
                DataSet.RowRemoved += OnDataRowRemoved;
            }

            private void RemoveRowsChangedListener()
            {
                DataSet.RowAdded -= OnDataRowAdded;
                DataSet.RowRemoved -= OnDataRowRemoved;
            }

            #region IReadOnlyList<RowPresenter>

            List<RowPresenter> _rows;
            RowPresenter _virtualRow;

            public IEnumerator<RowPresenter> GetEnumerator()
            {
                foreach (var row in _rows)
                    yield return row;
                if (_virtualRow != null)
                    yield return _virtualRow;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count
            {
                get
                {
                    var result = _rows.Count;
                    if (_virtualRow != null)
                        result++;
                    return result;
                }
            }

            public RowPresenter this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    return _virtualRow != null && index == Count - 1 ? _virtualRow : _rows[index];
                }
            }

            #endregion

            private readonly DataPresenter _owner;

            private DataSet DataSet
            {
                get { return _owner._dataSet; }
            }

            private void OnDataRowAdded(object sender, DataRowEventArgs e)
            {
                OnDataRowAdded(e.DataRow);
            }

            private void OnDataRowAdded(DataRow dataRow)
            {
                var index = dataRow.Index;

                if (_virtualRow != null && _virtualRow.Kind == RowKind.EmptySet)
                {
                    var row = _virtualRow;
                    _virtualRow = null;
                    NotifyRowRemoved(0, row);
                }

                {
                    var row = RowPresenter.Create(_owner, dataRow);
                    _rows.Insert(index, row);
                    NotifyRowAdded(index);
                }
            }

            void NotifyRowAdded(int index)
            {
                _owner.OnRowAdded(index);
            }

            void NotifyRowRemoved(int index, RowPresenter row)
            {
                _owner.OnRowRemoved(index, row);
            }

            private void OnDataRowRemoved(object sender, DataRowRemovedEventArgs e)
            {
                OnDataRowRemoved(e.Index);
            }

            private void OnDataRowRemoved(int index)
            {
                var row = _rows[index];
                _rows[index].Dispose();
                _rows.RemoveAt(index);
                NotifyRowRemoved(index, row);

                if (_rows.Count == 0 && _owner.IsEmptySetVisible)
                {
                    Debug.Assert(_virtualRow == null);
                    _virtualRow = RowPresenter.CreateEmptySet(_owner);
                    NotifyRowAdded(0);
                }
            }

            public void EofToDataRow()
            {
                var eof = _virtualRow;
                Debug.Assert(eof.Kind == RowKind.Eof);

                RemoveRowsChangedListener();

                var dataRow = new DataRow();
                DataSet.Add(dataRow);
                _rows.Add(eof);
                eof.Initialize(dataRow, RowKind.DataRow);
                eof.OnBindingsReset();
                _virtualRow = RowPresenter.CreateEof(_owner);
                NotifyRowAdded(Count - 1);

                AddRowsChangedListener();
            }

            public void DataRowToEof()
            {
                RemoveRowsChangedListener();

                var eof = _virtualRow;
                _virtualRow = null;
                NotifyRowRemoved(Count - 1, eof);

                var index = DataSet.Count - 1;
                var row = this[index];
                DataSet.RemoveAt(index);
                _rows.RemoveAt(index);
                row.Initialize(null, RowKind.Eof);
                _virtualRow = row;
                row.OnBindingsReset();

                AddRowsChangedListener();
            }
        }
    }
}
