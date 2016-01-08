using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    partial class DataSetPresenter
    {
        private class RowCollection : IReadOnlyList<DataRowPresenter>
        {
            public RowCollection(DataSetPresenter owner)
            {
                Debug.Assert(owner != null);
                _owner = owner;

                _rows = new List<DataRowPresenter>(DataSet.Count);
                foreach (var dataRow in DataSet)
                    _rows.Add(DataRowPresenter.Create(_owner, dataRow));

                if (_owner.IsEofVisible)
                    _virtualRow = DataRowPresenter.CreateEof(_owner);
                else if (_owner.IsEmptySetVisible && _rows.Count == 0)
                    _virtualRow = DataRowPresenter.CreateEmptySet(_owner);

                DataSet.RowAdded += OnDataRowAdded;
                DataSet.RowRemoved += OnDataRowRemoved;
            }

            #region IReadOnlyList<DataRowPresenter>

            List<DataRowPresenter> _rows;
            DataRowPresenter _virtualRow;

            public IEnumerator<DataRowPresenter> GetEnumerator()
            {
                foreach (var dataRowPresenter in _rows)
                    yield return dataRowPresenter;
                if (_virtualRow != null)
                    yield return _virtualRow;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int IndexOf(DataRowPresenter item)
            {
                return item == null || item.Owner != _owner ? -1 : (item == _virtualRow ? Count - 1 : item.DataRow.Index);
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

            public DataRowPresenter this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    return _virtualRow != null && index == Count - 1 ? _virtualRow : _rows[index];
                }
            }

            #endregion

            private readonly DataSetPresenter _owner;

            private DataSet DataSet
            {
                get { return _owner.DataSet; }
            }

            private void OnDataRowAdded(object sender, DataRowEventArgs e)
            {
                OnDataRowAdded(e.DataRow);
            }

            private void OnDataRowAdded(DataRow dataRow)
            {
                var index = dataRow.Index;

                if (_virtualRow != null && _virtualRow.RowType == RowType.EmptySet)
                {
                    var row = _virtualRow;
                    _virtualRow = null;
                    NotifyRowRemoved(0, row);
                }

                {
                    var row = DataRowPresenter.Create(_owner, dataRow);
                    _rows.Insert(index, row);
                    NotifyRowAdded(index);
                }
            }

            void NotifyRowAdded(int index)
            {
                _owner.OnRowAdded(index);
            }

            void NotifyRowRemoved(int index, DataRowPresenter row)
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
                    _virtualRow = DataRowPresenter.CreateEmptySet(_owner);
                    NotifyRowAdded(0);
                }
            }
        }
    }
}
