using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class RowManager : IReadOnlyList<RowPresenter>
    {
        internal RowManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
        {
            Debug.Assert(template != null && template.RowManager == null);
            Debug.Assert(dataSet != null);
            _template = template;
            _template.RowManager = this;
            _dataSet = dataSet;
            dataSet.RowAdded += OnDataRowAdded;
            dataSet.RowRemoved += OnDataRowRemoved;
            dataSet.RowUpdated += OnDataRowUpdated;
            _where = MakeRecursive(where);
            _orderBy = MakeRecursive(orderBy);
            Initialize();
        }

        private void Initialize()
        {
            RowMappings_Initialize();
            MappedRows_Initialize();
            FlatRows_Initialize();
            Rows_Initialize();
            SetCurrentRow(CoercedCurrentRow);
        }

        private readonly Template _template;
        public Template Template
        {
            get { return _template; }
        }

        private abstract class Recursive<T> : IReadOnlyList<T>
            where T : class
        {
            private readonly RowManager _rowManager;
            private string _sealizedString;
            private readonly List<T> _list;

            protected Recursive(RowManager rowManager, T firstValue)
            {
                _rowManager = rowManager;
                _list = new List<T>();
                _list.Add(firstValue);
            }

            private Model RootModel
            {
                get { return _rowManager.DataSet.Model; }
            }

            private int RecursiveModelOrdinal
            {
                get { return _rowManager.Template.RecursiveModelOrdinal; }
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
                var result = _rowManager.DataSet.Model;
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
            public RecursiveWhere(RowManager rowManager, _Boolean where)
                : base(rowManager, where)
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
            public RecursiveOrderBy(RowManager rowManager, ColumnSort[] orderBy)
                : base(rowManager, orderBy)
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

        public void Select(_Boolean where)
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

        public void Sort(ColumnSort[] orderBy)
        {
            var changed = SetOrderBy(orderBy);
            if (changed)
                Initialize();
        }

        public void Select(_Boolean where, ColumnSort[] orderBy)
        {
            var whereChanged = SetWhere(where);
            var orderByChanged = SetOrderBy(orderBy);
            if (whereChanged || orderByChanged)
                Initialize();
        }

        private bool IsRecursive
        {
            get { return Template.IsRecursive; }
        }

        internal int GetDepth(DataRow dataRow)
        {
            return GetDepth(dataRow.Model);
        }

        private int GetDepth(Model model)
        {
            return model.GetDepth() - DataSet.Model.GetDepth();
        }

        private readonly DataSet _dataSet;
        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        internal abstract void Invalidate(RowPresenter rowPresenter);

        protected virtual void OnRowsChanged()
        {
            Invalidate(null);
        }

        protected virtual void OnCurrentRowChanged()
        {
            Invalidate(null);
        }

        private void OnEditingRowChanged()
        {
            Invalidate(null);
        }

        private void OnSelectedRowsChanged()
        {
            Invalidate(null);
        }

        protected virtual void DisposeRow(RowPresenter row)
        {
            row.Dispose();
        }

        /// <summary>Mapping between <see cref="DataRow"/> and <see cref="RowPresenter"/></summary>
        private Dictionary<DataRow, RowPresenter> _rowMappings;

        private void RowMappings_Initialize()
        {
            if (IsRecursive || _where != null || _orderBy != null)
                _rowMappings = new Dictionary<DataRow, RowPresenter>();

            if (!IsRecursive || _where == null)
                return;

            //If any child row fulfills the WHERE predicates, all the rows along the parent chain should be kept
            for (var dataSet = GetChildDataSet(DataSet); dataSet != null; dataSet = GetChildDataSet(dataSet))
            {
                foreach (var dataRow in dataSet)
                {
                    if (Predicate(dataRow))
                        RowMappings_InitializeAncestors(dataRow);
                }
            }
        }

        private DataSet GetChildDataSet(DataSet dataSet)
        {
            return IsRecursive && dataSet.Count > 0 ? dataSet.Model.GetChildModels()[Template.RecursiveModelOrdinal].GetDataSet() : null;
        }

        private void RowMappings_InitializeAncestors(DataRow dataRow)
        {
            for (dataRow = dataRow.ParentDataRow; dataRow != null; dataRow = dataRow.ParentDataRow)
            {
                bool exists = RowMappings_ExistsOrCreate(dataRow);
                if (exists)
                    return;
            }
        }

        private RowPresenter RowMappings_GetOrCreate(DataRow dataRow)
        {
            RowPresenter result;
            if (_rowMappings.TryGetValue(dataRow, out result))
                return result;
            result = new RowPresenter(this, dataRow);
            _rowMappings.Add(dataRow, result);
            return result;
        }

        private bool RowMappings_ExistsOrCreate(DataRow dataRow)
        {
            if (_rowMappings.ContainsKey(dataRow))
                return true;
            var row = new RowPresenter(this, dataRow);
            _rowMappings.Add(dataRow, row);
            return false;
        }

        internal IEnumerable<RowPresenter> RowMappings_GetOrCreateChildren(DataRow dataRow)
        {
            IEnumerable<DataRow> childDataRows = dataRow[Template.RecursiveModelOrdinal];
            childDataRows = Filter(childDataRows);
            childDataRows = Sort(childDataRows, GetDepth(dataRow) + 1);
            return childDataRows.Select(x => RowMappings_GetOrCreate(x));
        }

        private List<RowPresenter> _mappedRows;

        private void MappedRows_Initialize()
        {
            _mappedRows = new List<RowPresenter>();
            var dataRows = Filter(DataSet);
            dataRows = Sort(dataRows, 0);
            foreach (var dataRow in dataRows)
            {
                var row =_rowMappings == null ? new RowPresenter(this, dataRow) : RowMappings_GetOrCreate(dataRow);
                _mappedRows.Add(row);
                if (IsRecursive)
                    row.InitializeChildren();
                else
                    row.Index = _mappedRows.Count - 1;
            }
        }

        private IEnumerable<DataRow> Filter(IEnumerable<DataRow> dataRows)
        {
            return _where == null ? dataRows : dataRows.Where(x => IsRecursive && _rowMappings.ContainsKey(x) ? true : Predicate(x));
        }

        private bool Predicate(DataRow dataRow)
        {
            var result = _where[dataRow.Model.GetDepth() - DataSet.Model.GetDepth()][dataRow];
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

        private List<RowPresenter> _flatRows;

        private void FlatRows_Initialize()
        {
            _flatRows = IsRecursive ? new List<RowPresenter>() : _mappedRows;
            if (IsRecursive)
            {
                int index = 0;
                foreach (var row in _mappedRows)
                    FlatRows_Insert(index++, row);
            }
        }

        private void FlatRows_Insert(int index, RowPresenter row)
        {
            Debug.Assert(row != null);

            _flatRows.Insert(index, row);
            row.Index = index;
        }

        internal void Expand(RowPresenter row)
        {
            Debug.Assert(IsRecursive && !row.IsExpanded);

            var nextIndex = row.Index + 1;
            for (int i = 0; i < row.Children.Count; i++)
            {
                var childRow = row.Children[i];
                nextIndex = FlatRows_InsertRecursively(nextIndex, childRow);
            }
            FlatRows_UpdateIndex(nextIndex);
            OnRowsChanged();
        }

        private int FlatRows_InsertRecursively(int index, RowPresenter row)
        {
            Debug.Assert(IsRecursive && !row.IsPlaceholder);

            FlatRows_Insert(index++, row);
            if (row.IsExpanded)
            {
                var children = row.Children;
                foreach (var childRow in children)
                    index = FlatRows_InsertRecursively(index, childRow);
            }
            return index;
        }

        internal void Collapse(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row.IsExpanded);

            var nextIndex = row.Index + 1;
            int count = FlatRows_NextIndexOf(row) - nextIndex;
            if (count == 0)
                return;

            FlatRows_RemoveRange(nextIndex, count);
            FlatRows_UpdateIndex(nextIndex);
            OnRowsChanged();
        }

        private int FlatRows_NextIndexOf(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row != null && row.Index >= 0);

            var depth = row.Depth;
            var result = row.Index + 1;
            for (; result < _flatRows.Count; result++)
            {
                if (_flatRows[result].Depth <= depth)
                    break;
            }
            return result;
        }

        private void FlatRows_RemoveRange(int startIndex, int count)
        {
            Debug.Assert(count > 0);

            for (int i = 0; i < count; i++)
                _flatRows[startIndex + i].Index = -1;

            _flatRows.RemoveRange(startIndex, count);
        }

        private void FlatRows_RemoveAt(int index)
        {
            SaveCurrentRowIndex(index, 1);
            _flatRows[index].Index = -1;
            _flatRows.RemoveAt(index);
        }

        private int _savedCurrentRowIndex = -1;
        private void SaveCurrentRowIndex(int startRemovalIndex, int count)
        {
            if (_currentRow != null)
            {
                var currentRowIndex = _currentRow.Index;
                if (currentRowIndex >= startRemovalIndex && currentRowIndex < startRemovalIndex + count)
                    _savedCurrentRowIndex = currentRowIndex;
            }
        }

        private void FlatRows_UpdateIndex(int startIndex)
        {
            Debug.Assert(_flatRows != _mappedRows);

            for (int i = startIndex; i < _flatRows.Count; i++)
                _flatRows[i].Index = i;
        }

        public RowPresenter Placeholder { get; private set; }

        private int PlaceholderIndex
        {
            get { return Placeholder == null ? -1 : Placeholder.Index; }
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return this; }
        }

        #region IReadOnlyList<RowPresenter>

        private int Rows_Count
        {
            get
            {
                var result = _flatRows.Count;
                if (Placeholder != null)
                    result++;
                return result;
            }
        }

        private RowPresenter Rows_Item(int index)
        {
            Debug.Assert(index >= 0 && index < Rows_Count);

            if (index == PlaceholderIndex)
                return Placeholder;

            if (PlaceholderIndex >= 0 && index > PlaceholderIndex)
                index--;
            return _flatRows[index];
        }

        int IReadOnlyCollection<RowPresenter>.Count
        {
            get { return Rows_Count; }
        }

        RowPresenter IReadOnlyList<RowPresenter>.this[int index]
        {
            get
            {
                if (index < 0 || index >= Rows_Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return Rows_Item(index);
            }
        }

        IEnumerator<RowPresenter> IEnumerable<RowPresenter>.GetEnumerator()
        {
            for (int i = 0; i < Rows_Count; i++)
                yield return Rows_Item(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Rows_Count; i++)
                yield return Rows_Item(i);
        }

        #endregion

        private void Rows_Initialize()
        {
            var rowPlaceholderStrategy = Template.RowPlaceholderStrategy;
            if (rowPlaceholderStrategy == RowPlaceholderStrategy.Top)
                Placeholder = new RowPresenter(this, 0);
            else if (rowPlaceholderStrategy == RowPlaceholderStrategy.Bottom)
                Placeholder = new RowPresenter(this, _flatRows.Count);
            else
                Rows_CoerceNoDataPlaceholder();
        }

        private void Rows_CoerceNoDataPlaceholder()
        {
            if (Template.RowPlaceholderStrategy != RowPlaceholderStrategy.NoData)
                return;

            var index = _flatRows.Count == 0 ? 0 : -1;
            if (index == 0)
            {
                if (Placeholder == null)
                    Placeholder = new RowPresenter(this, 0);
            }
            else
            {
                if (Placeholder != null)
                {
                    var placeholder = Placeholder;
                    Placeholder = null;
                    DisposeRow(placeholder);
                }
            }
        }

        //private void RowMappings_Remove(int depth, int ordinal)
        //{
        //    DisposeRow(_rowMappings[depth][ordinal]);
        //    _rowMappings[depth].RemoveAt(ordinal);
        //    if (!IsQuery)
        //        OnRowsChanged();
        //}

        //private int GetIndex(RowPresenter row)
        //{
        //    Debug.Assert(!row.IsPlaceholder);

        //    if (!IsRecursive)
        //        return -1;

        //    var parentRow = row.Parent;
        //    var prevSiblingRow = PrevSiblingOf(row);
        //    if (parentRow == null)
        //        return prevSiblingRow == null ? 0 : FlatRows_NextIndexOf(prevSiblingRow);
        //    else if (parentRow.Index >= 0 && parentRow.IsExpanded)
        //        return prevSiblingRow == null ? parentRow.Index + 1 : FlatRows_NextIndexOf(prevSiblingRow);
        //    else
        //        return -1;
        //}

        //private RowPresenter PrevSiblingOf(RowPresenter row)
        //{
        //    return row.DataRow.Index == 0 ? null : RowMappings_GetRow(row.Depth, row.DataRow.Ordinal - 1);
        //}

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

        private RowPresenter CoercedCurrentRow
        {
            get
            {
                if (_currentRow == null)
                {
                    if (Rows.Count > 0)
                        return Rows[0];
                }
                else
                {
                    if (_savedCurrentRowIndex != -1)
                    {
                        var currentRowIndex = Math.Min(Rows.Count - 1, _savedCurrentRowIndex);
                        _savedCurrentRowIndex = -1;
                        return currentRowIndex < 0 ? null : Rows[currentRowIndex];
                    }
                    else if (Rows.Count == 0)
                        return null;
                }
                return _currentRow;
            }
        }

        private DataRow _viewUpdateSuppressed;

        internal void SuppressViewUpdate(DataRow dataRow)
        {
            Debug.Assert(_viewUpdateSuppressed == null);
            _viewUpdateSuppressed = dataRow;
        }

        internal void ResumeViewUpdate()
        {
            Debug.Assert(_viewUpdateSuppressed != null);
            _viewUpdateSuppressed = null;
        }

        private void OnDataRowUpdated(object sender, DataRow dataRow)
        {
            //if (_viewUpdateSuppressed != dataRow)
            //{
            //    var row = RowMappings_GetRow(dataRow);
            //    Invalidate(row);
            //}
        }

        private RowPresenter _currentRow;
        public virtual RowPresenter CurrentRow
        {
            get { return _currentRow; }
            set
            {
                SetCurrentRow(value);
                OnCurrentRowChanged();
            }
        }

        private void SetCurrentRow(RowPresenter value)
        {
            if (_currentRow == value)
                return;

            if (value != null)
            {
                if (value.RowManager != this || value.Index < 0)
                    throw new ArgumentException(Strings.RowManager_InvalidCurrentRow, nameof(value));
            }

            var oldValue = _currentRow;
            if (_currentRow != null)
                _currentRow.IsCurrent = false;

            _currentRow = value;

            if (_currentRow != null)
                _currentRow.IsCurrent = true;
        }

        private HashSet<RowPresenter> _selectedRows = new HashSet<RowPresenter>();
        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return _selectedRows; }
        }

        internal void AddSelectedRow(RowPresenter row)
        {
            _selectedRows.Add(row);
            OnSelectedRowsChanged();
        }

        internal void RemoveSelectedRow(RowPresenter row)
        {
            _selectedRows.Remove(row);
            OnSelectedRowsChanged();
        }

        private RowPresenter _editingRow;
        public virtual RowPresenter EditingRow
        {
            get { return _editingRow; }
            internal set
            {
                _editingRow = value;
                OnEditingRowChanged();
            }
        }
    }
}
