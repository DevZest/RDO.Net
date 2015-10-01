using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>Represents an in-memory collection of data.</summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    public abstract class DataSet<T> : DataSource, IDataSet, IList<DataRow>
        where T : Model, new()
    {
        private sealed class MainDataSet : DataSet<T>
        {
            public MainDataSet(T model)
                : base(model)
            {
                model.SetDataSource(this);
            }

            internal override IDataSet CreateSubDataSet(DataRow parentRow)
            {
                return new SubDataSet(this, parentRow);
            }

            public override bool IsReadOnly
            {
                get { return Model.ParentModel != null; }
            }

            public override int IndexOf(DataRow dataRow)
            {
                if (dataRow == null)
                    return -1;
                if (dataRow.Model != Model)
                    return -1;
                return dataRow.Ordinal;
            }

            public override void RemoveAt(int index)
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var dataRow = this[index];
                if (dataRow.Parent != null)
                    throw Error.NotSupportedByReadOnlyList();
                dataRow.DisposeByMainDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustOrdinal(i);
            }

            public override void Clear()
            {
                if (IsReadOnly)
                    throw Error.NotSupportedByReadOnlyList();

                for (int i = _rows.Count - 1; i >= 0; i--)
                    _rows[i].DisposeByMainDataSet();

                _rows.Clear();
                var columns = Model.Columns;
                foreach (var column in columns)
                    column.ClearRows();
            }

            public override void Insert(int index, DataRow dataRow)
            {
                if (dataRow == null)
                    throw new ArgumentNullException(nameof(dataRow));
                if (IsReadOnly && dataRow.Parent == null)
                    throw Error.NotSupportedByReadOnlyList();
                if (index < 0 || index > Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                dataRow.InitializeByMainDataSet(Model, index);
                _rows.Insert(index, dataRow);
            }
        }

        private sealed class SubDataSet : DataSet<T>
        {
            public SubDataSet(MainDataSet mainDataSet, DataRow parentRow)
                : base(mainDataSet._)
            {
                Debug.Assert(mainDataSet != null);
                _mainDataSet = mainDataSet;
                _parentRow = parentRow;
            }

            internal override IDataSet CreateSubDataSet(DataRow parentRow)
            {
                throw new NotSupportedException();
            }

            private DataSet<T> _mainDataSet;
            private DataRow _parentRow;

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override int IndexOf(DataRow dataRow)
            {
                if (dataRow == null)
                    return -1;

                if (dataRow.Model != Model)
                    return -1;

                return dataRow.ChildOrdinal;
            }

            public override void RemoveAt(int index)
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var dataRow = this[index];
                dataRow.DisposeBySubDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustChildOrdinal(i);

                _mainDataSet.RemoveAt(dataRow.Ordinal);
            }

            public override void Clear()
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    var dataRow = this[i];
                    dataRow.DisposeBySubDataSet();
                    _mainDataSet.RemoveAt(dataRow.Ordinal);
                }
                _rows.Clear();
            }

            public override void Insert(int index, DataRow dataRow)
            {
                if (index < 0 || index > Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                if (dataRow == null)
                    throw new ArgumentNullException(nameof(dataRow));

                if (dataRow.Parent != null)
                    throw new ArgumentException(nameof(dataRow));

                dataRow.InitializeBySubDataSet(_parentRow, index);
                _rows.Insert(index, dataRow);
                _mainDataSet.Insert(GetMainDataSetIndex(dataRow), dataRow);
            }

            private int GetMainDataSetIndex(DataRow dataRow)
            {
                if (_mainDataSet.Count == 0)
                    return 0;

                if (Count > 1)
                {
                    if (dataRow.ChildOrdinal > 0)
                        return this[dataRow.ChildOrdinal - 1].Ordinal + 1;  // after the previous DataRow
                    else
                        return this[dataRow.ChildOrdinal + 1].Ordinal - 1;  // before the next DataRow
                }

                return BinarySearchMainDataSetIndex(_mainDataSet[0], _mainDataSet[_mainDataSet.Count - 1], dataRow.Parent.Ordinal);
            }

            private int BinarySearchMainDataSetIndex(DataRow startRow, DataRow endRow, int parentOrdinal)
            {
                if (parentOrdinal > endRow.Parent.Ordinal)
                    return endRow.Ordinal + 1;  // after the end DataRow

                if (parentOrdinal < startRow.Parent.Ordinal)
                    return startRow.Ordinal;  // before the start DataRow

                var midOrdinal = (startRow.Ordinal + endRow.Ordinal) >> 1;
                var midRow = _mainDataSet[midOrdinal];
                if (parentOrdinal < midRow.Parent.Ordinal)
                    return BinarySearchMainDataSetIndex(startRow, midRow, parentOrdinal);
                else
                    return BinarySearchMainDataSetIndex(midRow, endRow, parentOrdinal);
            }
        }

        internal static DataSet<T> Create(T model)
        {
            return new MainDataSet(model);
        }

        /// <summary>
        /// Creates a new instance of <see cref="DataRow"/> and add to this data set.
        /// </summary>
        /// <returns>The new <see cref="DataRow"/> object.</returns>
        public DataRow AddRow()
        {
            var result = new DataRow();
            this.Add(result);
            return result;
        }

        private DataSet(T model)
            : base(model)
        {
            Debug.Assert(model != null);
            this._ = model;
        }

        /// <inheritdoc/>
        public sealed override DataSourceKind Kind
        {
            get { return DataSourceKind.DataSet; }
        }

        IDataSet IDataSet.CreateSubDataSet(DataRow parentRow)
        {
            return CreateSubDataSet(parentRow);
        }

        internal abstract IDataSet CreateSubDataSet(DataRow parentRow);

        private readonly List<DataRow> _rows = new List<DataRow>();

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count
        {
            get { return _rows.Count; }
        }

        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        public abstract bool IsReadOnly { get; }

        /// <summary>Gets or sets the <see cref="DataRow"/> at specified index.</summary>
        /// <param name="index">The zero-based index of the <see cref="DataRow"/> to get or set.</param>
        /// <returns>The <see cref="DataRow"/> at the specified index.</returns>
        public DataRow this[int index]
        {
            get { return _rows[index]; }
            set
            {
                if (this[index] == value)
                    return;

                RemoveAt(index);
                Insert(index, value);
            }
        }

        /// <inheritdoc cref="IList{T}.IndexOf(T)"/>
        public abstract int IndexOf(DataRow dataRow);

        /// <inheritdoc cref="ICollection{T}.Contains(T)"/>
        public bool Contains(DataRow dataRow)
        {
            return IndexOf(dataRow) != -1;
        }

        /// <inheritdoc cref="IList{T}.Insert(int, T)"/>
        public abstract void Insert(int index, DataRow dataRow);

        public virtual bool Remove(DataRow dataRow)
        {
            var index = IndexOf(dataRow);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        /// <inheritdoc cref="IList{T}.RemoveAt(int)"/>
        public abstract void RemoveAt(int index);

        public void Add(DataRow dataRow)
        {
            Insert(Count, dataRow);
        }

        public abstract void Clear();

        public void CopyTo(DataRow[] array, int arrayIndex)
        {
            _rows.CopyTo(array, arrayIndex);
        }

        public IEnumerator<DataRow> GetEnumerator()
        {
            foreach (var dataRow in _rows)
                yield return dataRow;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToJsonString(isPretty: true);
        }

        public string ToJsonString(bool isPretty)
        {
            var result = new StringBuilder();
            BuildJsonString(result);

            if (isPretty)
                return JsonFormatter.PrettyPrint(result.ToString());
            else
                return result.ToString();
        }

        public void BuildJsonString(StringBuilder stringBuilder)
        {
            stringBuilder.Append('[');
            int count = 0;
            foreach (var dataRow in this)
            {
                if (count > 0)
                    stringBuilder.Append(',');
                dataRow.BuildJsonString(stringBuilder);
                count++;
            }
            stringBuilder.Append(']');
        }

        public static DataSet<T> New(Action<T> initializer = null)
        {
            var model = new T();
            if (initializer != null)
                initializer(model);
            return new MainDataSet(model);
        }

        public readonly T _;

        public static DataSet<T> ParseJson(string json, Action<T> initializer = null)
        {
            var result = New(initializer);
            JsonParser.Parse(json, result);
            return result;
        }

        public DataSet<TChild> CreateChild<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<DbQuery<TChild>> childQueryInitializer = null)
            where TChild : Model, new()
        {
            var dataRow = this[dataRowOrdinal];
            var childModel = getChildModel(this._);
            if (childModel.ParentModel != this._)
                throw new ArgumentException(Strings.InvalidChildModelGetter, nameof(getChildModel));

            var childDataSet = (DataSet<TChild>)dataRow[childModel];
            var mappings = childModel.ParentModelColumnMappings;
            var childQuery = GetChildQuery(sourceData, dataRow, mappings);
            if (childQueryInitializer != null)
                childQueryInitializer(childQuery);
            sourceData.DbSession.FillDataSet(childQuery, childDataSet);

            return childDataSet;
        }

        private static DbQuery<TChild> GetChildQuery<TChild>(DbSet<TChild> dbSet, DataRow parentRow, ReadOnlyCollection<ColumnMapping> mappings)
            where TChild : Model, new()
        {
            var childModel = Model.Clone(dbSet._, false);
            var queryBuilder = dbSet.QueryStatement.MakeQueryBuilder(dbSet.DbSession, childModel, false);
            queryBuilder.Where(parentRow, mappings);
            return queryBuilder.ToQuery(childModel);
        }

        public async Task<DataSet<TChild>> CreateChildAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<DbQuery<TChild>> childQueryInitializer, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            var dataRow = this[dataRowOrdinal];
            var childModel = getChildModel(this._);
            if (childModel.ParentModel != this._)
                throw new ArgumentException(Strings.InvalidChildModelGetter, nameof(getChildModel));

            var childDataSet = (DataSet<TChild>)dataRow[childModel];
            var mappings = childModel.ParentModelColumnMappings;
            var childQuery = GetChildQuery(sourceData, dataRow, mappings);
            if (childQueryInitializer != null)
                childQueryInitializer(childQuery);
            await sourceData.DbSession.FillDataSetAsync(childQuery, childDataSet, cancellationToken);

            return childDataSet;
        }

        public Task<DataSet<TChild>> CreateChildAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<DbQuery<TChild>> childQueryInitializer = null)
            where TChild : Model, new()
        {
            return CreateChildAsync(dataRowOrdinal, getChildModel, sourceData, childQueryInitializer, CancellationToken.None);
        }

        public DataSet<TChild> GetChildren<TChild>(Func<T, TChild> getChild)
            where TChild : Model, new()
        {
            Check.NotNull(getChild, nameof(getChild));

            var child = getChild(_);
            return child == null ? null : (DataSet<TChild>)child.DataSource;
        }

        public bool AllowsKeyUpdate(bool value)
        {
            return Model.AllowsKeyUpdate(value);
        }
    }
}
