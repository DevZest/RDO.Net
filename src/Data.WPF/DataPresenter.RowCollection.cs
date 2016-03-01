using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    partial class DataPresenter
    {
        /// <summary>This class manages mappings between RowPresenter and DataRow objects.
        /// A special dummy RowPresenter is maintained for EOF or empty DataSet.</summary>
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
                    _dummyRow = RowPresenter.CreateEof(_owner);
                else if (_owner.IsEmptySetVisible && _rows.Count == 0)
                    _dummyRow = RowPresenter.CreateEmptySet(_owner);

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

            List<RowPresenter> _rows;   // 1 on 1 mapping to DataRows
            RowPresenter _dummyRow; // EOF or empty DataSet

            public IEnumerator<RowPresenter> GetEnumerator()
            {
                foreach (var row in _rows)
                    yield return row;
                if (_dummyRow != null)
                    yield return _dummyRow;
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
                    if (_dummyRow != null)
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

                    return _dummyRow != null && index == Count - 1 ? _dummyRow : _rows[index];
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

                if (_dummyRow != null && _dummyRow.Kind == RowKind.EmptySet)
                {
                    var row = _dummyRow;
                    _dummyRow = null;
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
                    Debug.Assert(_dummyRow == null);
                    _dummyRow = RowPresenter.CreateEmptySet(_owner);
                    NotifyRowAdded(0);
                }
            }

            public void EofToDataRow()
            {
                var eof = _dummyRow;
                Debug.Assert(eof.Kind == RowKind.Eof);

                RemoveRowsChangedListener();

                var dataRow = new DataRow();
                DataSet.Add(dataRow);
                _rows.Add(eof);
                eof.Initialize(dataRow, RowKind.DataRow);
                eof.OnBindingsReset();
                _dummyRow = RowPresenter.CreateEof(_owner);
                NotifyRowAdded(Count - 1);

                AddRowsChangedListener();
            }

            public void DataRowToEof()
            {
                RemoveRowsChangedListener();

                var eof = _dummyRow;
                _dummyRow = null;
                NotifyRowRemoved(Count - 1, eof);

                var index = DataSet.Count - 1;
                var row = this[index];
                DataSet.RemoveAt(index);
                _rows.RemoveAt(index);
                row.Initialize(null, RowKind.Eof);
                _dummyRow = row;
                row.OnBindingsReset();

                AddRowsChangedListener();
            }
        }
    }
}
