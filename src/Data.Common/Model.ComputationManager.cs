using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    partial class Model
    {
        private sealed class ComputationManager
        {
            public static ComputationManager Merge(ComputationManager computationManager, Model model)
            {
                Debug.Assert(model != null);

                var computationColumns = model.ComputationColumns;
                if (computationColumns.Count == 0)
                    return computationManager;

                if (computationManager == null)
                    computationManager = new ComputationManager();

                foreach (var column in computationColumns)
                    computationManager.Merge(column);

                foreach (var value in computationManager._dependencies.Values)
                    Seal(value);

                return computationManager;
            }

            private Dictionary<Column, Dictionary<Model, IColumnSet>> _dependencies = new Dictionary<Column, Dictionary<Model, IColumnSet>>();

            private ComputationManager()
            {
            }

            private void Merge(Column column)
            {
                Debug.Assert(column.IsExpression);

                var baseColumns = column.GetExpression().BaseColumns;
                foreach (var baseColumn in baseColumns)
                    Merge(baseColumn, column, true);
            }

            private void Merge(Column baseColumn, Column affectedColumn, bool addDependency)
            {
                if (baseColumn == affectedColumn)
                    throw new InvalidOperationException(Strings.Model_CircularComputation(baseColumn.Name));

                if (baseColumn.ParentModel != null)
                {
                    if (addDependency)
                    {
                        AddDependency(baseColumn, affectedColumn);
                        addDependency = false;
                    }
                }

                if (baseColumn.IsExpression)
                {
                    var baseBaseColumns = baseColumn.GetExpression().BaseColumns;
                    foreach (var baseBaseColumn in baseBaseColumns)
                        Merge(baseBaseColumn, affectedColumn, addDependency);
                }
            }

            private void AddDependency(Column baseColumn, Column affectedColumn)
            {
                var model = affectedColumn.ParentModel;
                Debug.Assert(model != null);
                Dictionary<Model, IColumnSet> affectedColumnsByModel;
                if (!_dependencies.TryGetValue(baseColumn, out affectedColumnsByModel))
                {
                    affectedColumnsByModel = new Dictionary<Model, IColumnSet>();
                    _dependencies.Add(baseColumn, affectedColumnsByModel);
                    baseColumn.EnsureConcrete();
                }
                if (affectedColumnsByModel.ContainsKey(model))
                    affectedColumnsByModel[model] = affectedColumnsByModel[model].Add(affectedColumn);
                else
                    affectedColumnsByModel.Add(model, affectedColumn);
            }

            public IReadOnlyDictionary<Model, IColumnSet> GetAffectedColumnsByModel(Column column)
            {
                Dictionary<Model, IColumnSet> result;
                _dependencies.TryGetValue(column, out result);
                return result;
            }
        }

        private ComputationManager _computationManager;

        private void MergeComputations(Model model)
        {
            Debug.Assert(RootModel == this);
            Debug.Assert(model.RootModel == this);
            _computationManager = ComputationManager.Merge(_computationManager, model);
        }

        private sealed class EmptyAffectedColumn : IReadOnlyDictionary<Model, IColumnSet>
        {
            public static readonly EmptyAffectedColumn Singleton = new EmptyAffectedColumn();

            private EmptyAffectedColumn()
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

        private IColumnSet GetAffectedColumns(IColumnSet updatedColumns)
        {
            var computationManager = RootModel._computationManager;
            if (computationManager == null)
                return ColumnSet.Empty;

            var result = ColumnSet.Empty;
            foreach (var updatedColumn in updatedColumns)
            {
                Debug.Assert(updatedColumn.ParentModel == this);
                var affectedColumns = computationManager.GetAffectedColumnsByModel(updatedColumn);
                if (affectedColumns != null && affectedColumns.ContainsKey(this))
                    result = result.Union(affectedColumns[this]);
            }
            return result.Seal();
        }

        internal IReadOnlyDictionary<Model, IColumnSet> GetCascadeAffectedColumns(IColumnSet columnSet)
        {
            Debug.Assert(columnSet.Count > 0);
            var computationManager = RootModel._computationManager;
            if (computationManager == null)
                return EmptyAffectedColumn.Singleton;

            Dictionary<Model, IColumnSet> result = null;
            foreach (var column in columnSet)
                result = GetCascadeAffectedColumns(result, computationManager, column);

            if (result == null)
                return EmptyAffectedColumn.Singleton;
            else
                return Seal(result);
        }

        private static Dictionary<Model, IColumnSet> GetCascadeAffectedColumns(Dictionary<Model, IColumnSet> result, ComputationManager computationManager, Column column)
        {
            var affectedColumnsByModel = computationManager.GetAffectedColumnsByModel(column);
            if (affectedColumnsByModel == null)
                return result;

            foreach (var keyValuePair in affectedColumnsByModel)
            {
                var model = keyValuePair.Key;
                if (model == column.ParentModel)
                    continue;

                var affectedColumns = keyValuePair.Value;
                if (result == null)
                    result = new Dictionary<Model, IColumnSet>();
                if (result.ContainsKey(model))
                    result[model] = result[model].Union(affectedColumns);
                else
                    result[model] = affectedColumns;
            }

            return result;
        }

        private IReadOnlyDictionary<Model, IColumnSet> GetAggregateAffectedColumns()
        {
            var computationManager = RootModel._computationManager;
            if (computationManager == null)
                return EmptyAffectedColumn.Singleton;

            Dictionary<Model, IColumnSet> result = null;
            var columns = Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var affectedColumns = computationManager.GetAffectedColumnsByModel(column);
                if (affectedColumns == null || affectedColumns.Count == 0)
                    continue;

                if (result == null)
                    result = new Dictionary<Model, IColumnSet>();
                foreach (var keyValuePair in affectedColumns)
                {
                    var aggregateModel = keyValuePair.Key;
                    if (aggregateModel.Depth >= Depth)
                        continue;
                    var aggregateColumns = keyValuePair.Value;

                    if (result.ContainsKey(aggregateModel))
                        result[aggregateModel] = result[aggregateModel].Union(aggregateColumns);
                    else
                        result.Add(aggregateModel, aggregateColumns);
                }
            }

            if (result == null)
                return EmptyAffectedColumn.Singleton;
            else
                return Seal(result);
        }

        private static Dictionary<Model, IColumnSet> Seal(Dictionary<Model, IColumnSet> affectedColumns)
        {
            foreach (var keyValuePair in affectedColumns)
            {
                var model = keyValuePair.Key;
                affectedColumns[model].Seal();
            }

            return affectedColumns;
        }
    }
}
