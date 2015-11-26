using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;
using System.Windows.Controls;

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
            return item.Owner != this ? -1 : DataSet.IndexOf(item.DataRow);
        }

        public bool Contains(DataRowView item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(DataRowView[] array, int arrayIndex)
        {
            _dataRowViews.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _dataRowViews.Count; }
        }

        public DataRowView this[int index]
        {
            get { return _dataRowViews[index]; }
        }
        #endregion

        private DataSetViewSelection _selection = DataSetViewSelection.Empty;

        private void CoerceSelection()
        {
            _selection.Coerce(DataSet.Count);
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
            get { return _selection; }
        }

        internal bool IsSelected(int index)
        {
            return _selection.IsSelected(index);
        }

        public void Select(int index, SelectionMode selectionMode)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            _selection = _selection.Select(index, selectionMode);
        }
    }
}
