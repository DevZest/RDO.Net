using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data
{
    /// <summary>
    /// Represents an in-memory collection of data.
    /// </summary>
    public abstract class DataSet : DataSource, IList<DataRow>, IReadOnlyList<DataRow>, IJsonView
    {
        /// <summary>
        /// Clones this DataSet.
        /// </summary>
        /// <returns>The cloned DataSet.</returns>
        public DataSet Clone()
        {
            return InternalClone();
        }

        internal abstract DataSet InternalClone();

        /// <inheritdoc/>
        public sealed override DataSourceKind Kind
        {
            get { return DataSourceKind.DataSet; }
        }

        internal abstract DataSet CreateChildDataSet(DataRow parentRow);

        /// <summary>
        /// Adds a DataRow into this DataSet.
        /// </summary>
        /// <param name="updateAction">The delegate to initialize the newly added DataRow.</param>
        /// <returns>The newly added DataRow.</returns>
        public DataRow AddRow(Action<DataRow> updateAction = null)
        {
            return InsertRow(Count, updateAction);
        }

        /// <summary>
        /// Inserts a DataRow into this DataSet at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <param name="updateAction">The delegate to initialize the newly added DataRow.</param>
        /// <returns>The newly added DataRow.</returns>
        public DataRow InsertRow(int index, Action<DataRow> updateAction = null)
        {
            var result = new DataRow();
            Insert(index, result, updateAction);
            return result;
        }

        internal readonly List<DataRow> _rows = new List<DataRow>();

        /// <summary>
        /// Gets the parent DataRow.
        /// </summary>
        public abstract DataRow ParentDataRow { get; }

        /// <summary>
        /// Gets the count of data rows.
        /// </summary>
        public int Count
        {
            get { return _rows.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this DataSet is readonly.
        /// </summary>
        /// <remarks>The child base DataSet is readonly. You need to manipulate via child DataSet instead.</remarks>
        public abstract bool IsReadOnly { get; }

        /// <summary>Gets or sets the <see cref="DataRow"/> at specified index.</summary>
        /// <param name="index">The zero-based index of the <see cref="DataRow"/> to get or set.</param>
        /// <returns>The <see cref="DataRow"/> at the specified index.</returns>
        public DataRow this[int index]
        {
            get { return _rows[index]; }
            set
            {
                if (this[index] == value)
                    return;

                RemoveAt(index);
                Insert(index, value);
            }
        }

        /// <summary>
        /// Gets the index of specified <see cref="DataRow"/> in this DataSet.
        /// </summary>
        /// <param name="dataRow">The specified <see cref="DataRow"/>.</param>
        /// <returns>Index in this DataSet.</returns>
        public abstract int IndexOf(DataRow dataRow);

        /// <summary>
        /// Determines whether this DataSet contains specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dataRow">The specified <see cref="DataRow"/>.</param>
        /// <returns><see langword="true" /> if this DataSet contains specified DataRow, otherwise <see langword="false" />.</returns>
        public bool Contains(DataRow dataRow)
        {
            return IndexOf(dataRow) != -1;
        }

        /// <summary>
        /// Inserts DataRow at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <param name="dataRow">The DataRow to be inserted.</param>
        public void Insert(int index, DataRow dataRow)
        {
            Insert(index, dataRow, null);
        }

        /// <summary>
        /// Inserts DataRow at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <param name="dataRow">The DataRow to be inserted.</param>
        /// <param name="updateAction">A delegate to initialize the DataRow.</param>
        public void Insert(int index, DataRow dataRow, Action<DataRow> updateAction)
        {
            dataRow.VerifyNotNull(nameof(dataRow));
            VerifyNotReadOnly();

            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (dataRow.Ordinal >= 0)
                throw new ArgumentException(DiagnosticMessages.DataSet_InvalidNewDataRow, nameof(dataRow));

            InternalInsert(index, dataRow);

            Model.HandlesDataRowInserted(dataRow, updateAction);
        }

        internal void InternalInsert(int index, DataRow dataRow)
        {
            UpdateRevision();
            CoreInsert(index, dataRow);
        }

        internal abstract void CoreInsert(int index, DataRow dataRow);

        /// <summary>
        /// Removes specified DataRow from this DataSet.
        /// </summary>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <returns><see langword="true" /> if DataRow removed successfully, otherwise <see langword="false"/>.</returns>
        public bool Remove(DataRow dataRow)
        {
            var index = IndexOf(dataRow);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes DataRow at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        public void RemoveAt(int index)
        {
            VerifyNotReadOnly();
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            OuterRemoveAt(index);
        }

        internal void OuterRemoveAt(int index)
        {
            var dataRow = this[index];
            Model.HandlesDataRowRemoving(dataRow);
            var baseDataSet = dataRow.BaseDataSet;
            var ordinal = dataRow.Ordinal;
            var dataSet = dataRow.DataSet;
            InnerRemoveAt(index);
            Model.HandlesDataRowRemoved(dataRow, baseDataSet, ordinal, dataSet, index);
        }

        internal void InnerRemoveAt(int index)
        {
            UpdateRevision();
            CoreRemoveAt(index, this[index]);
        }

        internal abstract void CoreRemoveAt(int index, DataRow dataRow);

        /// <summary>
        /// Adds DataRow into this DataSet.
        /// </summary>
        /// <param name="dataRow">The DataRow to be added.</param>
        public void Add(DataRow dataRow)
        {
            Insert(Count, dataRow, null);
        }

        /// <summary>
        /// Adds DataRow into this DataSet.
        /// </summary>
        /// <param name="dataRow">The DataRow to be added.</param>
        /// <param name="updateAction">A delegate to initialize the DataRow.</param>
        public void Add(DataRow dataRow, Action<DataRow> updateAction)
        {
            Insert(Count, dataRow, updateAction);
        }

        /// <summary>
        /// Removes all DataRows from this DataSet.
        /// </summary>
        public void Clear()
        {
            VerifyNotReadOnly();

            for (int i = Count - 1; i >= 0; i--)
                OuterRemoveAt(i);
        }

        /// <summary>
        /// Copies all DataRow objects in this DataSet to an array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="arrayIndex">The start index of the target array to receive DataRow objects.</param>
        public void CopyTo(DataRow[] array, int arrayIndex)
        {
            _rows.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returs an enumerator for this DataSet.
        /// </summary>
        /// <returns>The enumerator for this DataSet.</returns>
        public IEnumerator<DataRow> GetEnumerator()
        {
            foreach (var dataRow in _rows)
                yield return dataRow;
        }

        /// <summary>
        /// Returs an enumerator for this DataSet.
        /// </summary>
        /// <returns>The enumerator for this DataSet.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToJsonString(isPretty: true);
        }

        /// <summary>
        /// Serializes this DataSet into JSON string.
        /// </summary>
        /// <param name="isPretty">Specifies whether serialized JSON string should be indented.</param>
        /// <param name="customizer">The customizer in serialization.</param>
        /// <returns></returns>
        public string ToJsonString(bool isPretty, IJsonCustomizer customizer = null)
        {
            return JsonWriter.Create(customizer).Write(this).ToString(isPretty);
        }

        /// <summary>
        /// Serializes specified DataRow objects in this DataSet into JSON string.
        /// </summary>
        /// <param name="dataRows">Specifies the DataRow objects to be serialized.</param>
        /// <param name="isPretty">Specifies whether serialized JSON string should be indented.</param>
        /// <param name="customizer">The customizer in serialization.</param>
        /// <returns></returns>
        public string ToJsonString(IEnumerable<DataRow> dataRows, bool isPretty, IJsonCustomizer customizer = null)
        {
            dataRows.VerifyNotNull(nameof(dataRows));
            return JsonWriter.Create(customizer).Write(this, dataRows).ToString(isPretty);
        }

        /// <summary>
        /// Validates all DataRow objects in this DataSet.
        /// </summary>
        /// <param name="recursive">Determines whether child DataSet should also be validated.</param>
        /// <param name="maxErrorRows">Specifies the maxium number of rows which will contain validation error.</param>
        /// <returns>The validation result.</returns>
        public IDataValidationResults Validate(bool recursive = true, int maxErrorRows = 100)
        {
            return Validate(DataValidationResults.Empty, this, maxErrorRows, recursive).Seal();
        }

        private static IDataValidationResults Validate(IDataValidationResults result, DataSet dataSet, int maxErrorRows, bool recursive)
        {
            foreach (var dataRow in dataSet)
            {
                if (maxErrorRows > 0 && result.Count >= maxErrorRows)
                    return result;

                var errors = dataRow.Validate();
                if (errors != null && errors.Count > 0)
                    result = result.Add(new DataValidationResult(dataRow, errors));

                if (recursive)
                {
                    var childModels = dataSet.Model.ChildModels;
                    foreach (var childModel in childModels)
                    {
                        var childDataSet = dataRow[childModel];
                        result = Validate(result, childDataSet, maxErrorRows, recursive);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the DataRow that is currently in editing mode.
        /// </summary>
        public DataRow EditingRow
        {
            get { return Model.EditingRow; }
        }

        /// <summary>
        /// Gets the DataRow that is currently in adding mode.
        /// </summary>
        public DataRow AddingRow
        {
            get { return Model.AddingRow; }
        }

        /// <summary>
        /// Validates the DataRow that is currently in adding mode.
        /// </summary>
        /// <returns></returns>
        public IDataValidationErrors ValidateAddingRow()
        {
            var addingRow = AddingRow;
            if (addingRow == null)
                throw new InvalidOperationException(DiagnosticMessages.DataSet_NullAddingRow);
            return Model.Validate(addingRow).Seal();
        }

        /// <summary>
        /// Enters the adding mode.
        /// </summary>
        /// <returns>The new DataRow that is in adding mode.</returns>
        public DataRow BeginAdd()
        {
            VerifyNotReadOnly();

            if (EditingRow != null)
                return null;

            var result = new DataRow(Model);
            Model.BeginEdit(result);
            return result;
        }

        /// <summary>
        /// Commits the adding DataRow into this DataSet.
        /// </summary>
        /// <returns>The DataRow which is newly added into this DataSet.</returns>
        public DataRow EndAdd()
        {
            return EndAdd(Count);
        }

        /// <summary>
        /// Commits the adding DataRow into this DataSet at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <returns>The DataRow which is newly added into this DataSet.</returns>
        public DataRow EndAdd(int index)
        {
            VerifyNotReadOnly();

            if (AddingRow == null)
                return null;

            var result = AddingRow;
            result.ResetModel();
            Insert(index, result, dataRow => Model.EndEdit(true));
            return result;
        }

        /// <summary>
        /// Cancels the Adding mode.
        /// </summary>
        /// <returns><see langword="true" /> if adding mode cancelled successfully, otherwise <see langword="false" />.</returns>
        public bool CancelAdd()
        {
            VerifyNotReadOnly();

            var addingRow = AddingRow;
            if (addingRow == null)
                return false;

            addingRow.ResetModel();
            Model.CancelEdit();
            return true;
        }

        private void VerifyNotReadOnly()
        {
            if (IsReadOnly)
                throw new NotSupportedException(DiagnosticMessages.DataSet_VerifyNotReadOnly);
        }

        /// <inheritdoc />
        IJsonView IJsonView.GetChildView(DataSet childDataSet)
        {
            return childDataSet;
        }

        JsonFilter IJsonView.Filter
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the <see cref="DataSetContainer"/> of this DataSet.
        /// </summary>
        public DataSetContainer Container
        {
            get { return Model.DataSetContainer; }
        }

        /// <summary>
        /// Deserializes JSON string into this DataSet.
        /// </summary>
        /// <param name="jsonString">The JSON string.</param>
        /// <param name="customizer">The customizer in deserialization.</param>
        public void Deserialize(string jsonString, IJsonCustomizer customizer = null)
        {
            jsonString.VerifyNotEmpty(nameof(jsonString));
            JsonReader.Create(jsonString, customizer).Deserialize(this, true);
        }
    }
}
