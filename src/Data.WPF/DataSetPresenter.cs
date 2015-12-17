using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;
using System.Windows.Controls;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed partial class DataSetPresenter : IReadOnlyList<DataRowPresenter>
    {
        internal DataSetPresenter(DataRowPresenter owner, GridTemplate template)
        {
            Debug.Assert(template != null);
            Debug.Assert(owner == null || template.Model.GetParentModel() == owner.Model);
            Owner = owner;
            Template = template;

            DataRow parentDataRow = owner != null ? Owner.DataRow : null;
            DataSet = Model[parentDataRow];
            Debug.Assert(DataSet != null);

            _rows = new List<DataRowPresenter>(DataSet.Count);
            foreach (var dataRow in DataSet)
            {
                var dataRowPresenter = new DataRowPresenter(this, dataRow);
                _rows.Add(dataRowPresenter);
            }

            if (template.ShowsEof)
                _virtualRow = new DataRowPresenter(this, DataViewRowType.Eof);
            else
                CoerceEmptyDataRow();
            CoerceSelection();
            LayoutManager = new LayoutManager(this);

            DataSet.RowCollectionChanged += OnRowCollectionChanged;
            DataSet.ColumnValueChanged += OnColumnValueChanged;
        }

        internal DataSetView View { get; set; }

        private void OnRowCollectionChanged(object sender, RowCollectionChangedEventArgs e)
        {
            var oldIndex = e.OldIndex;
            var isDelete = oldIndex >= 0;
            if (isDelete)
            {
                _rows[oldIndex].Dispose();
                _rows.RemoveAt(oldIndex);
            }
            else
            {
                var dataRow = e.DataRow;
                var dataRowPresenter = new DataRowPresenter(this, dataRow);
                _rows.Insert(DataSet.IndexOf(dataRow), dataRowPresenter);
            }

            CoerceEmptyDataRow();
            CoerceSelection();
        }

        private void OnColumnValueChanged(object sender, ColumnValueChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public DataRowPresenter Owner { get; private set; }

        public GridTemplate Template { get; private set; }

        public DataSet DataSet { get; private set; }

        public Model Model
        {
            get { return Template.Model; }
        }

        #region IReadOnlyList<DataRowPresenter>

        List<DataRowPresenter> _rows;
        DataRowPresenter _virtualRow;

        private void CoerceEmptyDataRow()
        {
            if (_virtualRow != null && _virtualRow.RowType == DataViewRowType.Eof)
                return;

            if (_rows.Count == 0)
            {
                if (Template.ShowsEmptyDataRow && _virtualRow == null)
                    _virtualRow = new DataRowPresenter(this, DataViewRowType.EmptyDataRow);
            }
            else if (_virtualRow != null)
                _virtualRow = null;
        }

        public IEnumerator<DataRowPresenter> GetEnumerator()
        {
            foreach (var dataRowPresenter in _rows)
                yield return dataRowPresenter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        public int IndexOf(DataRowPresenter item)
        {
            return item == null || item.Owner != this ? -1 : (item == _virtualRow ? Count - 1 : DataSet.IndexOf(item.DataRow));
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

        private Selection _selection = Windows.Selection.Empty;

        private void CoerceSelection()
        {
            _selection.Coerce(Count);
        }

        public int Current
        {
            get { return _selection.Current; }
        }

        internal bool IsCurrent(int index)
        {
            return index != -1 && Current == index;
        }

        public IReadOnlyList<int> Selection
        {
            get
            {
                if (_virtualRow == null)
                    return _selection;

                var virtualRowIndex = VirtualRowIndex;
                if (_selection.IsSelected(virtualRowIndex))
                    return _selection.Where(x => x != virtualRowIndex).ToArray();

                return _selection;
            }
        }

        private int VirtualRowIndex
        {
            get
            {
                Debug.Assert(_virtualRow != null);
                return DataSet.Count;
            }
        }

        private bool IsVirtualRow(int index)
        {
            return _virtualRow != null && VirtualRowIndex == index;
        }

        internal bool IsSelected(int index)
        {
            return !IsVirtualRow(index) && _selection.IsSelected(index);
        }

        public void Select(int index, SelectionMode selectionMode)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            _selection = _selection.Select(index, selectionMode);
        }

        internal LayoutManager LayoutManager { get; private set; }
    }
}
