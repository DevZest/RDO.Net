using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class DataSet : DataSource, IList<DataRow>
    {
        internal DataSet(Model model)
            : base(model)
        {
        }

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

        internal DataRow AddRow(Action<DataRow> updateAction = null)
        {
            var result = new DataRow();
            Insert(Count, result, updateAction);
            return result;
        }

        internal readonly List<DataRow> _rows = new List<DataRow>();

        public abstract DataRow ParentDataRow { get; }

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count
        {
            get { return _rows.Count; }
        }

        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
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

        /// <inheritdoc cref="IList{T}.IndexOf(T)"/>
        public abstract int IndexOf(DataRow dataRow);

        /// <inheritdoc cref="ICollection{T}.Contains(T)"/>
        public bool Contains(DataRow dataRow)
        {
            return IndexOf(dataRow) != -1;
        }

        /// <inheritdoc cref="IList{T}.InternalInsert(int, T)"/>
        public void Insert(int index, DataRow dataRow)
        {
            Insert(index, dataRow, null);
        }

        public void Insert(int index, DataRow dataRow, Action<DataRow> updateAction)
        {
            Check.NotNull(dataRow, nameof(dataRow));
            if (IsReadOnly)
                throw new NotSupportedException(DiagnosticMessages.NotSupportedByReadOnlyList);
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (dataRow.ParentDataRow != null)
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

        public bool Remove(DataRow dataRow)
        {
            var index = IndexOf(dataRow);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        /// <inheritdoc cref="IList{T}.RemoveAt(int)"/>
        public void RemoveAt(int index)
        {
            if (IsReadOnly)
                throw new NotSupportedException(DiagnosticMessages.NotSupportedByReadOnlyList);
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

        public void Add(DataRow dataRow)
        {
            Insert(Count, dataRow, null);
        }

        public void Add(DataRow dataRow, Action<DataRow> updateAction)
        {
            Insert(Count, dataRow, updateAction);
        }

        public void Clear()
        {
            if (IsReadOnly)
                throw new NotSupportedException(DiagnosticMessages.NotSupportedByReadOnlyList);

            for (int i = Count - 1; i >= 0; i--)
                OuterRemoveAt(i);
        }

        public void CopyTo(DataRow[] array, int arrayIndex)
        {
            _rows.CopyTo(array, arrayIndex);
        }

        public IEnumerator<DataRow> GetEnumerator()
        {
            foreach (var dataRow in _rows)
                yield return dataRow;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToJsonString(isPretty: true);
        }

        public static string ToJsonString(DataSet dataSet, bool isPretty)
        {
            return dataSet == null ? JsonValue.Null.Text : dataSet.ToJsonString(isPretty);
        }

        public string ToJsonString(bool isPretty)
        {
            return JsonWriter.New().Write(this).ToString(isPretty);
        }

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

        public DataRow EditingRow
        {
            get { return Model.EditingRow; }
        }

        public DataRow AddingRow
        {
            get
            {
                var editingRow = EditingRow;
                return editingRow == DataRow.Placeholder ? editingRow : null;
            }
        }

        public IDataValidationErrors ValidateAddingRow()
        {
            var addingRow = AddingRow;
            if (addingRow == null)
                throw new InvalidOperationException(DiagnosticMessages.DataSet_NullAddingRow);
            return Model.Validate(addingRow).Seal();
        }

        public DataRow BeginAdd()
        {
            if (IsReadOnly)
                throw new NotSupportedException(DiagnosticMessages.NotSupportedByReadOnlyList);

            if (EditingRow != null)
                return null;

            Model.BeginEdit(DataRow.Placeholder);
            return DataRow.Placeholder;
        }

        public DataRow EndAdd()
        {
            return EndAdd(Count);
        }

        public DataRow EndAdd(int index)
        {
            if (IsReadOnly)
                throw new NotSupportedException(DiagnosticMessages.NotSupportedByReadOnlyList);

            if (EditingRow != DataRow.Placeholder)
                return null;

            var result = new DataRow();
            Insert(index, result, dataRow => Model.EndEdit(dataRow, true));
            return result;
        }

        public bool CancelAdd()
        {
            if (IsReadOnly)
                throw new NotSupportedException(DiagnosticMessages.NotSupportedByReadOnlyList);

            if (EditingRow != DataRow.Placeholder)
                return false;

            Model.CancelEdit();
            return true;
        }

        public DataSetContainer Container
        {
            get { return Model.DataSetContainer; }
        }
    }
}
