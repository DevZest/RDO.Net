using DevZest.Data.Primitives;
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
    public abstract class DataSet<T> : DataSet
        where T : Model, new()
    {
        private sealed class MainDataSet : DataSet<T>
        {
            public MainDataSet(T model)
                : base(model)
            {
                model.SetDataSource(this);
            }

            internal override DataSet CreateSubDataSet(DataRow parentRow)
            {
                return new SubDataSet(this, parentRow);
            }

            public override DataRow ParentRow
            {
                get { return null; }
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

            internal override void InternalRemoveAtCore(int index, DataRow dataRow)
            {
                Debug.Assert(dataRow.Model == Model && dataRow.Ordinal == index);

                dataRow.DisposeByMainDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustOrdinal(i);
            }

            internal override void InternalInsertCore(int index, DataRow dataRow)
            {
                Debug.Assert(index >= 0 && index <= Count);
                Debug.Assert(dataRow.Model == null);

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

            internal override DataSet CreateSubDataSet(DataRow parentRow)
            {
                throw new NotSupportedException();
            }

            private DataSet<T> _mainDataSet;
            private DataRow _parentRow;
            public override DataRow ParentRow
            {
                get { return _parentRow; }
            }

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

            internal override void InternalRemoveAtCore(int index, DataRow dataRow)
            {
                Debug.Assert(dataRow.Model == Model && dataRow.ChildOrdinal == index);

                dataRow.DisposeBySubDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustChildOrdinal(i);

                _mainDataSet.InternalRemoveAt(dataRow.Ordinal);
            }

            internal override void InternalInsertCore(int index, DataRow dataRow)
            {
                Debug.Assert(index >= 0 && index <= Count);
                Debug.Assert(dataRow.Model == null);

                dataRow.InitializeBySubDataSet(_parentRow, index);
                _rows.Insert(index, dataRow);
                _mainDataSet.InternalInsert(GetMainDataSetIndex(dataRow), dataRow);
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
        private DataSet(T model)
            : base(model)
        {
            Debug.Assert(model != null);
            this._ = model;
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
            var parentRelationship = childModel.ParentRelationship;
            var childQuery = GetChildQuery(sourceData, dataRow, parentRelationship);
            if (childQueryInitializer != null)
                childQueryInitializer(childQuery);
            sourceData.DbSession.FillDataSet(childQuery, childDataSet);

            return childDataSet;
        }

        private static DbQuery<TChild> GetChildQuery<TChild>(DbSet<TChild> dbSet, DataRow parentRow, ReadOnlyCollection<ColumnMapping> parentRelationship)
            where TChild : Model, new()
        {
            var dbSession = dbSet.DbSession;
            var childModel = Model.Clone(dbSet._, false);
            var queryBuilder = dbSet.QueryStatement.MakeSelectAllQueryBuilder(childModel, false);
            queryBuilder.Where(parentRow, parentRelationship);
            return dbSession.CreateQuery(childModel, queryBuilder);
        }

        public async Task<DataSet<TChild>> CreateChildAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<DbQuery<TChild>> childQueryInitializer, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            var dataRow = this[dataRowOrdinal];
            var childModel = getChildModel(this._);
            if (childModel.ParentModel != this._)
                throw new ArgumentException(Strings.InvalidChildModelGetter, nameof(getChildModel));

            var childDataSet = (DataSet<TChild>)dataRow[childModel];
            var mappings = childModel.ParentRelationship;
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

        public DbTable<T> ToTempTable(DbSession dbSession)
        {
            Check.NotNull(dbSession, nameof(dbSession));

            var result = dbSession.CreateTempTable<T>();
            result.Insert(this, null, false);
            return result;
        }

        public Task<DbTable<T>> ToTempTableAsync(DbSession dbSession)
        {
            return ToTempTableAsync(dbSession, CancellationToken.None);
        }

        public async Task<DbTable<T>> ToTempTableAsync(DbSession dbSession, CancellationToken cancellationToken)
        {
            Check.NotNull(dbSession, nameof(dbSession));

            Action<T> initializer = null;
            var result = await dbSession.CreateTempTableAsync<T>(initializer, cancellationToken);
            await result.InsertAsync(this, null, false, false, cancellationToken);
            return result;
        }
    }
}
