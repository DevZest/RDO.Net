using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;
using System.Windows.Controls;
using System.Linq;

namespace DevZest.Data.Windows
{
    public sealed partial class DataSetPresenter : IReadOnlyList<DataRowPresenter>
    {
        public static DataSetPresenter Create<T>(DataSet<T> dataSet, Action<DataSetPresenterBuilder, T> builder = null)
            where T : Model, new()
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            var model = dataSet._;
            var result = new DataSetPresenter(null, dataSet);
            using (var presenterBuilder = new DataSetPresenterBuilder(result))
            {
                if (builder != null)
                    builder(presenterBuilder, model);
                else
                    DefaultBuilder(presenterBuilder, model);
            }

            return result;
        }

        private static void DefaultBuilder(DataSetPresenterBuilder builder, Model model)
        {
            var columns = model.GetColumns();
            if (columns.Count == 0)
                return;

            builder.AddGridColumns(columns.Select(x => "Auto").ToArray())
                .AddGridRows("Auto", "Auto")
                .Range(0, 1, columns.Count - 1, 1).Repeat();

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                //builder.Range(i, 0).ScalarEntry<ColumnHeader>(t => { })
                //    .Range(i, 1).BeginListEntry<TextBlock>().Bind(e => e.Text = e.GetColumnValue(column).ToString()).End();
            }
        }

        private DataSetPresenter(DataRowPresenter owner, DataSet dataSet)
        {
            Debug.Assert(dataSet != null);
            Debug.Assert(dataSet.ParentRow == null || dataSet.ParentRow == owner.DataRow);

            Owner = owner;
            DataSet = dataSet;
            Template = new GridTemplate(this);
            IsVirtualizing = true;

            DataRow parentDataRow = owner != null ? Owner.DataRow : null;
            DataSet = Model[parentDataRow];
            Debug.Assert(DataSet != null);

            _rows = new DataRowPresenterCollection(this);
            LayoutManager = LayoutManager.Create(this);

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
            get { return DataSet.Model; }
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

        public bool IsVirtualizing { get; internal set; }

        public bool IsEofVisible { get; internal set; }

        public bool IsEmptySetVisible { get; internal set; }
    }
}
