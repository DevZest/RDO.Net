using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;
using System.Windows.Controls;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows
{
    public sealed partial class DataSetManager : IReadOnlyList<DataRowManager>
    {
        internal DataSetManager(DataRowManager owner, GridTemplate template)
        {
            Debug.Assert(template != null);
            Debug.Assert(owner == null || template.Model.GetParentModel() == owner.Model);
            Owner = owner;
            Template = template;

            DataRow parentDataRow = owner != null ? Owner.DataRow : null;
            DataSet = Model[parentDataRow];
            Debug.Assert(DataSet != null);

            _dataRowManagers = new List<DataRowManager>(DataSet.Count);
            foreach (var dataRow in DataSet)
            {
                var dataRowManager = new DataRowManager(this, dataRow);
                _dataRowManagers.Add(dataRowManager);
            }
            if (template.CanAddNew)
                _eof = new DataRowManager(this, null);

            DataSet.RowCollectionChanged += OnRowCollectionChanged;
            DataSet.ColumnValueChanged += OnColumnValueChanged;

            CoerceSelection();

            LayoutManager = new LayoutManager(this);
        }

        private void OnRowCollectionChanged(object sender, RowCollectionChangedEventArgs e)
        {
            var oldIndex = e.OldIndex;
            var isDelete = oldIndex >= 0;
            if (isDelete)
            {
                _dataRowManagers[oldIndex].Dispose();
                _dataRowManagers.RemoveAt(oldIndex);
            }
            else
            {
                var dataRow = e.DataRow;
                var dataRowManager = new DataRowManager(this, dataRow);
                _dataRowManagers.Insert(DataSet.IndexOf(dataRow), dataRowManager);
            }

            CoerceSelection();
        }

        private void OnColumnValueChanged(object sender, ColumnValueChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public DataRowManager Owner { get; private set; }

        public GridTemplate Template { get; private set; }

        public DataSet DataSet { get; private set; }

        public Model Model
        {
            get { return Template.Model; }
        }

        #region IReadOnlyList<DataRowManager>

        List<DataRowManager> _dataRowManagers;
        DataRowManager _eof;

        public IEnumerator<DataRowManager> GetEnumerator()
        {
            foreach (var dataRowManager in _dataRowManagers)
                yield return dataRowManager;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dataRowManagers.GetEnumerator();
        }

        public int IndexOf(DataRowManager item)
        {
            return item == null || item.Owner != this ? -1 : (item == _eof ? Count - 1 : DataSet.IndexOf(item.DataRow));
        }

        public int Count
        {
            get
            {
                var result = _dataRowManagers.Count;
                if (_eof != null)
                    result++;
                return result;
            }
        }

        public DataRowManager this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _eof != null && index == Count - 1 ? _eof : _dataRowManagers[index];
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
