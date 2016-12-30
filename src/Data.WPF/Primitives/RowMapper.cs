using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    /// <summary>Handles mapping between <see cref="DataRow"/> and <see cref="RowPresenter"/>, with filter and sort.</summary>
    internal abstract class RowMapper : IDataCriteria
    {
        private abstract class Normalized<T> : IReadOnlyList<T>
            where T : class
        {
            private readonly RowMapper _rowMapper;
            private string _sealizedString;
            private readonly List<T> _list;

            protected Normalized(RowMapper rowMapper, T firstValue)
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

        private sealed class NormalizedWhere : Normalized<_Boolean>
        {
            public NormalizedWhere(RowMapper rowMapper, _Boolean where)
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

        private sealed class NormalizedOrderBy : Normalized<ColumnSort[]>
        {
            public NormalizedOrderBy(RowMapper rowMapper, ColumnSort[] orderBy)
                : base(rowMapper, orderBy)
            {
            }

            protected override string Serialize(Model model, ColumnSort[] first)
            {
                return first.ToJson(false);
            }

            protected override ColumnSort[] Deserialize(string serializedString, Model model)
            {
                return model.ParseOrderBy(serializedString).ToArray();
            }
        }

        protected RowMapper(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
        {
            Debug.Assert(template != null && template.RowManager == null);
            Debug.Assert(dataSet != null);
            _template = template;
            _dataSet = dataSet;
            _normalizedWhere = Normalize(where);
            _normalizedOrderBy = Normalize(orderBy);
            Initialize();
            WireDataChangedEvents();
        }

        private int _maxDepth;

        private void WireDataChangedEvents()
        {
            WireDataChangedEvents(_dataSet);

            if (IsRecursive)
            {
                for (var childDataSet = GetChildDataSet(_dataSet); childDataSet != null; childDataSet = GetChildDataSet(childDataSet))
                {
                    WireDataChangedEvents(childDataSet);
                    _maxDepth++;
                }
            }
        }

        private void WireDataChangedEvents(DataSet dataSet)
        {
            dataSet.RowAdded += OnDataRowAdded;
            dataSet.RowRemoved += OnDataRowRemoved;
            dataSet.RowUpdated += OnDataRowUpdated;
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

        private IReadOnlyList<_Boolean> _normalizedWhere;
        public _Boolean Where
        {
            get { return _normalizedWhere == null ? null : _normalizedWhere[0]; }
        }

        private bool SetWhere(_Boolean where)
        {
            if (Where == where)
                return false;

            _normalizedWhere = Normalize(where);
            return true;
        }

        private IReadOnlyList<_Boolean> Normalize(_Boolean where)
        {
            if (where == null)
                return null;
            else if (IsRecursive)
                return new NormalizedWhere(this, where);
            else
                return new _Boolean[] { where };
        }

        private bool IsQuery
        {
            get { return _normalizedWhere != null || _normalizedOrderBy != null; }
        }

        private IReadOnlyList<ColumnSort[]> _normalizedOrderBy;

        public ColumnSort[] OrderBy
        {
            get { return _normalizedOrderBy == null ? null : _normalizedOrderBy[0]; }
        }

        private bool SetOrderBy(ColumnSort[] orderBy)
        {
            if (Equals(orderBy, OrderBy))
                return false;

            _normalizedOrderBy = Normalize(orderBy);
            return true;
        }

        private static bool Equals(ColumnSort[] x, ColumnSort[] y)
        {
            if (x == y)
                return true;

            if (x == null && y.Length == 0)
                return true;

            return false;
        }

        private IReadOnlyList<ColumnSort[]> Normalize(ColumnSort[] orderBy)
        {
            if (orderBy == null || orderBy.Length == 0)
                return null;
            else if (IsRecursive)
                return new NormalizedOrderBy(this, orderBy);
            else
                return new ColumnSort[][] { orderBy };
        }

        public void Apply(_Boolean where, ColumnSort[] orderBy)
        {
            var whereChanged = SetWhere(where);
            var orderByChanged = SetOrderBy(orderBy);
            if (whereChanged || orderByChanged)
                Reload();
        }

        protected virtual void Reload()
        {
            Initialize();
        }

        private void Initialize()
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
            if (IsRecursive || IsQuery)
                _mappings = new Dictionary<DataRow, RowPresenter>();

            if (!IsRecursive || _normalizedWhere == null)
                return;

            //If any child row fulfills the WHERE predicates, all the rows along the parent chain should be kept
            for (var dataSet = GetChildDataSet(DataSet); dataSet != null; dataSet = GetChildDataSet(dataSet))
            {
                foreach (var dataRow in dataSet)
                {
                    if (ApplyWhere(dataRow))
                        EnsureAncestorsCreated(dataRow);
                }
            }
        }

        private DataSet GetChildDataSet(DataSet dataSet)
        {
            return IsRecursive && dataSet.Count > 0 ? GetChildDataSet(dataSet.Model) : null;
        }

        private DataSet GetChildDataSet(Model model)
        {
            return model.GetChildModels()[Template.RecursiveModelOrdinal].GetDataSet();
        }

        private RowPresenter EnsureAncestorsCreated(DataRow dataRow)
        {
            for (int i = GetDepth(dataRow); i > 0; i--)
            {
                dataRow = dataRow.ParentDataRow;
                RowPresenter row;
                bool exists = CreateIfNotExist(dataRow, out row);
                if (exists)
                    return row;
            }

            return null;
        }

        private RowPresenter GetOrCreate(DataRow dataRow)
        {
            RowPresenter result;
            CreateIfNotExist(dataRow, out result);
            return result;
        }

        private bool CreateIfNotExist(DataRow dataRow, out RowPresenter row)
        {
            if (_mappings.TryGetValue(dataRow, out row))
                return true;
            row = CreateRowPresenter(dataRow);
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
                result.Add(row);
            }
            return result;
        }

        private IEnumerable<DataRow> Filter(IEnumerable<DataRow> dataRows)
        {
            return _normalizedWhere == null ? dataRows : dataRows.Where(x => IsRecursive && _mappings.ContainsKey(x) ? true : ApplyWhere(x));
        }

        private bool ApplyWhere(DataRow dataRow)
        {
            var result = _normalizedWhere[GetDepth(dataRow)][dataRow];
            return result.HasValue && result.GetValueOrDefault();
        }

        private IEnumerable<DataRow> Sort(IEnumerable<DataRow> dataRows, int depth)
        {
            if (_normalizedOrderBy == null)
                return dataRows;

            var orderBy = _normalizedOrderBy[depth];
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

        internal int GetDepth(Model model)
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
            }
        }

        protected virtual void DisposeRow(RowPresenter row)
        {
            row.Dispose();
        }

        private bool IsValid(DataRow dataRow)
        {
            return IsValid(dataRow.Model);
        }

        private bool IsValid(Model model)
        {
            if (model == DataSet.Model)
                return true;

            if (IsRecursive)
            {
                for (var dataSet = GetChildDataSet(DataSet); dataSet != null; dataSet = GetChildDataSet(dataSet))
                {
                    if (model == dataSet.Model)
                        return true;
                }
            }
            return false;
        }

        private bool PassesFilter(DataRow dataRow)
        {
            return _normalizedWhere == null || ApplyWhere(dataRow);
        }

        private void OnDataRowAdded(object sender, DataRow dataRow)
        {
            if (!IsValid(dataRow))
                return;

            if (IsRecursive && GetDepth(dataRow) == _maxDepth)
            {
                var childDataSet = GetChildDataSet(dataRow.Model);
                WireDataChangedEvents(childDataSet);
            }
            Add(dataRow);
        }

        private void Add(DataRow dataRow)
        {
            if (!PassesFilter(dataRow))
                return;

            var row = CreateRowPresenter(dataRow);
            if (_mappings != null)
                _mappings.Add(dataRow, row);

            if (IsRecursive && GetDepth(dataRow) > 0)
            {
                var existedAncestor = EnsureAncestorsCreated(dataRow);
                for (var parentDataRow = row.DataRow.ParentDataRow; parentDataRow != null; parentDataRow = parentDataRow.ParentDataRow)
                {
                    var parentRow = _mappings[parentDataRow];
                    row.Parent = parentRow;
                    if (parentRow == existedAncestor)
                        break;
                    parentRow.InsertChild(0, row);
                    row = row.Parent;
                }
            }

            var index = GetIndex(row, -1);
            InsertAt(index, row);
            OnRowAdded(row, index);
        }

        private void InsertAt(int index, RowPresenter row)
        {
            if (row.Parent == null)
                _rows.Insert(index, row);
            else
                row.Parent.InsertChild(index, row);
        }

        private IReadOnlyList<RowPresenter> GetContainerList(RowPresenter row)
        {
            return row.Parent == null ? _rows : row.Parent.Children;
        }

        private int GetIndex(RowPresenter row, int oldIndex)
        {
            if (!IsQuery)
                return row.DataRow.Index;

            var list = oldIndex < 0 ? GetContainerList(row) : new OneItemExcludedList<RowPresenter>(GetContainerList(row), oldIndex);
            return BinarySearch(list, row);
        }

        private int BinarySearch(IReadOnlyList<RowPresenter> list, RowPresenter row)
        {
            int min = 0;
            int max = list.Count - 1;
            while (min <= max)
            {
                int mid = (min + max) >> 1;
                if (ShouldSwap(list[mid], row))
                    max = mid - 1;
                else
                    min = mid + 1;
            }
            return min;
        }

        private bool ShouldSwap(RowPresenter x, RowPresenter y)
        {
            return ShouldSwap(x.DataRow, y.DataRow);
        }

        private bool ShouldSwap(DataRow x, DataRow y)
        {
            Debug.Assert(GetDepth(x) == GetDepth(y));

            if (_normalizedOrderBy != null)
            {
                var orderBy = _normalizedOrderBy[GetDepth(x)];
                for (int i = 0; i < orderBy.Length; i++)
                {
                    var columnSort = orderBy[i];
                    var compare = columnSort.Column.Compare(x, y);
                    if (compare == 0)
                        continue;

                    if (columnSort.Direction == SortDirection.Descending)
                        compare = compare * -1;
                    return compare == 1;
                }
            }

            return x.Index > y.Index;
        }

        private void OnDataRowRemoved(object sender, DataRowRemovedEventArgs e)
        {
            if (!IsValid(e.Model))
                return;
            var row = this[e];
            if (row != null)
                Remove(row);
        }

        private RowPresenter this[DataRowRemovedEventArgs e]
        {
            get { return _mappings == null ? _rows[e.Index] : this[e.DataRow]; }
        }

        private RowPresenter this[DataRow dataRow]
        {
            get
            {
                if (_mappings == null)
                    return _rows[dataRow.Index];
                RowPresenter result;
                return _mappings.TryGetValue(dataRow, out result) ? result : null;
            }
        }

        private void Remove(RowPresenter row)
        {
            Debug.Assert(row != null);

            if (_mappings != null)
                _mappings.Remove(row.DataRow);

            var parent = row.Parent;
            var index = IndexOf(row);
            RemoveAt(parent, index);

            DisposeRow(row);

            if (parent != null && parent.Children.Count == 0 && !PassesFilter(parent.DataRow))
                Remove(parent);
            else
                OnRowRemoved(parent, index);
        }

        private void RemoveAt(RowPresenter parent, int index)
        {
            if (parent != null)
                parent.RemoveChild(index);
            else
                _rows.RemoveAt(index);
        }

        private int IndexOf(RowPresenter row)
        {
            if (!IsQuery && row.DataRow.Index >= 0)
                return row.DataRow.Index;

            var list = GetContainerList(row);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == row)
                    return i;
            }
            return -1;
        }

        private void OnDataRowUpdated(object sender, DataRow dataRow)
        {
            if (!IsValid(dataRow))
                return;

            var row = this[dataRow];
            if (row == null)
                Add(dataRow);
            else if (!PassesFilter(dataRow))
                Remove(row);
            else
                Update(row);
        }

        private void Update(RowPresenter row)
        {
            var oldIndex = IndexOf(row);
            var newIndex = GetIndex(row, oldIndex);
            if (oldIndex == newIndex)
            {
                OnRowUpdated(row);
                return;
            }

            RemoveAt(row.Parent, oldIndex);
            InsertAt(newIndex, row);
            OnRowMoved(row, oldIndex, newIndex);
        }

        private sealed class OneItemExcludedList<T> : IReadOnlyList<T>
        {
            public OneItemExcludedList(IReadOnlyList<T> baseList, int excludedItemIndex)
            {
                Debug.Assert(baseList != null && excludedItemIndex >= 0 && excludedItemIndex < baseList.Count);
                _baseList = baseList;
                _excludedItemIndex = excludedItemIndex;
            }

            private readonly IReadOnlyList<T> _baseList;
            private readonly int _excludedItemIndex;

            public T this[int index]
            {
                get
                {
                    if (index >= _excludedItemIndex)
                        index++;
                    return _baseList[index];
                }
            }

            public int Count
            {
                get { return _baseList.Count - 1; }
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

        protected virtual void OnRowAdded(RowPresenter row, int index)
        {
        }

        protected virtual void OnRowRemoved(RowPresenter parent, int index)
        {
        }

        protected virtual void OnRowMoved(RowPresenter row, int oldIndex, int newIndex)
        {
        }

        protected virtual void OnRowUpdated(RowPresenter row)
        {
        }
    }
}
