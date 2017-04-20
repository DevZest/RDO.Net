using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public sealed partial class DataSetContainer
    {
        private sealed class ComputationManager
        {
            private sealed class Node : HashSet<DataRow>
            {
                public Node(Column column, IEnumerable<DataRow> dataRows)
                    : base(dataRows)
                {
                    Column = column;
                }

                public Node(Column column, DataRow dataRow)
                {
                    base.Add(dataRow);
                    Column = column;
                }

                public Column Column { get; private set; }
                public Node Next { get; set; }
            }

            private Dictionary<Column, IColumnSet> _indirectDependencies = new Dictionary<Column, IColumnSet>();
            private Dictionary<Column, IColumnSet> _directDependencies = new Dictionary<Column, IColumnSet>();
            private Dictionary<Model, List<Column>> _computationColumns = new Dictionary<Model, List<Column>>();
            private Dictionary<Model, List<Column>> _aggregateColumns = new Dictionary<Model, List<Column>>();

            internal void AddDependency(Column baseColumn, Column computationColumn, bool isDirect)
            {
                Debug.Assert(baseColumn.ParentModel != null);
                Debug.Assert(computationColumn.ParentModel != null);
                AddDependency(_indirectDependencies, baseColumn, computationColumn);
                if (isDirect)
                {
                    AddDependency(_directDependencies, baseColumn, computationColumn);
                    baseColumn.TryMakeConcrete();
                    AddComputationColumn(_computationColumns, computationColumn.ParentModel, computationColumn);
                    if (computationColumn.ParentModel.Depth < baseColumn.ParentModel.Depth)
                        AddComputationColumn(_aggregateColumns, baseColumn.ParentModel, computationColumn);
                }
            }

            private static void AddDependency<T>(Dictionary<T, IColumnSet> dependencies, T key, Column computationColumn)
            {
                IColumnSet computationColumns;
                if (dependencies.TryGetValue(key, out computationColumns))
                    dependencies[key] = computationColumns.Add(computationColumn);
                else
                    dependencies.Add(key, computationColumn);
            }

            private void AddComputationColumn(Dictionary<Model, List<Column>> dictionary, Model key, Column value)
            {
                List<Column> computationColumns;
                if (dictionary.TryGetValue(key, out computationColumns))
                    AddComputationColumn(computationColumns, value);
                else
                {
                    computationColumns = new List<Column>();
                    computationColumns.Add(value);
                    dictionary.Add(key, computationColumns);
                }
            }

            private void AddComputationColumn(List<Column> computationColumns, Column value)
            {
                for (int i = 0; i < computationColumns.Count; i++)
                {
                    var current = computationColumns[i];
                    if (current == value)
                        return;
                    if (IsDependentUpon(value, current))
                    {
                        computationColumns.Insert(i, value);
                        return;
                    }
                }
                computationColumns.Add(value);
            }

            private int _suspendComputationCount;
            private Dictionary<Column, Node> _nodes = new Dictionary<Column, Node>();
            private Node _firstNode;

            private void Invalidate(Column column, DataRow dataRow)
            {
                Debug.Assert(column != null);
                Debug.Assert(dataRow != null);
                Debug.Assert(_suspendComputationCount > 0);

                if (dataRow.ValueChangedSuspended)
                    return;

                Node node;
                if (_nodes.TryGetValue(column, out node))
                    node.Add(dataRow);
                else
                    AddNode(new Node(column, dataRow));
            }

            private void AddNode(Node node)
            {
                var column = node.Column;
                _nodes.Add(column, node);
                Node prev = null;
                Node current = _firstNode;
                while (current != null)
                {
                    if (IsDependentUpon(current.Column, column))
                        break;
                    prev = current;
                    current = current.Next;
                }

                if (prev == null)
                    _firstNode = node;
                else
                    prev.Next = node;
                node.Next = current;
            }

            private bool IsDependentUpon(Column computationColumn, Column baseColumn)
            {
                IColumnSet computationColumns;
                if (_indirectDependencies.TryGetValue(baseColumn, out computationColumns))
                    return computationColumns.Contains(computationColumn);
                else
                    return false;
            }

            private void RefreshComputations()
            {
                while (_firstNode != null)
                {
                    var node = _firstNode;
                    _firstNode = node.Next;
                    var column = node.Column;
                    _nodes.Remove(column);
                    foreach (var dataRow in node)
                    {
                        if (dataRow.Model == null)  // disposed DataRow
                            continue;
                        column.RefreshComputation(dataRow);
                    }
                }
            }

            internal void SuspendComputation()
            {
                _suspendComputationCount++;
            }

            internal void ResumeComputation()
            {
                if (_suspendComputationCount == 0)
                    throw new InvalidOperationException(Strings.DataSetContainer_ResumeComputationWithoutSuspendComputation);
                if (_suspendComputationCount == 1)
                    RefreshComputations();
                _suspendComputationCount--;
            }

            private IReadOnlyList<Column> ComputationColumnsOf(Model model)
            {
                return GetColumns(_computationColumns, model);
            }

            private IReadOnlyList<Column> AggregateColumnsOf(Model model)
            {
                return GetColumns(_aggregateColumns, model);
            }

            private static  IReadOnlyList<Column> GetColumns(Dictionary<Model, List<Column>> dictionary, Model model)
            {
                List<Column> result;
                if (dictionary.TryGetValue(model, out result))
                    return result;
                else
                    return Array<Column>.Empty;
            }

            internal void OnBeforeDataRowInserted(DataRow dataRow)
            {
                var computationColumns = ComputationColumnsOf(dataRow.Model);
                if (computationColumns.Count == 0)
                    return;

                for (int i = 0; i < computationColumns.Count; i++)
                    computationColumns[i].RefreshComputation(dataRow);
            }

            internal void OnAfterDataRowInserted(DataRow dataRow)
            {
                InvalidateAggregateComputationColumns(dataRow.Model, dataRow.ParentDataRow);
            }

            internal void OnDataRowRemoved(DataSet dataSet)
            {
                InvalidateAggregateComputationColumns(dataSet.Model, dataSet.ParentDataRow);
            }

            private void InvalidateAggregateComputationColumns(Model model, DataRow parentDataRow)
            {
                if (parentDataRow == null)
                    return;
                var aggregateColumns = AggregateColumnsOf(model);
                if (aggregateColumns.Count == 0)
                    return;

                for (int i = 0; i < aggregateColumns.Count; i++)
                    InvalidateAggregateComputationColumn(aggregateColumns[i], parentDataRow);
            }

            private void InvalidateAggregateComputationColumn(Column aggregateColumn, DataRow parentDataRow)
            {
                var aggregateModel = aggregateColumn.ParentModel;
                var ancestorLevel = parentDataRow.Model.Depth - aggregateModel.Depth;
                var aggregateDataRow = parentDataRow.AncestorOf(ancestorLevel);
                Invalidate(aggregateColumn, aggregateDataRow);
            }

            private IColumnSet this[Column column]
            {
                get
                {
                    IColumnSet result;
                    return _directDependencies.TryGetValue(column, out result) ? result : ColumnSet.Empty;
                }
            }

            internal void OnValueChanged(DataRow dataRow, Column column)
            {
                var computationColumns = this[column];
                if (computationColumns.Count == 0)
                    return;

                var depth = dataRow.Model.Depth;
                foreach (var computationColumn in computationColumns)
                {
                    var computationModelDepth = computationColumn.ParentModel.Depth;
                    if (computationModelDepth == depth)
                        Invalidate(computationColumn, dataRow);
                    else if (computationModelDepth < depth)
                        InvalidateAggregateComputationColumn(computationColumn, dataRow.ParentDataRow);
                    else
                        InvalidateDescendentComputation(computationColumn, dataRow);
                }
            }

            private void InvalidateDescendentComputation(Column decendentColumn, DataRow dataRow)
            {
                var decendentModel = decendentColumn.ParentModel;
                var childModel = ChildAncestorOf(dataRow, decendentModel);
                var childDataSet = dataRow[childModel];
                if (childModel == decendentModel)
                {
                    for (int i = 0; i < childDataSet.Count; i++)
                        Invalidate(decendentColumn, childDataSet[i]);
                }
                else
                {
                    for (int i = 0; i < childDataSet.Count; i++)
                        InvalidateDescendentComputation(decendentColumn, childDataSet[i]);
                }
            }

            private static Model ChildAncestorOf(DataRow dataRow, Model decendent)
            {
                for (; decendent != null; decendent = decendent.ParentModel)
                {
                    if (decendent.ParentModel == dataRow.Model)
                        return decendent;
                }
                return null;
            }
        }

        public event DataRowEventHandler DataRowInserting = delegate { };
        public event DataRowEventHandler BeforeDataRowInserted = delegate { };
        public event DataRowEventHandler AfterDataRowInserted = delegate { };
        public event DataRowEventHandler DataRowRemoving = delegate { };
        public event DataRowRemovedEventHandler DataRowRemoved = delegate { };
        public event ValueChangedEventHandler ValueChanged = delegate { };

        private ComputationManager _computationManager;

        internal void MergeComputations(Model model)
        {
            Debug.Assert(model != null);
            var columns = model.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (column.IsExpression)
                    MergeComputation(column);
            }
        }

        private void MergeComputation(Column computationColumn)
        {
            Debug.Assert(computationColumn.IsExpression);

            if (_computationManager == null)
                _computationManager = new ComputationManager();

            var baseColumns = computationColumn.GetExpression().BaseColumns;
            foreach (var baseColumn in baseColumns)
                MergeComputation(baseColumn, computationColumn, true);
        }

        private void MergeComputation(Column baseColumn, Column computationColumn, bool isDirect)
        {
            if (baseColumn == computationColumn)
                throw new InvalidOperationException(Strings.ComputationManager_CircularComputation(baseColumn.Name));

            if (baseColumn.ParentModel != null)
            {
                _computationManager.AddDependency(baseColumn, computationColumn, isDirect);
                isDirect = false;
            }

            if (baseColumn.IsExpression)
            {
                var baseBaseColumns = baseColumn.GetExpression().BaseColumns;
                foreach (var baseBaseColumn in baseBaseColumns)
                    MergeComputation(baseBaseColumn, computationColumn, isDirect);
            }
        }

        public void SuspendComputation()
        {
            _computationManager?.SuspendComputation();
        }

        public void ResumeComputation()
        {
            _computationManager?.ResumeComputation();
        }

        internal void OnDataRowInserting(DataRow dataRow)
        {
            DataRowInserting(dataRow);
        }

        internal void OnBeforeDataRowInserted(DataRow dataRow)
        {
            _computationManager?.OnBeforeDataRowInserted(dataRow);
            BeforeDataRowInserted(dataRow);
        }

        internal void OnAfterDataRowInserted(DataRow dataRow)
        {
            AfterDataRowInserted(dataRow);
            _computationManager?.OnAfterDataRowInserted(dataRow);
        }

        internal void OnValueChanged(DataRow dataRow, Column column)
        {
            ValueChanged(dataRow, column);
            _computationManager?.OnValueChanged(dataRow, column);
        }

        internal void OnDataRowRemoving(DataRow dataRow)
        {
            DataRowRemoving(dataRow);
        }

        internal void OnDataRowRemoved(DataRow dataRow, DataSet baseDataSet, int ordinal, DataSet dataSet, int index)
        {
            DataRowRemoved(dataRow, baseDataSet, ordinal, dataSet, index);
            _computationManager?.OnDataRowRemoved(dataSet);
        }
    }
}
