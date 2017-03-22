using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a row of in-memory data.
    /// </summary>
    public sealed class DataRow
    {
        internal static readonly DataRow Placeholder = new DataRow();

        /// <summary>Initializes a new instance of <see cref="DataRow"/> object.</summary>
        public DataRow()
        {
            Ordinal = -1;
            _index = -1;
        }

        private DataSet[] _childDataSets;
        internal IReadOnlyCollection<DataSet> ChildDataSets
        {
            get { return _childDataSets; }
        }

        /// <summary>Gets the <see cref="Model"/> which associated with this <see cref="DataRow"/>.</summary>
        public Model Model { get; private set; }

        public int Ordinal { get; private set; }

        private int _index;
        public int Index
        {
            get { return _index == -1 ? Ordinal : _index; }
        }

        /// <summary>Gets the parent <see cref="DataRow"/>.</summary>
        public DataRow ParentDataRow { get; private set; }

        public DataSet BaseDataSet
        {
            get { return Model == null ? null : Model.DataSet; }
        }

        public DataSet DataSet
        {
            get
            {
                var parentRow = ParentDataRow;
                return parentRow == null ? BaseDataSet : parentRow[Model];
            }
        }

        internal void InitializeByChildDataSet(DataRow parent, int index)
        {
            Debug.Assert(ParentDataRow == null);
            Debug.Assert(parent != null);

            ParentDataRow = parent;
            _index = index;
        }

        internal void DisposeByChildDataSet()
        {
            ParentDataRow = null;
            _index = -1;
        }

        internal void InitializeByBaseDataSet(Model model, int ordinal)
        {
            Debug.Assert(Model == null);
            Debug.Assert(model != null);

            Model = model;
            Ordinal = ordinal;

            model.EnsureChildModelsInitialized();
            var childModels = model.ChildModels;
            _childDataSets = childModels.Count == 0 ? Array<DataSet>.Empty : new DataSet[childModels.Count];
            for (int i = 0; i < childModels.Count; i++)
                _childDataSets[i] = childModels[i].DataSet.CreateChildDataSet(this);

            var columns = model.Columns;
            foreach (var column in columns)
                column.InsertRow(this);
        }

        internal void DisposeByBaseDataSet()
        {
            ClearChildren();

            var columns = Model.Columns;
            foreach (var column in columns)
                column.RemoveRow(this);

            Model = null;
            Ordinal = -1;
        }

        internal void AdjustOrdinal(int value)
        {
            Debug.Assert(Ordinal != value);
            Ordinal = value;
        }

        internal void AdjustIndex(int value)
        {
            Debug.Assert(_index != value);
            _index = value;
        }

        public DataSet this[Model childModel]
        {
            get
            {
                Check.NotNull(childModel, nameof(childModel));
                if (childModel.ParentModel != Model)
                    throw new ArgumentException(Strings.InvalidChildModel, nameof(childModel));
                return _childDataSets[childModel.Ordinal];
            }
        }

        public DataSet this[int modelOrdinal]
        {
            get
            {
                if (modelOrdinal < 0 || modelOrdinal >= _childDataSets.Length)
                    throw new ArgumentOutOfRangeException(nameof(modelOrdinal));
                return _childDataSets[modelOrdinal];
            }
        }

        private void ClearChildren()
        {
            foreach (var dataSet in _childDataSets)
                dataSet.Clear();
            _childDataSets = null;
        }

        /// <summary>Gets the children data set of this <see cref="DataRow"/>.</summary>
        /// <typeparam name="T">The type of child model.</typeparam>
        /// <param name="childModel">The child model.</param>
        /// <returns>The children data set.</returns>
        public DataSet<T> Children<T>(T childModel)
            where T : Model, new()
        {
            Check.NotNull(childModel, nameof(childModel));
            if (childModel.ParentModel != Model)
                throw new ArgumentException(Strings.InvalidChildModel, nameof(childModel));

            return (DataSet<T>)this[childModel.Ordinal];
        }

        public override string ToString()
        {
            var parentDataRow = this.ParentDataRow;
            if (this.ParentDataRow == null)
                return string.Format(CultureInfo.InvariantCulture, "/[{0}]", Ordinal);

            var result = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", Model.Name, _index);
            return parentDataRow.ToString() + "/" + result;
        }

        internal static DataRow FromString(DataSet dataSet, string input)
        {
            Debug.Assert(dataSet.Model.ParentModel == null);

            var inputIndex = 0;
            ExpectString(input, ref inputIndex, '/');
            var leftSquareBracketIndex = inputIndex;
            var dataRowOrdinal = ExpectInt(input, ref inputIndex, '[', ']');
            var dataRow = GetDataRow(dataSet, dataRowOrdinal, input, leftSquareBracketIndex);
            return inputIndex == input.Length ? dataRow : Deserialize(dataRow, input, inputIndex);
        }

        private static DataRow GetDataRow(DataSet dataSet, int ordinal, string input, int leftSquareBracketIndex)
        {
            if (ordinal < 0 || ordinal >= dataSet.Count)
                throw new FormatException(Strings.DataRow_FromString_InvalidDataRowOrdinal(ordinal, input.Substring(0, leftSquareBracketIndex)));
            return dataSet[ordinal];
        }

        private static DataRow Deserialize(DataRow parentDataRow, string input, int inputIndex)
        {
            var dataRowPathEndIndex = inputIndex;
            var childModelName = ExpectString(input, ref inputIndex, '/', '[');
            var leftSquareBracketIndex = inputIndex - 1;
            var dataRowOrdinal = ExpectInt(input, ref inputIndex, ']');

            var childModel = parentDataRow.Model[childModelName] as Model;
            if (childModel == null)
                throw new FormatException(Strings.DataRow_FromString_InvalidChildModelName(childModelName, input.Substring(0, dataRowPathEndIndex)));

            var result = GetDataRow(parentDataRow[childModel], dataRowOrdinal, input, leftSquareBracketIndex);
            return inputIndex == input.Length ? result : Deserialize(result, input, inputIndex);
        }

        private static string ExpectString(string input, ref int inputIndex, char startChar, char endChar)
        {
            if (input[inputIndex] != startChar)
                throw new FormatException(Strings.DataRow_FromString_ExpectChar(startChar, input.Substring(0, inputIndex)));

            inputIndex++;
            return ExpectString(input, ref inputIndex, endChar);
        }

        private static string ExpectString(string input, ref int inputIndex, char endChar)
        {
            var startIndex = inputIndex;
            while (inputIndex < input.Length && input[inputIndex] != endChar)
                inputIndex++;

            if (inputIndex == input.Length)
                throw new FormatException(Strings.DataRow_FromString_ExpectChar(endChar, input.Substring(0, startIndex)));

            var result = input.Substring(startIndex, inputIndex - startIndex);
            inputIndex++;
            return result;
        }

        private static int ExpectInt(string input, ref int inputIndex, char endChar)
        {
            return ParseInt(ExpectString(input, ref inputIndex, endChar));
        }

        private static int ExpectInt(string input, ref int inputIndex, char startChar, char endChar)
        {
            return ParseInt(ExpectString(input, ref inputIndex, startChar, endChar));
        }

        private static int ParseInt(string input)
        {
            int result;
            if (!Int32.TryParse(input, out result))
                throw new FormatException(Strings.DataRow_FromString_ParseInt(input));
            return result;
        }

        public IValidationMessageGroup Validate(ValidationSeverity? severity = ValidationSeverity.Error)
        {
            return Model.Validate(this, severity);
        }

        public void CopyValuesFrom(DataRow from, bool recursive = true)
        {
            Check.NotNull(from, nameof(from));
            var fromPrototype = from.Model.Prototype;
            var thisPrototype = this.Model.Prototype;
            if (fromPrototype == null || fromPrototype != thisPrototype)
                throw new ArgumentException(Strings.DataRow_VerifyPrototype, nameof(from));

            DoCopyValuesFrom(from, recursive);
        }

        private void DoCopyValuesFrom(DataRow from, bool recursive)
        {
            var thisColumns = Model.Columns;
            var fromColumns = from.Model.Columns;
            for (int i = 0; i < thisColumns.Count; i++)
                thisColumns[i].MapFrom(fromColumns[i]).CopyValue(from, this);

            if (recursive)
                CopyChildren(from);
        }

        private void CopyChildren(DataRow from)
        {
            for (int i = 0; i < _childDataSets.Length; i++)
            {
                var children = _childDataSets[i];
                var fromChildren = from._childDataSets[i];
                for (int j = 0; j < fromChildren.Count; j++)
                    children.AddRow(x => x.DoCopyValuesFrom(fromChildren[j], true));
            }
        }

        public void CopyValuesFrom(DataRow from, IList<ColumnMapping> columnMappings)
        {
            Check.NotNull(from, nameof(from));
            Check.NotNull(columnMappings, nameof(columnMappings));
            for (int i = 0; i < columnMappings.Count; i++)
                columnMappings[i].CopyValue(from, this);
        }

        public void Move(int offset)
        {
            if (offset == 0)
                return;

            var dataSet = DataSet;
            if (dataSet == null)
                return;

            var newIndex = Index + offset;
            if (newIndex < 0 || newIndex > dataSet.Count - 1)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var restore = Backup();
            Model.LockEditingRow(() => dataSet.Remove(this));
            dataSet.Insert(newIndex, this, restore);
        }

        private Action<DataRow> Backup()
        {
            var dataSet = DataSet;

            if (Model.EditingRow == null && !HasChild)
            {
                Model.BeginEdit(this);
                return dataRow => dataRow.Model.EndEdit(dataRow);
            }
            else
            {
                var savedDataSet = dataSet.Clone();
                savedDataSet.AddRow(dataRow => dataRow.CopyValuesFrom(this));
                return dataRow => dataRow.CopyValuesFrom(savedDataSet[0]);
            }
        }

        private bool HasChild
        {
            get
            {
                for (int i = 0; i < _childDataSets.Length; i++)
                {
                    if (_childDataSets[i].Count > 0)
                        return true;
                }

                return false;
            }
        }

        public bool BeginEdit()
        {
            if (Model == null || Model.EditingRow != null)
                return false;

            Model.BeginEdit(this);
            return true;
        }

        public bool EndEdit()
        {
            if (Model == null || Model.EditingRow != this)
                return false;

            Model.EndEdit(this);
            return true;
        }

        public bool CancelEdit()
        {
            if (Model == null || Model.EditingRow != this)
                return false;

            Model.CancelEdit();
            return true;
        }

        internal DataRow AncestorOf(int ancestorLevel)
        {
            var dataRow = this;
            Debug.Assert(ancestorLevel >= 0);
            for (int i = 0; i < ancestorLevel; i++)
                dataRow = dataRow.ParentDataRow;
            return dataRow;
        }

        private int _suspendUpdatedCount;
        private IColumnSet _pendingUpdatedColumns = ColumnSet.Empty;
        private IColumnSet _pendingComputationColumns = ColumnSet.Empty;
        private IColumnSet _handledColumns = ColumnSet.Empty;
        private bool _isFiringEvent;

        public void SuspendUpdated()
        {
            if (this == Placeholder)
                return;
            _suspendUpdatedCount++;
        }

        public void ResumeUpdated()
        {
            if (this == Placeholder)
                return;
            ResumeUpdated(false);
        }

        internal void ResumeUpdated(bool omitHandler)
        {
            if (_suspendUpdatedCount == 1)
            {
                if (omitHandler)
                    _pendingUpdatedColumns = _pendingComputationColumns = _handledColumns = ColumnSet.Empty;
                else
                    HandlesUpdated();
            }
            _suspendUpdatedCount--;
        }

        private void HandlesUpdated()
        {
            for (var pendingColumns = HandlePendingColumns(); pendingColumns.Count > 0; pendingColumns = HandlePendingColumns())
                Model.HandlesDataRowUpdated(this, pendingColumns);

            if (_handledColumns.Count > 0)
            {
                UpdateDataSetRevision();
                var updatedColumns = FireUpdatedEvent();
                RefreshCascadeComputations(updatedColumns);
            }
        }

        private void UpdateDataSetRevision()
        {
            BaseDataSet.UpdateRevision();
            if (DataSet != BaseDataSet)
                DataSet.UpdateRevision();
        }

        private IColumnSet FireUpdatedEvent()
        {
            Debug.Assert(_handledColumns.Count > 0);
            _isFiringEvent = true;
            var result = _handledColumns.Seal();
            _handledColumns = ColumnSet.Empty;
            try
            {
                Model.FireDataRowUpdatedEvent(this, result);
            }
            finally
            {
                _isFiringEvent = false;
            }
            return result;
        }

        private void RefreshCascadeComputations(IColumnSet updatedColumns)
        {
            var cascadeComputations = Model.GetCascadeAffectedColumns(updatedColumns);
            if (cascadeComputations.Count == 0)
                return;
            foreach (var keyValuePair in cascadeComputations)
            {
                var model = keyValuePair.Key;
                Debug.Assert(model.Depth != Model.Depth);
                var columns = keyValuePair.Value;
                if (model.Depth < Model.Depth)
                    AncestorOf(Model.Depth - model.Depth).RefreshComputations(columns);
                else
                    RefreshDescendentComputations(model, columns);
            }
        }

        private void RefreshDescendentComputations(Model decendent, IColumnSet columnSet)
        {
            var childModel = ChildAncestorOf(decendent);
            var childDataSet = this[childModel];
            if (childModel == decendent)
            {
                for (int i = 0; i < childDataSet.Count; i++)
                    childDataSet[i].RefreshComputations(columnSet);
            }
            else
            {
                for (int i = 0; i < childDataSet.Count; i++)
                    childDataSet[i].RefreshDescendentComputations(decendent, columnSet);
            }
        }

        private Model ChildAncestorOf(Model decendent)
        {
            for (; decendent != null; decendent = decendent.ParentModel)
            {
                if (decendent.ParentModel == Model)
                    return decendent;
            }
            return null;
        }

        private IColumnSet HandlePendingColumns()
        {
            var result = ColumnSet.Empty.Union(_pendingUpdatedColumns).Union(_pendingComputationColumns).Seal();
            _handledColumns = _handledColumns.Union(result);
            _pendingUpdatedColumns = _pendingComputationColumns = ColumnSet.Empty;
            return result;
        }

        internal void OnUpdated(Column column)
        {
            if (_isFiringEvent)
                throw new InvalidOperationException(Strings.DataRow_UpdateInDataRowChangedEventNotAllowed);

            SuspendUpdated();
            _pendingUpdatedColumns = _pendingUpdatedColumns.Add(column);
            ResumeUpdated();
        }

        internal void RefreshComputations(IColumnSet computationColumns)
        {
            SuspendUpdated();
            foreach (var computationColumn in computationColumns)
            {
                if (computationColumn.RefreshComputation(this))
                    _pendingComputationColumns = _pendingComputationColumns.Add(computationColumn);
            }
            ResumeUpdated();
        }

        internal void Update(Action<DataRow> updateAction)
        {
            Check.NotNull(updateAction, nameof(updateAction));

            SuspendUpdated();
            try
            {
                updateAction(this);
            }
            finally
            {
                ResumeUpdated();
            }
        }
    }
}
