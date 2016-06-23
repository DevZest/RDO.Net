using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DevZest.Data
{
    public abstract class DataSet : DataSource, IList<DataRow>
    {
        public static DataSet Get(Model model)
        {
            return model == null ? null : model.DataSet;
        }

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

        internal abstract DataSet CreateSubDataSet(DataRow parentRow);

        /// Creates a new instance of <see cref="DataRow"/> and add to this data set.
        /// </summary>
        /// <returns>The new <see cref="DataRow"/> object.</returns>
        public DataRow AddRow(Action<DataRow> updateAction = null)
        {
            var result = new DataRow();
            this.Add(result, updateAction);
            return result;
        }

        internal readonly List<DataRow> _rows = new List<DataRow>();

        public abstract DataRow ParentRow { get; }

        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count
        {
            get { return _rows.Count; }
        }

        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        public abstract bool IsReadOnly { get; }

        public event EventHandler<DataRow> RowAdded;

        internal void OnRowAdded(DataRow e)
        {
            var rowAdded = RowAdded;
            if (rowAdded != null)
                rowAdded(this, e);
        }

        public event EventHandler<DataRowRemovedEventArgs> RowRemoved;

        internal void OnRowRemoved(DataRowRemovedEventArgs e)
        {
            var rowRemoved = RowRemoved;
            if (rowRemoved != null)
                rowRemoved(this, e);
        }

        public event EventHandler<DataRow> RowUpdated;

        internal void OnRowUpdated(DataRow e)
        {
            UpdateRevision();
            var rowUpdated = RowUpdated;
            if (rowUpdated != null)
                rowUpdated(this, e);
        }

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
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (dataRow.ParentDataRow != null)
                throw new ArgumentException(Strings.DataSet_InvalidNewDataRow, nameof(dataRow));

            InternalInsert(index, dataRow);

            if (updateAction != null)
            {
                dataRow.BeginUpdate();
                try
                {
                    updateAction(dataRow);
                }
                finally
                {
                    dataRow.EndUpdate(true);
                }
            }
            dataRow.OnAdded();
        }

        internal void InternalInsert(int index, DataRow dataRow)
        {
            UpdateRevision();
            InternalInsertCore(index, dataRow);
        }

        internal abstract void InternalInsertCore(int index, DataRow dataRow);

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
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var dataRow = this[index];
            var e = new DataRowRemovedEventArgs(dataRow);
            InternalRemoveAt(index);
            dataRow.OnRemoved(e);
        }

        internal void InternalRemoveAt(int index)
        {
            Debug.Assert(index >= 0 && index < Count);

            UpdateRevision();
            InternalRemoveAtCore(index, this[index]);
        }

        internal abstract void InternalRemoveAtCore(int index, DataRow dataRow);

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
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);

            for (int i = Count - 1; i >= 0; i--)
                InternalRemoveAt(i);
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
            var result = new StringBuilder().WriteDataSet(this).ToString();
            if (isPretty)
                result = JsonFormatter.PrettyPrint(result);
            return result;
        }

        public bool AllowsKeyUpdate(bool value)
        {
            return Model.AllowsKeyUpdate(value);
        }

        public ValidationResult Validate(ValidationLevel validationLevel = ValidationLevel.Error, bool recursive = true)
        {
            return ValidationResult.New(Validate(this, validationLevel, recursive));
        }

        private static IEnumerable<ValidationEntry> Validate(DataSet dataSet, ValidationLevel validationLevel, bool recursive)
        {
            foreach (var dataRow in dataSet)
            {
                foreach (var message in dataRow.ValidationMessages)
                {
                    if (message.Level >= validationLevel)
                        yield return new ValidationEntry(dataRow, message);
                }

                if (recursive)
                {
                    var childModels = dataSet.Model.ChildModels;
                    foreach (var childModel in childModels)
                    {
                        var childDataSet = dataRow[childModel];
                        foreach (var entry in Validate(childDataSet, validationLevel, recursive))
                            yield return entry;
                    }
                }
            }
        }

        public void Merge(ValidationResult result)
        {
            MergeRecursively(this, result);
        }

        private static void MergeRecursively(DataSet dataSet, ValidationResult result)
        {
            foreach (var dataRow in dataSet)
            {
                dataRow.Merge(result);

                var childModels = dataSet.Model.ChildModels;
                foreach (var childModel in childModels)
                {
                    var childDataSet = dataRow[childModel];
                    MergeRecursively(childDataSet, result);
                }
            }
        }

        public DataRow EditingRow
        {
            get { return Model.EditingRow; }
        }
    }
}
