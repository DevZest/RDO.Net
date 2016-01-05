using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DevZest.Data
{
    public delegate void RowCollectionChanged(DataSet dataSet, int oldIndex, DataRow dataRow);

    public delegate void ColumnValueChanged(DataSet dataSet, DataRow dataRow, Column column);

    public abstract class DataSet : DataSource, IList<DataRow>
    {
        internal DataSet(Model model)
            : base(model)
        {
        }

        /// <inheritdoc/>
        public sealed override DataSourceKind Kind
        {
            get { return DataSourceKind.DataSet; }
        }

        internal abstract DataSet CreateSubDataSet(DataRow parentRow);

        /// Creates a new instance of <see cref="DataRow"/> and add to this data set.
        /// </summary>
        /// <returns>The new <see cref="DataRow"/> object.</returns>
        public DataRow AddRow()
        {
            var result = new DataRow();
            this.Add(result);
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
            Check.NotNull(dataRow, nameof(dataRow));
            if (IsReadOnly)
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (dataRow.ParentDataRow != null)
                throw new ArgumentException(Strings.DataSet_InvalidNewDataRow, nameof(dataRow));

            InternalInsert(index, dataRow);
        }

        internal void InternalInsert(int index, DataRow dataRow)
        {
            InternalInsertCore(index, dataRow);
            OnRowCollectionChanged(-1, dataRow);
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

            InternalRemoveAt(index);
        }

        internal void InternalRemoveAt(int index)
        {
            Debug.Assert(index >= 0 && index < Count);

            var dataRow = this[index];
            InternalRemoveAtCore(index, dataRow);
            OnRowCollectionChanged(index, dataRow);
        }

        internal abstract void InternalRemoveAtCore(int index, DataRow dataRow);

        public void Add(DataRow dataRow)
        {
            Insert(Count, dataRow);
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
            var result = new StringBuilder();
            BuildJsonString(result);

            if (isPretty)
                return JsonFormatter.PrettyPrint(result.ToString());
            else
                return result.ToString();
        }

        internal static void BuildJsonString(DataSet dataSet, StringBuilder stringBuilder)
        {
            if (dataSet == null)
                stringBuilder.Append(JsonValue.Null.Text);
            else
                dataSet.BuildJsonString(stringBuilder);
        }

        internal void BuildJsonString(StringBuilder stringBuilder)
        {
            stringBuilder.Append('[');
            int count = 0;
            foreach (var dataRow in this)
            {
                if (count > 0)
                    stringBuilder.Append(',');
                dataRow.BuildJsonString(stringBuilder);
                count++;
            }
            stringBuilder.Append(']');
        }

        public bool AllowsKeyUpdate(bool value)
        {
            return Model.AllowsKeyUpdate(value);
        }

        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "No need to create EventArgs object each and every time.")]
        public event RowCollectionChanged RowCollectionChanged;

        private void OnRowCollectionChanged(int oldIndex, DataRow dataRow)
        {
            UpdateRevision();

            var rowCollectionChanged = RowCollectionChanged;
            if (rowCollectionChanged != null)
                rowCollectionChanged(this, oldIndex, dataRow);
        }

        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "No need to create EventArgs object each and every time.")]
        public event ColumnValueChanged ColumnValueChanged;

        internal void OnColumnValueChanged(DataRow dataRow, Column column)
        {
            UpdateRevision();

            var columnValueChanged = ColumnValueChanged;
            if (columnValueChanged != null)
                columnValueChanged(this, dataRow, column);
        }

        public ValidationResult Validate(ValidationLevel validationLevel = ValidationLevel.Error, bool recursive = true)
        {
            return ValidationResult.New(Validate(this, validationLevel, recursive));
        }

        private static IEnumerable<ValidationEntry> Validate(DataSet dataSet, ValidationLevel validationLevel, bool recursive)
        {
            foreach (var dataRow in dataSet)
            {
                foreach (var message in dataRow.Validate())
                {
                    if (message.Level >= validationLevel)
                        yield return new ValidationEntry(dataRow, message);
                }

                if (recursive)
                {
                    var childModels = dataSet.Model.ChildModels;
                    foreach (var childModel in childModels)
                    {
                        var childDataSet = childModel[dataRow];
                        foreach (var entry in Validate(childDataSet, validationLevel, recursive))
                            yield return entry;
                    }
                }
            }
        }
    }
}
