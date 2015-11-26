﻿using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;

namespace DevZest.Data.Windows
{
    public sealed class DataSetView : IReadOnlyList<DataRowView>
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
        }

        private void OnColumnValueChanged(object sender, ColumnValueChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnRowCollectionChanged(object sender, RowCollectionChangedEventArgs e)
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
            //set
            //{
            //    if (this[index] == value)
            //        return;

            //    RemoveAt(index);
            //    Insert(index, value);
            //}
        }
        #endregion

        //public bool IsReadOnly
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //public void Insert(int index, DataRowView item)
        //{
        //    if (item == null)
        //        throw new ArgumentNullException(nameof(item));
        //    if (item.Owner != null)
        //        throw new ArgumentException(nameof(item));
        //    if (index < 0 || index >= Count)
        //        throw new ArgumentOutOfRangeException(nameof(item));

        //    var dataRow = new DataRow();
        //    _dataSet.Insert(index, dataRow);
        //    item.Initialize(this, dataRow);
        //    _dataRowViews.Insert(index, item);
        //}

        //public void RemoveAt(int index)
        //{
        //    _dataSet.RemoveAt(index);
        //    _dataRowViews.RemoveAt(index);
        //}

        //public void Add(DataRowView item)
        //{
        //    Insert(Count, item);
        //}

        //public void Clear()
        //{
        //    _dataSet.Clear();
        //    _dataRowViews.Clear();
        //}

        //public bool Remove(DataRowView item)
        //{
        //    var index = IndexOf(item);
        //    if (index == -1)
        //        return false;
        //    RemoveAt(index);
        //    return true;
        //}


    }
}
