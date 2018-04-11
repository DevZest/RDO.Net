using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>Handles mapping between <see cref="DataRow"/> and <see cref="RowPresenter"/>, with filtering and sorting.</summary>
    internal abstract class RowMapper
    {
        protected RowMapper(Template template, DataSet dataSet, IReadOnlyList<Column> rowMatchColumns, Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            Debug.Assert(template != null && template.RowManager == null);
            Debug.Assert(dataSet != null);
            _template = template;
            _dataSet = dataSet;
            if (rowMatchColumns != null && rowMatchColumns.Count == 0)
                rowMatchColumns = null;
            _rowMatchColumns = rowMatchColumns;
            if (rowMatchColumns != null)
                _rowMatches = new Dictionary<RowMatch, RowPresenter>();
            Where = where;
            OrderBy = orderBy;
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

        public Predicate<DataRow> Where { get; private set; }

        private bool ApplyWhere(Predicate<DataRow> where)
        {
            var oldValue = Where;
            Where = where;
            return where != oldValue;
        }

        private bool IsQuery
        {
            get { return Where != null || OrderBy != null; }
        }

        public IComparer<DataRow> OrderBy { get; private set; }

        private bool ApplyOrderBy(IComparer<DataRow> orderBy)
        {
            var oldValue = OrderBy;
            OrderBy = orderBy;
            return OrderBy != oldValue;
        }

        public void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy)
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

            if (!IsRecursive || Where == null)
                return;

            //If any child row fulfills the WHERE predicates, all the rows along the parent chain should be kept
            for (var dataSet = GetChildDataSet(DataSet); dataSet != null; dataSet = GetChildDataSet(dataSet))
            {
                foreach (var dataRow in dataSet)
                {
                    if (EvaluateFilter(dataRow))
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
            childDataRows = EvaluateFilter(childDataRows);
            childDataRows = EvaluateSort(childDataRows);
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

        private IEnumerable<DataRow> EvaluateFilter(IEnumerable<DataRow> dataRows)
        {
            return Where == null ? dataRows : dataRows.Where(x => IsRecursive && _mappings.ContainsKey(x) ? true : EvaluateFilter(x));
        }

        private bool EvaluateFilter(DataRow dataRow)
        {
            return Where(dataRow);
        }

        private IEnumerable<DataRow> EvaluateSort(IEnumerable<DataRow> dataRows)
        {
            return OrderBy == null ? dataRows : dataRows.OrderBy(x => x, OrderBy);
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
            var dataRows = EvaluateFilter(DataSet);
            dataRows = EvaluateSort(dataRows);
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
            return Where == null || EvaluateFilter(dataRow);
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

            if (OrderBy != null && OrderBy.Compare(x, y) == 1)
                return true;

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

        internal RowPresenter this[DataRow dataRow]
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

            row.Dispose();
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
            var row = this[dataRow];
            if (row == null)
                Add(dataRow);
            else if (!PassesFilter(dataRow))
                Remove(row);
            else
                Update(row, e);
        }

        private void Update(RowPresenter row, ValueChangedEventArgs e)
        {
            var oldIndex = IndexOf(row);
            var newIndex = GetIndex(row, oldIndex);
            if (oldIndex == newIndex)
            {
                OnRowUpdated(row, e);
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

        protected virtual void OnRowUpdated(RowPresenter row, ValueChangedEventArgs e)
        {
            row.OnValueChanged(e);
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

        private readonly IReadOnlyList<Column> _rowMatchColumns;
        private readonly Dictionary<RowMatch, RowPresenter> _rowMatches;
        internal IReadOnlyList<Column> RowMatchColumns
        {
            get { return _rowMatchColumns; }
        }

        public bool CanMatchRow
        {
            get { return _rowMatches != null; }
        }

        internal void UpdateRowMatch(RowPresenter rowPresenter, int? oldValueHashCode, int? newValueHashCode)
        {
            Debug.Assert(CanMatchRow);
            Debug.Assert(rowPresenter != null);
            Debug.Assert(oldValueHashCode.HasValue || newValueHashCode.HasValue);

            if (oldValueHashCode.HasValue)
                _rowMatches.Remove(new RowMatch(rowPresenter, oldValueHashCode.Value));

            if (newValueHashCode.HasValue)
                _rowMatches.Add(new RowMatch(rowPresenter, newValueHashCode.Value), rowPresenter);
        }

        public RowPresenter this[RowMatch rowMatch]
        {
            get { return _rowMatches == null ? null : _rowMatches.ContainsKey(rowMatch) ? _rowMatches[rowMatch] : null; }
        }

        public RowPresenter Match(RowPresenter rowPresenter)
        {
            if (rowPresenter == null)
                return null;

            if (!rowPresenter.MatchValueHashCode.HasValue)
                return null;
            return this[new RowMatch(rowPresenter, rowPresenter.MatchValueHashCode.Value)];
        }
    }
}
