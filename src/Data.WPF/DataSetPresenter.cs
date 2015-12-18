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

            _rows = new DataRowPresenterCollection(this);
            LayoutManager = new LayoutManager(this);

            DataSet.ColumnValueChanged += OnColumnValueChanged;
        }

        internal DataSetView View { get; set; }

        internal void OnRowCollectionChanged(int index, bool isDelete)
        {
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

        DataRowPresenterCollection _rows;

        public IEnumerator<DataRowPresenter> GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        public int IndexOf(DataRowPresenter item)
        {
            return _rows.IndexOf(item);
        }

        public int Count
        {
            get { return _rows.Count; }
        }

        public DataRowPresenter this[int index]
        {
            get { return _rows[index]; }
        }
        #endregion

        public int Current
        {
            get { return _rows.Current; }
        }

        public IReadOnlyList<int> Selection
        {
            get { return _rows.Selection; }
        }

        internal bool IsSelected(DataRowPresenter row)
        {
            Debug.Assert(row.Owner == this);
            return _rows.IsSelected(row);
        }

        public void Select(int index, SelectionMode selectionMode)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _rows.Select(index, selectionMode);
        }

        internal LayoutManager LayoutManager { get; private set; }
    }
}
