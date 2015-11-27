using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;
using System.Windows.Controls;
using System.Linq;

namespace DevZest.Data.Windows
{
    public sealed partial class DataSetView : IReadOnlyList<DataRowView>
    {
        internal DataSetView(DataRowView owner, GridTemplate template)
        {
            Debug.Assert(template != null);
            Debug.Assert(owner == null || template.Model.GetParentModel() == owner.Model);
            Owner = owner;
            Template = template;

            DataRow parentDataRow = owner != null ? Owner.DataRow : null;
            DataSet = Model[parentDataRow];
            Debug.Assert(DataSet != null);

            _dataRowViews = new List<DataRowView>(DataSet.Count);
            foreach (var dataRow in DataSet)
            {
                var dataRowView = new DataRowView(this, dataRow);
                _dataRowViews.Add(dataRowView);
            }
            if (template.CanAddNew)
                _eof = new DataRowView(this, null);

            DataSet.RowCollectionChanged += OnRowCollectionChanged;
            DataSet.ColumnValueChanged += OnColumnValueChanged;

            CoerceSelection();
        }

        private void OnRowCollectionChanged(object sender, RowCollectionChangedEventArgs e)
        {
            var oldIndex = e.OldIndex;
            var isDelete = oldIndex >= 0;
            if (isDelete)
            {
                _dataRowViews[oldIndex].Dispose();
                _dataRowViews.RemoveAt(oldIndex);
            }
            else
            {
                var dataRow = e.DataRow;
                var dataRowView = new DataRowView(this, dataRow);
                _dataRowViews.Insert(DataSet.IndexOf(dataRow), dataRowView);
            }

            CoerceSelection();
        }

        private void OnColumnValueChanged(object sender, ColumnValueChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public DataRowView Owner { get; private set; }

        public GridTemplate Template { get; private set; }

        public DataSet DataSet { get; private set; }

        public Model Model
        {
            get { return Template.Model; }
        }

        #region IReadOnlyList<DataRowView>

        List<DataRowView> _dataRowViews;
        DataRowView _eof;

        public IEnumerator<DataRowView> GetEnumerator()
        {
            foreach (var dataRowView in _dataRowViews)
                yield return dataRowView;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dataRowViews.GetEnumerator();
        }

        public int IndexOf(DataRowView item)
        {
            return item == null || item.Owner != this ? -1 : (item == _eof ? Count - 1 : DataSet.IndexOf(item.DataRow));
        }

        public int Count
        {
            get
            {
                var result = _dataRowViews.Count;
                if (_eof != null)
                    result++;
                return result;
            }
        }

        public DataRowView this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _eof != null && index == Count - 1 ? _eof : _dataRowViews[index];
            }
        }
        #endregion

        private DataSetViewSelection _selection = DataSetViewSelection.Empty;

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
                if (_eof == null)
                    return _selection;

                var eofIndex = EofIndex;
                if (_selection.IsSelected(eofIndex))
                    return _selection.Where(x => x != eofIndex).ToArray();

                return _selection;
            }
        }

        private int EofIndex
        {
            get
            {
                Debug.Assert(_eof != null);
                return DataSet.Count;
            }
        }

        private bool IsEof(int index)
        {
            return _eof != null && EofIndex == index;
        }

        internal bool IsSelected(int index)
        {
            return !IsEof(index) && _selection.IsSelected(index);
        }

        public void Select(int index, SelectionMode selectionMode)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            _selection = _selection.Select(index, selectionMode);
        }

        private void RefreshScalarItems()
        {
            throw new NotImplementedException();
        }

        private void RefreshSetItems()
        {
            throw new NotImplementedException();
        }
    }
}
