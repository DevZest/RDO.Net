using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>Represents an in-memory collection of data.</summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    public abstract class DataSet<T> : DataSet
        where T : class, IModelReference, new()
    {
        private sealed class BaseDataSet : DataSet<T>
        {
            public BaseDataSet(T modelRef)
                : base(modelRef)
            {
                modelRef.Model.SetDataSource(this);
            }

            internal override DataSet CreateChildDataSet(DataRow parentRow)
            {
                return new ChildDataSet(this, parentRow);
            }

            public override DataRow ParentDataRow
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

            internal override void CoreRemoveAt(int index, DataRow dataRow)
            {
                Debug.Assert(dataRow.Model == Model && dataRow.Ordinal == index);

                dataRow.DisposeByBaseDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustOrdinal(i);
            }

            internal override void CoreInsert(int index, DataRow dataRow)
            {
                Debug.Assert(index >= 0 && index <= Count);
                Debug.Assert(dataRow.Model == null);

                dataRow.InitializeByBaseDataSet(Model, index);
                _rows.Insert(index, dataRow);
                for (int i = index + 1; i < _rows.Count; i++)
                    _rows[i].AdjustOrdinal(i);
            }
        }

        private sealed class ChildDataSet : DataSet<T>
        {
            public ChildDataSet(BaseDataSet baseDataSet, DataRow parentRow)
                : base(baseDataSet._)
            {
                Debug.Assert(baseDataSet != null);
                _baseDataSet = baseDataSet;
                _parentRow = parentRow;
            }

            internal override DataSet CreateChildDataSet(DataRow parentRow)
            {
                throw new NotSupportedException();
            }

            private DataSet<T> _baseDataSet;
            private DataRow _parentRow;
            public override DataRow ParentDataRow
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

            internal override void CoreRemoveAt(int index, DataRow dataRow)
            {
                Debug.Assert(dataRow.Model == Model && dataRow.Index == index);

                dataRow.DisposeByChildDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustIndex(i);

                _baseDataSet.InnerRemoveAt(dataRow.Ordinal);
            }

            internal override void CoreInsert(int index, DataRow dataRow)
            {
                Debug.Assert(index >= 0 && index <= Count);
                Debug.Assert(dataRow.Model == null);

                dataRow.InitializeByChildDataSet(_parentRow, index);
                _rows.Insert(index, dataRow);
                for (int i = index + 1; i < _rows.Count; i++)
                    _rows[i].AdjustIndex(i);
                _baseDataSet.InternalInsert(GetBaseDataSetOrdinal(dataRow), dataRow);
            }

            private int GetBaseDataSetOrdinal(DataRow dataRow)
            {
                if (_baseDataSet.Count == 0)
                    return 0;

                if (Count > 1)
                {
                    if (dataRow.Index > 0)
                        return this[dataRow.Index - 1].Ordinal + 1;  // after the previous DataRow
                    else
                        return this[dataRow.Index + 1].Ordinal;  // before the next DataRow
                }

                return BinarySearchBaseDataSetOrdinal(dataRow);
            }

            private int BinarySearchBaseDataSetOrdinal(DataRow dataRow)
            {
                var parentOrdinal = dataRow.ParentDataRow.Ordinal;

                var endOrdinal = _baseDataSet.Count - 1;
                if (parentOrdinal > _baseDataSet[endOrdinal].ParentDataRow.Ordinal)
                    return endOrdinal + 1;  // after the end

                var startOrdinal = 0;
                if (parentOrdinal < _baseDataSet[startOrdinal].ParentDataRow.Ordinal)
                    return startOrdinal;  // before the start

                int resultOrdinal = endOrdinal;
                bool flagBefore = true;
                while (startOrdinal < endOrdinal)
                {
                    var mid = (startOrdinal + endOrdinal) / 2;
                    if (parentOrdinal < _baseDataSet[mid].ParentDataRow.Ordinal)
                    {
                        resultOrdinal = endOrdinal;
                        flagBefore = true;
                        endOrdinal = mid - 1;
                    }
                    else
                    {
                        resultOrdinal = startOrdinal;
                        flagBefore = false;
                        startOrdinal = mid + 1;
                    }
                }

                return flagBefore ? resultOrdinal - 1 : resultOrdinal + 1;
            }
        }

        public static DataSet<T> New(Action<T> initializer = null)
        {
            var modelRef = new T();
            modelRef.Initialize(initializer);
            return Create(modelRef);
        }

        internal static DataSet<T> Create(T modelRef)
        {
            return new BaseDataSet(modelRef);
        }

        public new DataSet<T> Clone()
        {
            var modelRef = _.MakeCopy(false);
            return Create(modelRef);
        }

        internal sealed override DataSet InternalClone()
        {
            return this.Clone();
        }

        private DataSet(T model)
        {
            Debug.Assert(model != null);
            _ = model;
        }

        public T _ { get; private set; }

        public sealed override Model Model
        {
            get { return _.Model; }
        }

        public static DataSet<T> ParseJson(string json)
        {
            return ParseJson(null, json);
        }

        public static DataSet<T> ParseJson(Action<T> initializer, string json)
        {
            Check.NotEmpty(json, nameof(json));

            return (DataSet<T>)(new JsonParser(json).Parse(() => New(initializer), true));
        }

        private static DbQuery<TChild> GetChildQuery<TChild>(DbSet<TChild> dbSet, DataRow parentRow, IReadOnlyList<ColumnMapping> parentRelationship, Action<TChild> initializer)
            where TChild : Model, new()
        {
            var dbSession = dbSet.DbSession;
            var childModel = dbSet._.MakeCopy(false);
            childModel.Initialize(initializer);
            var queryStatement = dbSet.QueryStatement.BuildQueryStatement(childModel, builder => builder.Where(parentRow, parentRelationship), null);
            return dbSession.PerformCreateQuery(childModel, queryStatement);
        }

        public async Task<DataSet<TChild>> FillAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<TChild> initializer, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            var dataRow = this[dataRowOrdinal];
            var childModel = getChildModel(this._);
            if (childModel.ParentModel != Model)
                throw new ArgumentException(DiagnosticMessages.InvalidChildModelGetter, nameof(getChildModel));

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
                throw new ArgumentException(DiagnosticMessages.InvalidChildModelGetter, nameof(getChild));
            return dataRow == null ? childModel.DataSet as DataSet<TChild> : dataRow.Children(childModel);
        }

        private Action<DataRow> GetUpdateAction(Action<T, DataRow> updateAction)
        {
            Action<DataRow> result;
            if (updateAction == null)
                result = null;
            else
                result = (DataRow x) => updateAction(_, x);
            return result;
        }

        public DataRow Insert(int index, Action<T, DataRow> updateAction)
        {
            var result = new DataRow();
            Insert(index, result, GetUpdateAction(updateAction));
            return result;
        }

        public DataRow AddRow(Action<T, DataRow> updateAction = null)
        {
            return Insert(Count, updateAction);
        }

        public DataSet<T> EnsureInitialized()
        {
            Model.EnsureInitialized();
            return this;
        }

        public JsonView<T> Filter(JsonFilter filter)
        {
            Check.NotNull(filter, nameof(filter));
            return new JsonView<T>(_, filter);
        }

        public JsonView<T> Filter(params JsonFilter[] filters)
        {
            return new JsonView<T>(_, JsonFilter.Join(filters));
        }
    }
}
