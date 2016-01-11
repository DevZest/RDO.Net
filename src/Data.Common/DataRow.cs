using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a row of in-memory data.
    /// </summary>
    public sealed class DataRow
    {
        /// <summary>Initializes a new instance of <see cref="DataRow"/> object.</summary>
        public DataRow()
        {
            Ordinal = -1;
            _index = -1;
            _eventArgs = new DataRowEventArgs(this);
        }

        private DataSet[] _childDataSets;

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

        public DataSet DataSet
        {
            get
            {
                var parentRow = ParentDataRow;
                return parentRow == null ? DataSet.FromModel(Model) : parentRow[Model];
            }
        }

        internal void InitializeBySubDataSet(DataRow parent, int index)
        {
            Debug.Assert(ParentDataRow == null);
            Debug.Assert(parent != null);

            ParentDataRow = parent;
            _index = index;
        }

        internal void DisposeBySubDataSet()
        {
            ParentDataRow = null;
            _index = -1;
        }

        internal void InitializeByGlobalDataSet(Model model, int ordinal)
        {
            Debug.Assert(Model == null);
            Debug.Assert(model != null);

            Model = model;
            Ordinal = ordinal;

            model.EnsureChildModelsInitialized();
            var childModels = model.ChildModels;
            _childDataSets = new DataSet[childModels.Count];
            for (int i = 0; i < childModels.Count; i++)
                _childDataSets[i] = childModels[i].DataSet.CreateSubDataSet(this);

            var columns = model.Columns;
            foreach (var column in columns)
                column.InsertRow(this);
        }

        internal void DisposeByGlobalDataSet()
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

        internal void BuildJsonString(StringBuilder stringBuilder)
        {
            stringBuilder.Append('{');

            var columns = Model.Columns;
            int count = 0;
            foreach (var column in columns)
            {
                if (!column.ShouldSerialize)
                    continue;

                if (count > 0)
                    stringBuilder.Append(',');
                BuildJsonObjectName(stringBuilder, column.Name);
                var dataSetColumn = column as IDataSetColumn;
                if (dataSetColumn != null)
                    dataSetColumn.Serialize(Ordinal, stringBuilder);
                else
                    column.Serialize(Ordinal).Write(stringBuilder);
                count++;
            }

            foreach (var dataSet in _childDataSets)
            {
                if (count > 0)
                    stringBuilder.Append(',');
                BuildJsonObjectName(stringBuilder, dataSet.Model.Name);
                dataSet.BuildJsonString(stringBuilder);
                count++;
            }

            stringBuilder.Append('}');
        }

        private static void BuildJsonObjectName(StringBuilder stringBuilder, string name)
        {
            stringBuilder.Append("\"");
            stringBuilder.Append(name);
            stringBuilder.Append("\"");
            stringBuilder.Append(":");
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

        internal Column DeserializeColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return null;

            var result = Model.Columns[columnName];
            if (result == null)
                throw new FormatException();
            return result;
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

        private readonly DataRowEventArgs _eventArgs;

        internal void OnUpdated(IModelSet modelSet)
        {
            OnUpdated();
            if (ParentDataRow != null)
                ParentDataRow.BubbleUpdatedEvent(modelSet);
        }

        internal void OnUpdated()
        {
            _validationMessages = null;
            Model.OnRowUpdated(_eventArgs);
            if (ParentDataRow != null)
                DataSet.OnRowUpdated(_eventArgs);
            Model.DataSet.OnRowUpdated(_eventArgs);
        }

        internal void OnAdded()
        {
            Model.OnRowAdded(_eventArgs);
            if (ParentDataRow != null)
                DataSet.OnRowAdded(_eventArgs);
            Model.DataSet.OnRowAdded(_eventArgs);

            if (ParentDataRow != null)
                ParentDataRow.BubbleUpdatedEvent(Model);
        }

        internal void OnRemoved(DataRowRemovedEventArgs e)
        {
            Debug.Assert(e.DataRow == this);

            e.Model.OnRowRemoved(e);
            if (e.ParentDataRow != null)
                e.DataSet.OnRowRemoved(e);
            e.Model.DataSet.OnRowRemoved(e);

            if (e.ParentDataRow != null)
                e.ParentDataRow.BubbleUpdatedEvent(e.Model);
        }

        private void BubbleUpdatedEvent(IModelSet modelSet)
        {
            if (ShouldRaiseUpdatedEvent(modelSet))
            {
                modelSet = modelSet.Union(Model);
                OnUpdated();
            }

            var parentDataRow = ParentDataRow;
            if (parentDataRow != null)
                parentDataRow.BubbleUpdatedEvent(modelSet);
        }

        private bool ShouldRaiseUpdatedEvent(IModelSet modelSet)
        {
            foreach (var column in Model.Columns)
            {
                var computation = column.GetComputation();
                if (computation == null)
                    continue;

                if (computation.AggregateModelSet.ContainsAny(modelSet))
                    return true;
            }

            foreach (var validator in Model.Validators)
            {
                var condition = validator.IsValidCondition;
                if (condition.AggregateModelSet.ContainsAny(modelSet))
                    return true;
            }

            return false;
        }

        private class ValidationMessageCollection : ReadOnlyCollection<ValidationMessage>
        {
            public ValidationMessageCollection()
                : base(new List<ValidationMessage>())
            {
            }

            public void Add(ValidationMessage item)
            {
                Items.Add(item);
            }
        }

        private static ValidationMessageCollection s_emptyValidationMessages = new ValidationMessageCollection();
        private ValidationMessageCollection _validationMessages;

        public ReadOnlyCollection<ValidationMessage> ValidationMessages
        {
            get
            {
                if (_validationMessages != null)
                    return _validationMessages;

                _validationMessages = s_emptyValidationMessages;
                foreach (var validator in Model.Validators)
                {
                    var isValid = validator.IsValidCondition[this];
                    if (isValid == true)
                        continue;

                    if (_validationMessages == s_emptyValidationMessages)
                        _validationMessages = new ValidationMessageCollection();
                    var message = validator.Message[this];
                    _validationMessages.Add(new ValidationMessage(validator.Id, validator.Level, validator.Columns, message));
                }

                return _validationMessages;
            }
        }

        private ValidationMessageCollection _mergedValidationMessages = s_emptyValidationMessages;
        public ReadOnlyCollection<ValidationMessage> MergedValidationMessages
        {
            get { return _mergedValidationMessages; }
        }

        internal void Merge(ValidationResult result)
        {
            var oldValue = _mergedValidationMessages;
            _mergedValidationMessages = s_emptyValidationMessages;
            for (int i = 0; i < result.Count; i++)
            {
                var entry = result[i];
                if (entry.DataRow != this)
                    continue;

                if (_mergedValidationMessages == s_emptyValidationMessages)
                    _mergedValidationMessages = new ValidationMessageCollection();
                _mergedValidationMessages.Add(entry.Message);
            }

            if (_mergedValidationMessages != oldValue)
                OnUpdated();
        }

        public void Save()
        {
            foreach (var column in Model.Columns)
                column.Save(this);
        }

        public void Load()
        {
            foreach (var column in Model.Columns)
                column.Load(this);
        }
    }
}
