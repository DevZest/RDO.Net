using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>
    /// Root container for DataSet and children DataSets to handle changes made to DataSet.
    /// </summary>
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

            private Dictionary<Column, IColumns> _indirectDependencies = new Dictionary<Column, IColumns>();
            private Dictionary<Column, IColumns> _directDependencies = new Dictionary<Column, IColumns>();
            private readonly Dictionary<Model, List<Column>> _computationColumns = new Dictionary<Model, List<Column>>();
            private readonly Dictionary<Model, List<Column>> _aggregateColumns = new Dictionary<Model, List<Column>>();

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

            private static void AddDependency<T>(Dictionary<T, IColumns> dependencies, T key, Column computationColumn)
            {
                if (dependencies.TryGetValue(key, out var computationColumns))
                    dependencies[key] = computationColumns.Add(computationColumn);
                else
                    dependencies.Add(key, computationColumn);
            }

            private void AddComputationColumn(Dictionary<Model, List<Column>> dictionary, Model key, Column value)
            {
                if (dictionary.TryGetValue(key, out var computationColumns))
                    AddComputationColumn(computationColumns, value);
                else
                {
                    computationColumns = new List<Column> { value };
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

                if (dataRow.IsValueChangedNotificationSuspended)
                    return;

                if (_nodes.TryGetValue(column, out var node))
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
                if (_indirectDependencies.TryGetValue(baseColumn, out var computationColumns))
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
                    throw new InvalidOperationException(DiagnosticMessages.DataSetContainer_ResumeComputationWithoutSuspendComputation);
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
                if (dictionary.TryGetValue(model, out var result))
                    return result;
                else
                    return Array.Empty<Column>();
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

            private IColumns this[Column column]
            {
                get { return _directDependencies.TryGetValue(column, out var result) ? result : Columns.Empty; }
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

        /// <summary>
        /// Occurs when DataRow is inserting.
        /// </summary>
        public event EventHandler<DataRowEventArgs> DataRowInserting = delegate { };

        /// <summary>
        /// Occurs before DataRow inserted.
        /// </summary>
        public event EventHandler<DataRowEventArgs> BeforeDataRowInserted = delegate { };

        /// <summary>
        /// Occurs after DataRow inserted.
        /// </summary>
        public event EventHandler<DataRowEventArgs> AfterDataRowInserted = delegate { };

        /// <summary>
        /// Occurs before DataRow removed.
        /// </summary>
        public event EventHandler<DataRowEventArgs> DataRowRemoving = delegate { };

        /// <summary>
        /// Occurs after DataRow removed.
        /// </summary>
        public event EventHandler<DataRowRemovedEventArgs> DataRowRemoved = delegate { };

        /// <summary>
        /// Occurs when data value changed.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs> ValueChanged = delegate { };

        private ComputationManager _computationManager;

        internal void MergeComputations(Model model)
        {
            Debug.Assert(model != null);
            MergeComputation(model.Columns);
            MergeComputation(model.LocalColumns);
        }

        private void MergeComputation(IReadOnlyList<Column> columns)
        {
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
                throw new InvalidOperationException(DiagnosticMessages.ComputationManager_CircularComputation(baseColumn.Name));

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

        /// <summary>
        /// Suspends automatic computation.
        /// </summary>
        public void SuspendComputation()
        {
            _computationManager?.SuspendComputation();
        }

        /// <summary>
        /// Resumes automatic computation.
        /// </summary>
        public void ResumeComputation()
        {
            _computationManager?.ResumeComputation();
        }

        internal void OnDataRowInserting(DataRowEventArgs e)
        {
            var dataRow = e.DataRow;
            var localColumns = dataRow.Model.LocalColumns;
            for (int i = 0; i < localColumns.Count; i++)
                ((ILocalColumn)localColumns[i]).OnDataRowInserting(dataRow);
            DataRowInserting(this, e);
        }

        internal void OnBeforeDataRowInserted(DataRowEventArgs e)
        {
            _computationManager?.OnBeforeDataRowInserted(e.DataRow);
            BeforeDataRowInserted(this, e);
        }

        internal void OnAfterDataRowInserted(DataRowEventArgs e)
        {
            AfterDataRowInserted(this, e);
            _computationManager?.OnAfterDataRowInserted(e.DataRow);
        }

        internal void OnValueChanged(ValueChangedEventArgs e)
        {
            ValueChanged(this, e);
            foreach (var column in e.Columns)
                _computationManager?.OnValueChanged(e.DataRow, column);
        }

        internal void OnDataRowRemoving(DataRowEventArgs e)
        {
            var dataRow = e.DataRow;
            var localColumns = dataRow.Model.LocalColumns;
            for (int i = 0; i < localColumns.Count; i++)
                ((ILocalColumn)localColumns[i]).OnDataRowRemoving(dataRow);
            DataRowRemoving(this, e);
        }

        internal void OnDataRowRemoved(DataRowRemovedEventArgs e)
        {
            DataRowRemoved(this, e);
            _computationManager?.OnDataRowRemoved(e.DataSet);
        }
    }
}
