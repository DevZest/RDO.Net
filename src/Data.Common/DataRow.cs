using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
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
            ChildOrdinal = -1;
        }

        private DataSet[] _childDataSets;

        /// <summary>Gets the <see cref="Model"/> which associated with this <see cref="DataRow"/>.</summary>
        public Model Model { get; private set; }

        internal int Ordinal { get; private set; }

        internal int ChildOrdinal { get; private set; }

        /// <summary>Gets the parent <see cref="DataRow"/>.</summary>
        public DataRow parentDataRow { get; private set; }

        internal void InitializeBySubDataSet(DataRow parent, int childOrdinal)
        {
            Debug.Assert(parentDataRow == null);
            Debug.Assert(parent != null);

            parentDataRow = parent;
            ChildOrdinal = childOrdinal;
        }

        internal void DisposeBySubDataSet()
        {
            parentDataRow = null;
        }

        internal void InitializeByMainDataSet(Model model, int ordinal)
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

        internal void DisposeByMainDataSet()
        {
            ClearChildren();

            var columns = Model.Columns;
            foreach (var column in columns)
                column.RemoveRow(this);

            Model = null;
        }

        internal void AdjustOrdinal(int value)
        {
            Debug.Assert(Ordinal != value);
            Ordinal = value;
        }

        internal void AdjustChildOrdinal(int value)
        {
            Debug.Assert(Ordinal != value);
            ChildOrdinal = value;
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

        public DataSet this[int ordinal]
        {
            get
            {
                if (ordinal < 0 || ordinal >= _childDataSets.Length)
                    throw new ArgumentOutOfRangeException(nameof(ordinal));
                return _childDataSets[ordinal];
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
            Utilities.Check.NotNull(childModel, nameof(childModel));
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

        public object this[Column column]
        {
            get { return column.GetValue(this); }
            set { column.SetValue(this, value); }
        }

        public object this[string columnName]
        {
            get { return Model.Columns[columnName].GetValue(this); }
            set { Model.Columns[columnName].SetValue(this, value); }
        }

        private static List<ValidationMessage> s_emptyValidationMessages = new List<ValidationMessage>();
        private List<ValidationMessage> _validationMessages = s_emptyValidationMessages;
        public IReadOnlyList<ValidationMessage> ValidationMessages
        {
            get { return _validationMessages; }
        }

        public void Validate()
        {
            _validationMessages = s_emptyValidationMessages;
            foreach (var validator in Model.Validators)
            {
                var isValid = validator.IsValidCondition.Eval(this);
                if (isValid == true)
                    continue;
                var message = validator.Message.Eval(this);
                if (_validationMessages == s_emptyValidationMessages)
                    _validationMessages = new List<ValidationMessage>();
                _validationMessages.Add(new ValidationMessage(validator.Id, validator.Level, message, validator.Columns));
            }
        }

        public override string ToString()
        {
            var parentDataRow = this.parentDataRow;
            if (this.parentDataRow == null)
                return string.Format(CultureInfo.InvariantCulture, "/[{0}]", Ordinal);

            var result = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", Model.Name, ChildOrdinal);
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
    }
}
