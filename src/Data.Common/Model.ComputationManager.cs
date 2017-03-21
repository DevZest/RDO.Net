using System;
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

                var columns = model.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    if (!column.IsExpression)
                        continue;

                    if (computationManager == null)
                        computationManager = new ComputationManager();
                    computationManager.Merge(column);
                }
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

            public IReadOnlyDictionary<Model, IColumnSet> GetAffectedColumns(Column column)
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
    }
}
