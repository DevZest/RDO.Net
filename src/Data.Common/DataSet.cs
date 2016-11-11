using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        internal DataRow AddRow(Action<DataRow> updateAction = null)
        {
            var result = new DataRow();
            Insert(Count, result, updateAction);
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

        internal void Insert(int index, DataRow dataRow, Action<DataRow> updateAction)
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

        public ValidationResult Validate(ValidationSeverity severity = ValidationSeverity.Error, bool recursive = true)
        {
            return ValidationResult.New(Validate(this, severity, recursive));
        }

        private static IEnumerable<ValidationEntry> Validate(DataSet dataSet, ValidationSeverity severity, bool recursive)
        {
            foreach (var dataRow in dataSet)
            {
                foreach (var message in dataRow.Validate())
                {
                    if (message.Severity >= severity)
                        yield return new ValidationEntry(dataRow, message);
                }

                if (recursive)
                {
                    var childModels = dataSet.Model.ChildModels;
                    foreach (var childModel in childModels)
                    {
                        var childDataSet = dataRow[childModel];
                        foreach (var entry in Validate(childDataSet, severity, recursive))
                            yield return entry;
                    }
                }
            }
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

        public DataRow BeginAdd()
        {
            if (IsReadOnly)
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);

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
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);

            if (EditingRow != DataRow.Placeholder)
                return null;

            var result = new DataRow();
            Insert(index, result, dataRow => Model.EndEdit(dataRow));
            return result;
        }

        public bool CancelAdd()
        {
            if (IsReadOnly)
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);

            if (EditingRow != DataRow.Placeholder)
                return false;

            Model.CancelEdit();
            return true;
        }
    }
}
