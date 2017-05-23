using DevZest.Data;
using DevZest.Data.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;

namespace DevZest.Windows.Primitives
{
    /// <summary>Handles mapping between <see cref="DataRow"/> and <see cref="RowPresenter"/>, with filtering and sorting.</summary>
    internal abstract class RowMapper : IDataCriteria
    {
        private sealed class Normalized<T> : IReadOnlyList<T>
            where T : class
        {
            private readonly RowMapper _rowMapper;
            private readonly Func<Model, T> _generator;
            private readonly List<bool> _generatedFlags;
            private readonly List<T> _list;

            public Normalized(RowMapper rowMapper, Func<Model, T> generator)
            {
                Debug.Assert(rowMapper != null);
                Debug.Assert(generator != null);
                _rowMapper = rowMapper;
                _generator = generator;
                _generatedFlags = new List<bool>();
                _list = new List<T>();
                GenerateFirst();
            }

            private void GenerateFirst()
            {
                Debug.Assert(_generatedFlags.Count == 0 && _list.Count == 0);

                var value = _generator(RootModel);
                _generatedFlags.Add(true);
                _list.Add(value);
            }

            private void Generate(int index)
            {
                Debug.Assert(_generatedFlags.Count == _list.Count);
                Debug.Assert(index > 0 && index < _list.Count);
                Debug.Assert(_generatedFlags[index] == false);

                var value = _generator(GetChildModel(index));
                _generatedFlags[index] = true;
                _list[index] = value;
            }

            private Model RootModel
            {
                get { return _rowMapper.RootModel; }
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
                    {
                        _generatedFlags.Add(false);
                        _list.Add(null);
                    }

                    if (_generatedFlags[index] == false)
                        Generate(index);

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

        protected RowMapper(Template template, DataSet dataSet, Func<Model, Column<bool?>> where, Func<Model, ColumnSort[]> orderBy)
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
            WireDataChangedEvents(_dataSet.Model);

            if (IsRecursive)
            {
                for (var childDataSet = GetChildDataSet(_dataSet); childDataSet != null; childDataSet = GetChildDataSet(childDataSet))
                    WireDataChangedEvents(childDataSet.Model);
            }
        }

        private void WireDataChangedEvents(Model model)
        {
            Debug.Assert(_maxDepth == GetDepth(model));
            model.DataRowInserting += OnDataRowInserting;
            model.AfterDataRowInserted += OnAfterDataRowInserted;
            model.DataRowRemoved += OnDataRowRemoved;
            model.ValueChanged += OnValueChanged;
            _maxDepth++;
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

        private Model RootModel
        {
            get { return DataSet.Model; }
        }

        private IReadOnlyList<Column<bool?>> _normalizedWhere;
        public Column<bool?> GetWhere(int depth)
        {
            if (depth < 0 || depth >= _maxDepth)
                throw new ArgumentOutOfRangeException(nameof(depth));

            return _normalizedWhere == null ? null : _normalizedWhere[depth];
        }

        private bool ApplyWhere(Func<Model, Column<bool?>> where)
        {
            var oldValue = _normalizedWhere;
            _normalizedWhere = Normalize(where);
            return _normalizedWhere != oldValue;
        }

        private IReadOnlyList<Column<bool?>> Normalize(Func<Model, Column<bool?>> where)
        {
            if (where == null)
                return null;
            else if (IsRecursive)
                return new Normalized<Column<bool?>>(this, where);
            else
                return new Column<bool?>[] { where(RootModel) };
        }

        private bool IsQuery
        {
            get { return _normalizedWhere != null || _normalizedOrderBy != null; }
        }

        private IReadOnlyList<ColumnSort[]> _normalizedOrderBy;

        public ColumnSort[] GetOrderBy(int depth)
        {
            if (depth < 0 || depth >= _maxDepth)
                throw new ArgumentOutOfRangeException(nameof(depth));
            return _normalizedOrderBy == null ? null : _normalizedOrderBy[depth];
        }

        private bool ApplyOrderBy(Func<Model, ColumnSort[]> orderBy)
        {
            var oldValue = _normalizedOrderBy;
            _normalizedOrderBy = Normalize(orderBy);
            return _normalizedOrderBy != oldValue;
        }

        private IReadOnlyList<ColumnSort[]> Normalize(Func<Model, ColumnSort[]> orderBy)
        {
            if (orderBy == null)
                return null;
            else if (IsRecursive)
                return new Normalized<ColumnSort[]>(this, orderBy);
            else
                return new ColumnSort[][] { orderBy(RootModel) };
        }

        public void Apply(Func<Model, Column<bool?>> where, Func<Model, ColumnSort[]> orderBy)
        {
            var whereChanged = ApplyWhere(where);
            var orderByChanged = ApplyOrderBy(orderBy);
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
            InitializeRowPresenters();
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

        private void InitializeRowPresenters()
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

        private bool PassesFilter(DataRow dataRow)
        {
            return _normalizedWhere == null || ApplyWhere(dataRow);
        }

        private Stack<DataRow> _insertingDataRows = new Stack<DataRow>();

        private void OnDataRowInserting(object sender, DataRowEventArgs e)
        {
            var dataRow = e.DataRow;
            if (IsRecursive && GetDepth(dataRow) == _maxDepth - 1)
            {
                var childDataSet = GetChildDataSet(dataRow.Model);
                WireDataChangedEvents(childDataSet.Model);
            }
            _insertingDataRows.Push(dataRow);
        }

        private void OnAfterDataRowInserted(object sender, DataRowEventArgs e)
        {
            var dataRow = e.DataRow;
            if (_insertingDataRows.Peek() == dataRow)
            {
                Add(dataRow);
                Debug.Assert(_insertingDataRows.Peek() == dataRow);
                _insertingDataRows.Pop();
            }
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
                    Debug.Assert(_insertingDataRows.Peek() == row.DataRow);
                    _insertingDataRows.Pop();
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
            var row = this[e.DataRow, e.Index];
            if (row != null)
                Remove(row);
        }

        private RowPresenter this[DataRow dataRow, int index]
        {
            get { return _mappings == null ? _rows[index] : this[dataRow]; }
        }

        protected RowPresenter this[DataRow dataRow]
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

        private void OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var dataRow= e.DataRow;
            var column = e.Column;
            var row = this[dataRow];
            if (row == null)
                Add(dataRow);
            else if (!PassesFilter(dataRow))
                Remove(row);
            else
                Update(row, column);
        }

        private void Update(RowPresenter row, Column column)
        {
            var oldIndex = IndexOf(row);
            var newIndex = GetIndex(row, oldIndex);
            if (oldIndex == newIndex)
            {
                HandlesRowUpdated(row, column);
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

        private DataPresenter DataPresenter
        {
            get { return Template.DataPresenter; }
        }

        protected void HandlesRowUpdated(RowPresenter row, Column column)
        {
            OnRowUpdated(row);
        }

        protected virtual void OnRowUpdated(RowPresenter row)
        {
        }

        private IEnumerable<DataRow> GetDataRows()
        {
            return IsRecursive ? GetDataRowsRecursively(DataSet) : DataSet;
        }

        private IEnumerable<DataRow> GetDataRowsRecursively(DataSet dataSet)
        {
            Debug.Assert(IsRecursive);
            foreach (var dataRow in dataSet)
                yield return dataRow;

            var childDataSet = GetChildDataSet(dataSet);
            if (childDataSet != null)
            {
                foreach (var childDataRow in GetDataRowsRecursively(childDataSet))
                    yield return childDataRow;
            }
        }
    }
}
