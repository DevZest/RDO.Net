using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public abstract class ComputationManager
    {
        public abstract IReadOnlyDictionary<Model, IColumnSet> this[Column column] { get; }

        public abstract IReadOnlyDictionary<Model, IColumnSet> BaseColumns { get; }

        internal ComputationManager Merge(Model model)
        {
            Debug.Assert(model != null);

            var computationColumns = model.ComputationColumns;
            return computationColumns.Count == 0 ? this : Merge(computationColumns);
        }

        internal abstract ComputationManager Merge(IColumnSet computationColumns);

        private static Dictionary<Model, IColumnSet> Seal(Dictionary<Model, IColumnSet> columnsByModel)
        {
            foreach (var columns in columnsByModel.Values)
                columns.Seal();

            return columnsByModel;
        }

        internal virtual IColumnSet GetSiblingComputationColumns(IColumnSet baseColumns)
        {
            var result = ColumnSet.Empty;
            foreach (var baseColumn in baseColumns)
            {
                var computationColumns = this[baseColumn];
                if (computationColumns != null)
                {
                    var model = baseColumn.ParentModel;
                    if (computationColumns.ContainsKey(model))
                        result = result.Union(computationColumns[model]);
                }
            }
            return result.Seal();
        }

        internal virtual IReadOnlyDictionary<Model, IColumnSet> GetNonSiblingComputationColumns(IColumnSet baseColumns)
        {
            Dictionary<Model, IColumnSet> result = null;
            foreach (var column in baseColumns)
                result = GetNonsiblingComputationColumns(result, column);

            if (result == null)
                return EmptyColumnsByModel.Singleton;
            else
                return Seal(result);
        }

        private Dictionary<Model, IColumnSet> GetNonsiblingComputationColumns(Dictionary<Model, IColumnSet> result, Column baseColumn)
        {
            var computationColumnsByModel = this[baseColumn];
            if (computationColumnsByModel == null)
                return result;

            foreach (var keyValuePair in computationColumnsByModel)
            {
                var model = keyValuePair.Key;
                if (model == baseColumn.ParentModel)
                    continue;

                var computationColumns = keyValuePair.Value;
                if (result == null)
                    result = new Dictionary<Model, IColumnSet>();
                if (result.ContainsKey(model))
                    result[model] = result[model].Union(computationColumns);
                else
                    result[model] = computationColumns;
            }

            return result;
        }

        internal virtual IReadOnlyDictionary<Model, IColumnSet> GetAggregateComputationColumns(Model model)
        {
            Dictionary<Model, IColumnSet> result = null;
            var columns = model.Columns;
            var depth = model.Depth;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var computationColumns = this[column];
                if (computationColumns == null || computationColumns.Count == 0)
                    continue;

                if (result == null)
                    result = new Dictionary<Model, IColumnSet>();
                foreach (var keyValuePair in computationColumns)
                {
                    var aggregateModel = keyValuePair.Key;
                    if (aggregateModel.Depth >= depth)
                        continue;
                    var aggregateColumns = keyValuePair.Value;

                    if (result.ContainsKey(aggregateModel))
                        result[aggregateModel] = result[aggregateModel].Union(aggregateColumns);
                    else
                        result.Add(aggregateModel, aggregateColumns);
                }
            }

            if (result == null)
                return EmptyColumnsByModel.Singleton;
            else
                return Seal(result);
        }

        public void RefreshComputations(DataRow dataRow)
        {
            VerifyDataRow(dataRow, nameof(dataRow));
            dataRow.RefreshComputations(dataRow.Model.ComputationColumns);
        }

        public void RefreshComputations(DataRow dataRow, IColumnSet computationColumns)
        {
            VerifyDataRow(dataRow, nameof(dataRow));
            Check.NotNull(computationColumns, nameof(computationColumns));

            dataRow.RefreshComputations(computationColumns);
        }

        private void VerifyDataRow(DataRow dataRow, string paramName)
        {
            Check.NotNull(dataRow, paramName);
            if (dataRow.Model == null)
                throw new ObjectDisposedException(paramName);
        }

        private sealed class EmptyColumnsByModel : IReadOnlyDictionary<Model, IColumnSet>
        {
            public static readonly EmptyColumnsByModel Singleton = new EmptyColumnsByModel();

            private EmptyColumnsByModel()
            {
            }

            public IColumnSet this[Model key]
            {
                get { throw new ArgumentOutOfRangeException(nameof(key)); }
            }

            public int Count
            {
                get { return 0; }
            }

            public IEnumerable<Model> Keys
            {
                get { yield break; }
            }

            public IEnumerable<IColumnSet> Values
            {
                get { yield break; }
            }

            public bool ContainsKey(Model key)
            {
                return false;
            }

            public IEnumerator<KeyValuePair<Model, IColumnSet>> GetEnumerator()
            {
                return EmptyEnumerator<KeyValuePair<Model, IColumnSet>>.Singleton;
            }

            public bool TryGetValue(Model key, out IColumnSet value)
            {
                value = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        internal static ComputationManager Empty
        {
            get { return EmptyComputationManager.Singleton; }
        }

        private sealed class EmptyComputationManager : ComputationManager
        {
            public static EmptyComputationManager Singleton = new EmptyComputationManager();

            private EmptyComputationManager()
            {
            }

            internal override ComputationManager Merge(IColumnSet computationColumns)
            {
                return new ConcreteComputationManager().Merge(computationColumns);
            }

            public override IReadOnlyDictionary<Model, IColumnSet> this[Column column]
            {
                get { return null; }
            }

            public override IReadOnlyDictionary<Model, IColumnSet> BaseColumns
            {
                get { return null; }
            }

            internal override IColumnSet GetSiblingComputationColumns(IColumnSet baseColumns)
            {
                return ColumnSet.Empty;
            }

            internal override IReadOnlyDictionary<Model, IColumnSet> GetNonSiblingComputationColumns(IColumnSet baseColumns)
            {
                return EmptyColumnsByModel.Singleton;
            }

            internal override IReadOnlyDictionary<Model, IColumnSet> GetAggregateComputationColumns(Model model)
            {
                return EmptyColumnsByModel.Singleton;
            }
        }

        private sealed class ConcreteComputationManager : ComputationManager
        {
            private Dictionary<Column, Dictionary<Model, IColumnSet>> _dependencies = new Dictionary<Column, Dictionary<Model, IColumnSet>>();
            private Dictionary<Model, IColumnSet> _baseColumns = new Dictionary<Model, IColumnSet>();

            public ConcreteComputationManager()
            {
            }

            internal override ComputationManager Merge(IColumnSet computationColumns)
            {
                foreach (var column in computationColumns)
                    Merge(column);

                Seal(_baseColumns);

                foreach (var value in _dependencies.Values)
                    Seal(value);

                return this;
            }

            private void Merge(Column computationColumn)
            {
                Debug.Assert(computationColumn.IsExpression);

                var baseColumns = computationColumn.GetExpression().BaseColumns;
                foreach (var baseColumn in baseColumns)
                    Merge(baseColumn, computationColumn, true);
            }

            private void Merge(Column baseColumn, Column computationColumn, bool addDependency)
            {
                if (baseColumn == computationColumn)
                    throw new InvalidOperationException(Strings.ComputationManager_CircularComputation(baseColumn.Name));

                if (baseColumn.ParentModel != null)
                {
                    if (addDependency)
                    {
                        AddColumn(_baseColumns, baseColumn);
                        AddDependency(baseColumn, computationColumn);
                        addDependency = false;
                    }
                }

                if (baseColumn.IsExpression)
                {
                    var baseBaseColumns = baseColumn.GetExpression().BaseColumns;
                    foreach (var baseBaseColumn in baseBaseColumns)
                        Merge(baseBaseColumn, computationColumn, addDependency);
                }
            }

            private void AddDependency(Column baseColumn, Column computationColumn)
            {
                var model = computationColumn.ParentModel;
                Debug.Assert(model != null);
                Dictionary<Model, IColumnSet> computationColumns;
                if (!_dependencies.TryGetValue(baseColumn, out computationColumns))
                {
                    computationColumns = new Dictionary<Model, IColumnSet>();
                    _dependencies.Add(baseColumn, computationColumns);
                    baseColumn.EnsureConcrete();
                }
                AddColumn(computationColumns, computationColumn);
            }

            private static void AddColumn(Dictionary<Model, IColumnSet> columnsByModel, Column column)
            {
                var model = column.ParentModel;
                Debug.Assert(model != null);
                if (columnsByModel.ContainsKey(model))
                    columnsByModel[model] = columnsByModel[model].Add(column);
                else
                    columnsByModel.Add(model, column);
            }

            public override IReadOnlyDictionary<Model, IColumnSet> this[Column column]
            {
                get
                {
                    Dictionary<Model, IColumnSet> result;
                    _dependencies.TryGetValue(column, out result);
                    return result;
                }
            }

            public override IReadOnlyDictionary<Model, IColumnSet> BaseColumns
            {
                get { return _baseColumns; }
            }
        }
    }
}
