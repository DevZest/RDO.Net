using DevZest.Data.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    /// <summary>Handles mapping between <see cref="DataRow"/> and <see cref="RowPresenter"/>, with filter and sort.</summary>
    internal abstract class RowMapper
    {
        private abstract class Recursive<T> : IReadOnlyList<T>
            where T : class
        {
            private readonly RowMapper _rowMapper;
            private string _sealizedString;
            private readonly List<T> _list;

            protected Recursive(RowMapper rowMapper, T firstValue)
            {
                _rowMapper = rowMapper;
                _list = new List<T>();
                _list.Add(firstValue);
            }

            private Model RootModel
            {
                get { return _rowMapper.DataSet.Model; }
            }

            private int RecursiveModelOrdinal
            {
                get { return _rowMapper.Template.RecursiveModelOrdinal; }
            }

            public T this[int index]
            {
                get
                {
                    for (int i = _list.Count; i <= index; i++)
                        _list.Add(null);

                    if (_list[index] == null)
                    {
                        if (_sealizedString == null)
                            _sealizedString = Serialize(RootModel, _list[0]);
                        _list[index] = Deserialize(_sealizedString, GetChildModel(index));
                    }

                    return _list[index];
                }
            }

            private Model GetChildModel(int index)
            {
                Debug.Assert(index > 0);
                var result = _rowMapper.DataSet.Model;
                for (int i = 1; i <= index; i++)
                    result = result.GetChildModels()[RecursiveModelOrdinal];
                return result;
            }

            protected abstract string Serialize(Model model, T first);

            protected abstract T Deserialize(string serializedString, Model model);

            public int Count
            {
                get { return _list.Count; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private sealed class RecursiveWhere : Recursive<_Boolean>
        {
            public RecursiveWhere(RowMapper rowMapper, _Boolean where)
                : base(rowMapper, where)
            {
            }

            protected override string Serialize(Model model, _Boolean first)
            {
                return first.ToJson(false);
            }

            protected override _Boolean Deserialize(string serializedString, Model model)
            {
                return Column.ParseJson<_Boolean>(model, serializedString);
            }
        }

        private sealed class RecursiveOrderBy : Recursive<ColumnSort[]>
        {
            public RecursiveOrderBy(RowMapper rowMapper, ColumnSort[] orderBy)
                : base(rowMapper, orderBy)
            {
            }

            protected override string Serialize(Model model, ColumnSort[] first)
            {
                return Data.OrderBy.ToJson(first, false);
            }

            protected override ColumnSort[] Deserialize(string serializedString, Model model)
            {
                return Data.OrderBy.ParseJson(model, serializedString);
            }
        }

        protected RowMapper(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
        {
            Debug.Assert(template != null && template.RowManager == null);
            Debug.Assert(dataSet != null);
            _template = template;
            _dataSet = dataSet;
            dataSet.RowAdded += OnDataRowAdded;
            dataSet.RowRemoved += OnDataRowRemoved;
            dataSet.RowUpdated += OnDataRowUpdated;
            _where = MakeRecursive(where);
            _orderBy = MakeRecursive(orderBy);
            Initialize();
        }

        private readonly Template _template;
        public Template Template
        {
            get { return _template; }
        }

        public bool IsRecursive
        {
            get { return Template.IsRecursive; }
        }

        private readonly DataSet _dataSet;
        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        private IReadOnlyList<_Boolean> _where;
        public _Boolean Where
        {
            get { return _where == null ? null : _where[0]; }
        }

        private bool SetWhere(_Boolean where)
        {
            var value = MakeRecursive(where);
            if (_where == value)
                return false;

            _where = value;
            return true;
        }

        private IReadOnlyList<_Boolean> MakeRecursive(_Boolean where)
        {
            if (where == null)
                return null;
            else if (IsRecursive)
                return new RecursiveWhere(this, where);
            else
                return new _Boolean[] { where };
        }

        public void Query(_Boolean where)
        {
            var changed = SetWhere(where);
            if (changed)
                Initialize();
        }

        private IReadOnlyList<ColumnSort[]> _orderBy;

        public IReadOnlyList<ColumnSort> OrderBy
        {
            get { return _orderBy == null ? null : _orderBy[0]; }
        }

        private bool SetOrderBy(ColumnSort[] orderBy)
        {
            var value = MakeRecursive(orderBy);
            if (_orderBy == value)
                return false;

            _orderBy = value;
            return true;
        }

        private IReadOnlyList<ColumnSort[]> MakeRecursive(ColumnSort[] orderBy)
        {
            if (orderBy == null || orderBy.Length == 0)
                return null;
            else if (IsRecursive)
                return new RecursiveOrderBy(this, orderBy);
            else
                return new ColumnSort[][] { orderBy };
        }

        public void Query(ColumnSort[] orderBy)
        {
            var changed = SetOrderBy(orderBy);
            if (changed)
                Initialize();
        }

        public void Query(_Boolean where, ColumnSort[] orderBy)
        {
            var whereChanged = SetWhere(where);
            var orderByChanged = SetOrderBy(orderBy);
            if (whereChanged || orderByChanged)
                Initialize();
        }

        protected virtual void Initialize()
        {
            InitializeMappings();
            InitializeRows();
        }

        /// <summary>Mapping between <see cref="DataRow"/> and <see cref="RowPresenter"/></summary>
        private Dictionary<DataRow, RowPresenter> _mappings;

        private RowPresenter CreateRowPresenter(DataRow dataRow)
        {
            return new RowPresenter(this, dataRow);
        }

        private void InitializeMappings()
        {
            if (IsRecursive || _where != null || _orderBy != null)
                _mappings = new Dictionary<DataRow, RowPresenter>();

            if (!IsRecursive || _where == null)
                return;

            //If any child row fulfills the WHERE predicates, all the rows along the parent chain should be kept
            for (var dataSet = GetChildDataSet(DataSet); dataSet != null; dataSet = GetChildDataSet(dataSet))
            {
                foreach (var dataRow in dataSet)
                {
                    if (PassesFilter(dataRow))
                        MapAncestors(dataRow);
                }
            }
        }

        private DataSet GetChildDataSet(DataSet dataSet)
        {
            return IsRecursive && dataSet.Count > 0 ? dataSet.Model.GetChildModels()[Template.RecursiveModelOrdinal].GetDataSet() : null;
        }

        private void MapAncestors(DataRow dataRow)
        {
            for (dataRow = dataRow.ParentDataRow; dataRow != null; dataRow = dataRow.ParentDataRow)
            {
                bool exists = ExistsOrCreate(dataRow);
                if (exists)
                    return;
            }
        }

        private RowPresenter GetOrCreate(DataRow dataRow)
        {
            RowPresenter result;
            if (_mappings.TryGetValue(dataRow, out result))
                return result;
            result = CreateRowPresenter(dataRow);
            _mappings.Add(dataRow, result);
            return result;
        }

        private bool ExistsOrCreate(DataRow dataRow)
        {
            if (_mappings.ContainsKey(dataRow))
                return true;
            var row = CreateRowPresenter(dataRow);
            _mappings.Add(dataRow, row);
            return false;
        }

        internal List<RowPresenter> GetOrCreateChildren(RowPresenter parent)
        {
            var parentDataRow = parent.DataRow;
            IEnumerable<DataRow> childDataRows = parentDataRow[Template.RecursiveModelOrdinal];
            childDataRows = Filter(childDataRows);
            childDataRows = Sort(childDataRows, GetDepth(parentDataRow) + 1);
            List<RowPresenter> result = null;
            foreach (var childDataRow in childDataRows)
            {
                if (result == null)
                    result = new List<RowPresenter>();
                var row = GetOrCreate(childDataRow);
                row.Parent = parent;
                result.Add(GetOrCreate(childDataRow));
            }
            return result;
        }

        private IEnumerable<DataRow> Filter(IEnumerable<DataRow> dataRows)
        {
            return _where == null ? dataRows : dataRows.Where(x => IsRecursive && _mappings.ContainsKey(x) ? true : PassesFilter(x));
        }

        private bool PassesFilter(DataRow dataRow)
        {
            var result = _where[GetDepth(dataRow)][dataRow];
            return result.HasValue && result.GetValueOrDefault();
        }

        private IEnumerable<DataRow> Sort(IEnumerable<DataRow> dataRows, int depth)
        {
            if (_orderBy == null)
                return dataRows;

            var orderBy = _orderBy[depth];
            var column = orderBy[0].Column;
            var direction = orderBy[0].Direction;
            IOrderedEnumerable<DataRow> result = direction == SortDirection.Descending ? dataRows.OrderByDescending(x => x, column) : dataRows.OrderBy(x => x, column);
            for (int i = 1; i < orderBy.Length; i++)
            {
                column = orderBy[i].Column;
                direction = orderBy[i].Direction;
                result = direction == SortDirection.Descending ? result.ThenByDescending(x => x, column) : result.ThenBy(x => x, column);
            }
            return result;
        }

        internal int GetDepth(DataRow dataRow)
        {
            return GetDepth(dataRow.Model);
        }

        private int GetDepth(Model model)
        {
            return model.GetDepth() - DataSet.Model.GetDepth();
        }

        private List<RowPresenter> _rows;

        public virtual IReadOnlyList<RowPresenter> Rows
        {
            get { return _rows; }
        }

        private void InitializeRows()
        {
            _rows = new List<RowPresenter>();
            var dataRows = Filter(DataSet);
            dataRows = Sort(dataRows, 0);
            foreach (var dataRow in dataRows)
            {
                var row = _mappings == null ? CreateRowPresenter(dataRow) : GetOrCreate(dataRow);
                _rows.Add(row);
                if (IsRecursive)
                    row.InitializeChildren();
                else
                    row.Index = _rows.Count - 1;
            }
        }

        protected virtual void OnRowsChanged()
        {
            Invalidate(null);
        }

        internal abstract void Invalidate(RowPresenter rowPresenter);

        private void OnDataRowAdded(object sender, DataRow dataRow)
        {
            //if (IsRecursive && dataRow.Model.GetDepth() == _rowMappings.Count - 1)
            //    _rowMappings.Add(new List<RowPresenter>());

            //var row = RowMappings_CreateRow(dataRow);
            //var index = GetIndex(row);
            //if (index >= 0)
            //{
            //    index = InsertRowRecursively(index, row);
            //    FlatRows_UpdateIndex(index);
            //}
            //else
            //    OnRowsChanged();
            //OnDataSetChanged();
        }

        private void OnDataRowRemoved(object sender, DataRowRemovedEventArgs e)
        {
            //OnDataRowRemoved(e.Model.GetDepth(), e.Index);
            //OnDataSetChanged();
        }

        private void OnDataRowRemoved(int depth, int ordinal)
        {
            //if (IsRecursive)
            //{
            //    var row = RowMappings_GetRow(depth, ordinal);
            //    var index = row.Index;
            //    if (index >= 0)
            //    {
            //        Rows_RemoveAt(index);
            //        Rows_UpdateIndex(index);
            //    }
            //}
            //RowMappings_Remove(depth, ordinal);
        }

        //private void OnDataSetChanged()
        //{
        //    CoerceEmptyRow();
        //    CurrentRow = CoercedCurrentRow;
        //    OnRowsChanged();
        //}

        private void OnDataRowUpdated(object sender, DataRow dataRow)
        {
            //if (_viewUpdateSuppressed != dataRow)
            //{
            //    var row = RowMappings_GetRow(dataRow);
            //    Invalidate(row);
            //}
        }
    }
}
