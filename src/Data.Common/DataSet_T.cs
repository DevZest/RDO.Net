using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>Represents an in-memory collection of data.</summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    public abstract class DataSet<T> : DataSet
        where T : Model, new()
    {
        private sealed class GlobalDataSet : DataSet<T>
        {
            public GlobalDataSet(T model)
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
                return dataRow == null || dataRow.Model != Model ? -1 : dataRow.Ordinal;
            }

            internal override void InternalRemoveAtCore(int index, DataRow dataRow)
            {
                Debug.Assert(dataRow.Model == Model && dataRow.Ordinal == index);

                dataRow.DisposeByGlobalDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustOrdinal(i);
            }

            internal override void InternalInsertCore(int index, DataRow dataRow)
            {
                Debug.Assert(index >= 0 && index <= Count);
                Debug.Assert(dataRow.Model == null);

                dataRow.InitializeByGlobalDataSet(Model, index);
                _rows.Insert(index, dataRow);
                for (int i = index + 1; i < _rows.Count; i++)
                    _rows[i].AdjustOrdinal(i);
            }
        }

        private sealed class SubDataSet : DataSet<T>
        {
            public SubDataSet(GlobalDataSet globalDataSet, DataRow parentRow)
                : base(globalDataSet._)
            {
                Debug.Assert(globalDataSet != null);
                _globalDataSet = globalDataSet;
                _parentRow = parentRow;
            }

            internal override DataSet CreateSubDataSet(DataRow parentRow)
            {
                throw new NotSupportedException();
            }

            private DataSet<T> _globalDataSet;
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
                return dataRow == null || dataRow.Model != Model ? -1 : dataRow.Index;
            }

            internal override void InternalRemoveAtCore(int index, DataRow dataRow)
            {
                Debug.Assert(dataRow.Model == Model && dataRow.Index == index);

                dataRow.DisposeBySubDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustIndex(i);

                _globalDataSet.InternalRemoveAt(dataRow.Ordinal);
            }

            internal override void InternalInsertCore(int index, DataRow dataRow)
            {
                Debug.Assert(index >= 0 && index <= Count);
                Debug.Assert(dataRow.Model == null);

                dataRow.InitializeBySubDataSet(_parentRow, index);
                _rows.Insert(index, dataRow);
                for (int i = index + 1; i < _rows.Count; i++)
                    _rows[i].AdjustIndex(i);
                _globalDataSet.InternalInsert(GetGlobalDataSetIndex(dataRow), dataRow);
            }

            private int GetGlobalDataSetIndex(DataRow dataRow)
            {
                if (_globalDataSet.Count == 0)
                    return 0;

                if (Count > 1)
                {
                    if (dataRow.Index > 0)
                        return this[dataRow.Index - 1].Ordinal + 1;  // after the previous DataRow
                    else
                        return this[dataRow.Index + 1].Ordinal - 1;  // before the next DataRow
                }

                return BinarySearchGlobalDataSetIndex(_globalDataSet[0], _globalDataSet[_globalDataSet.Count - 1], dataRow.ParentDataRow.Ordinal);
            }

            private int BinarySearchGlobalDataSetIndex(DataRow startRow, DataRow endRow, int parentOrdinal)
            {
                if (parentOrdinal > endRow.ParentDataRow.Ordinal)
                    return endRow.Ordinal + 1;  // after the end DataRow

                if (parentOrdinal < startRow.ParentDataRow.Ordinal)
                    return startRow.Ordinal;  // before the start DataRow

                var midOrdinal = (startRow.Ordinal + endRow.Ordinal) >> 1;
                var midRow = _globalDataSet[midOrdinal];
                if (parentOrdinal < midRow.ParentDataRow.Ordinal)
                    return BinarySearchGlobalDataSetIndex(startRow, midRow, parentOrdinal);
                else
                    return BinarySearchGlobalDataSetIndex(midRow, endRow, parentOrdinal);
            }
        }

        internal static DataSet<T> Create(T model)
        {
            return new GlobalDataSet(model);
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
            return new GlobalDataSet(model);
        }

        public readonly T _;

        public static DataSet<T> ParseJson(string json, Action<T> initializer = null)
        {
            Check.NotEmpty(json, nameof(json));

            return (DataSet<T>)(new JsonParser(json).Parse(() => New(initializer)));
        }

        public DataSet<TChild> Fill<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<TChild> initializer = null)
            where TChild : Model, new()
        {
            var dataRow = this[dataRowOrdinal];
            var childModel = getChildModel(this._);
            if (childModel.ParentModel != this._)
                throw new ArgumentException(Strings.InvalidChildModelGetter, nameof(getChildModel));

            var childDataSet = dataRow.Children(childModel);
            var parentRelationship = childModel.ParentRelationship;
            var childQuery = GetChildQuery(sourceData, dataRow, parentRelationship, initializer);
            sourceData.DbSession.FillDataSet(childQuery, childDataSet);

            return childDataSet;
        }

        private static DbQuery<TChild> GetChildQuery<TChild>(DbSet<TChild> dbSet, DataRow parentRow, ReadOnlyCollection<ColumnMapping> parentRelationship, Action<TChild> initializer)
            where TChild : Model, new()
        {
            var dbSession = dbSet.DbSession;
            var childModel = Model.Clone(dbSet._, false);
            if (initializer != null)
                initializer(childModel);
            var queryStatement = dbSet.QueryStatement.BuildQueryStatement(childModel, builder => builder.Where(parentRow, parentRelationship), null);
            return dbSession.CreateQuery(childModel, queryStatement);
        }

        public async Task<DataSet<TChild>> FillAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<TChild> initializer, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            var dataRow = this[dataRowOrdinal];
            var childModel = getChildModel(this._);
            if (childModel.ParentModel != this._)
                throw new ArgumentException(Strings.InvalidChildModelGetter, nameof(getChildModel));

            var childDataSet = (DataSet<TChild>)dataRow[childModel];
            var mappings = childModel.ParentRelationship;
            var childQuery = GetChildQuery(sourceData, dataRow, mappings, initializer);
            await sourceData.DbSession.FillDataSetAsync(childQuery, childDataSet, cancellationToken);

            return childDataSet;
        }

        public Task<DataSet<TChild>> FillAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<TChild> initializer = null)
            where TChild : Model, new()
        {
            return FillAsync(dataRowOrdinal, getChildModel, sourceData, initializer, CancellationToken.None);
        }

        public Task<DataSet<TChild>> FillAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            return FillAsync(dataRowOrdinal, getChildModel, sourceData, null, cancellationToken);
        }

        public DataSet<TChild> Children<TChild>(Func<T, TChild> getChild, int ordinal)
            where TChild : Model, new()
        {
            return Children(getChild, this[ordinal]);
        }

        public DataSet<TChild> Children<TChild>(Func<T, TChild> getChild, DataRow dataRow = null)
            where TChild : Model, new()
        {
            Check.NotNull(getChild, nameof(getChild));
            var childModel = getChild(_);
            if (childModel == null || childModel.ParentModel != this.Model)
                throw new ArgumentException(Strings.InvalidChildModelGetter, nameof(getChild));
            return dataRow == null ? (DataSet<TChild>)childModel.DataSet : dataRow.Children(childModel);
        }
    }
}
