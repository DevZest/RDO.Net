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

            _dataRowPresenters = new List<DataRowPresenter>(DataSet.Count);
            foreach (var dataRow in DataSet)
            {
                var dataRowPresenter = new DataRowPresenter(this, dataRow);
                _dataRowPresenters.Add(dataRowPresenter);
            }
            if (template.Appendable)
                _eof = new DataRowPresenter(this, null);

            DataSet.RowCollectionChanged += OnRowCollectionChanged;
            DataSet.ColumnValueChanged += OnColumnValueChanged;

            CoerceSelection();

            LayoutManager = new LayoutManager(this);
        }

        internal DataSetView View { get; set; }

        private void OnRowCollectionChanged(object sender, RowCollectionChangedEventArgs e)
        {
            var oldIndex = e.OldIndex;
            var isDelete = oldIndex >= 0;
            if (isDelete)
            {
                _dataRowPresenters[oldIndex].Dispose();
                _dataRowPresenters.RemoveAt(oldIndex);
            }
            else
            {
                var dataRow = e.DataRow;
                var dataRowPresenter = new DataRowPresenter(this, dataRow);
                _dataRowPresenters.Insert(DataSet.IndexOf(dataRow), dataRowPresenter);
            }

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

        List<DataRowPresenter> _dataRowPresenters;
        DataRowPresenter _eof;

        public IEnumerator<DataRowPresenter> GetEnumerator()
        {
            foreach (var dataRowPresenter in _dataRowPresenters)
                yield return dataRowPresenter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dataRowPresenters.GetEnumerator();
        }

        public int IndexOf(DataRowPresenter item)
        {
            return item == null || item.Owner != this ? -1 : (item == _eof ? Count - 1 : DataSet.IndexOf(item.DataRow));
        }

        public int Count
        {
            get
            {
                var result = _dataRowPresenters.Count;
                if (_eof != null)
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

                return _eof != null && index == Count - 1 ? _eof : _dataRowPresenters[index];
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

        internal LayoutManager LayoutManager { get; private set; }
    }
}
