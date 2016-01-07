using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

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
                {
                    var dataRowPresenter = new DataRowPresenter(_owner, dataRow);
                    _rows.Add(dataRowPresenter);
                }

                if (_owner.IsEofVisible)
                    _virtualRow = new DataRowPresenter(_owner, DataViewRowType.Eof);
                else
                    CoerceEmptySet(false);

                CoerceSelection();
                DataSet.RowAdded += OnRowAdded;
                DataSet.RowRemoved += OnRowRemoved;
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
                return item == null || item.Owner != _owner ? -1 : (item == _virtualRow ? Count - 1 : DataSet.IndexOf(item.DataRow));
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

            private GridTemplate Template
            {
                get { return _owner.Template; }
            }

            private void SetVirtualRow(DataRowPresenter value, bool notifyChange)
            {
                Debug.Assert(_virtualRow != value);

                if (_virtualRow != null && notifyChange)
                    _owner.OnRowsChanged(DataSet.Count, RowCollectionChangeAction.Remove);

                _virtualRow = value;

                if (_virtualRow != null && notifyChange)
                    _owner.OnRowsChanged(DataSet.Count, RowCollectionChangeAction.Add);
            }

            private void CoerceEmptySet(bool notifyChange)
            {
                if (_virtualRow != null && _virtualRow.RowType == DataViewRowType.Eof)
                    return;

                if (_rows.Count == 0)
                {
                    if (_owner.IsEmptySetVisible && _virtualRow == null)
                        SetVirtualRow(new DataRowPresenter(_owner, DataViewRowType.EmptySet), notifyChange);
                }
                else if (_virtualRow != null)
                    SetVirtualRow(null, notifyChange);
            }

            private void OnRowAdded(object sender, DataRowEventArgs e)
            {
                var dataRow = e.DataRow;
                var dataRowPresenter = new DataRowPresenter(_owner, dataRow);
                _rows.Insert(dataRow.Index, dataRowPresenter);
                OnRowsChanged(dataRow.Index, RowCollectionChangeAction.Add);
            }

            private void OnRowRemoved(object sender, DataRowRemovedEventArgs e)
            {
                _rows[e.Index].Dispose();
                _rows.RemoveAt(e.Ordinal);
                OnRowsChanged(e.Index, RowCollectionChangeAction.Remove);
            }

            private void OnRowsChanged(int index, RowCollectionChangeAction action)
            {
                CoerceEmptySet(true);
                CoerceSelection();
                _owner.OnRowsChanged(index, action);
            }

            private Selection _selection = Windows.Selection.Empty;

            private void CoerceSelection()
            {
                _selection = _selection.Coerce(Count);
            }

            public int Current
            {
                get { return _selection.Current; }
            }

            internal bool IsCurrent(int index)
            {
                return index != -1 && Current == index;
            }

            internal IReadOnlyList<int> Selection
            {
                get
                {
                    if (_virtualRow == null)
                        return _selection;

                    var virtualRowIndex = DataSet.Count;
                    if (_selection.IsSelected(virtualRowIndex))
                        return _selection.Where(x => x != virtualRowIndex).ToArray();

                    return _selection;
                }
            }

            internal bool IsSelected(DataRowPresenter row)
            {
                Debug.Assert(row.Owner == _owner);
                return row.RowType != DataViewRowType.DataRow ? false : _selection.IsSelected(IndexOf(row));
            }

            internal void Select(int index, SelectionMode selectionMode)
            {
                Debug.Assert(index >= 0 || index < Count);
                _selection = _selection.Select(index, selectionMode);
            }
        }
    }
}
