using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    partial class DataView
    {
        private class RowCollection : IReadOnlyList<RowView>
        {
            public RowCollection(DataView owner)
            {
                Debug.Assert(owner != null);
                _owner = owner;

                _rows = new List<RowView>(DataSet.Count);
                foreach (var dataRow in DataSet)
                    _rows.Add(RowView.Create(_owner, dataRow));

                if (_owner.IsEofVisible)
                    _virtualRow = RowView.CreateEof(_owner);
                else if (_owner.IsEmptySetVisible && _rows.Count == 0)
                    _virtualRow = RowView.CreateEmptySet(_owner);

                DataSet.RowAdded += OnDataRowAdded;
                DataSet.RowRemoved += OnDataRowRemoved;
            }

            #region IReadOnlyList<RowView>

            List<RowView> _rows;
            RowView _virtualRow;

            public IEnumerator<RowView> GetEnumerator()
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

            public RowView this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    return _virtualRow != null && index == Count - 1 ? _virtualRow : _rows[index];
                }
            }

            #endregion

            private readonly DataView _owner;

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

                if (_virtualRow != null && _virtualRow.RowType == RowType.EmptySet)
                {
                    var row = _virtualRow;
                    _virtualRow = null;
                    NotifyRowRemoved(0, row);
                }

                {
                    var row = RowView.Create(_owner, dataRow);
                    _rows.Insert(index, row);
                    NotifyRowAdded(index);
                }
            }

            void NotifyRowAdded(int index)
            {
                _owner.OnRowAdded(index);
            }

            void NotifyRowRemoved(int index, RowView row)
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
                    _virtualRow = RowView.CreateEmptySet(_owner);
                    NotifyRowAdded(0);
                }
            }
        }
    }
}
