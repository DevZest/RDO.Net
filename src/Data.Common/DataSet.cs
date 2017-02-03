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
            return JsonWriter.New().Write(this).ToString(isPretty);
        }

        public bool AllowsKeyUpdate(bool value)
        {
            return Model.AllowsKeyUpdate(value);
        }

        private abstract class ValidationEntriesCounter
        {
            public static ValidationEntriesCounter Create(ValidationSeverity? severity, int maxEntries)
            {
                if (severity.HasValue)
                    return new SingleSeverityCounter(severity.GetValueOrDefault(), maxEntries);
                else
                    return new AllSeverityCounter(maxEntries);
            }

            protected ValidationEntriesCounter(int maxEntries)
            {
                MaxEntries = maxEntries;
            }

            protected int MaxEntries { get; private set; }

            public abstract bool HasNext { get; }

            public abstract ValidationEntry? Next(DataRow dataRow);

            private sealed class SingleSeverityCounter : ValidationEntriesCounter
            {
                public SingleSeverityCounter(ValidationSeverity severity, int maxEntries)
                    : base(maxEntries)
                {
                    _severity = severity;
                }

                private ValidationSeverity _severity;
                private int _count;

                public override bool HasNext
                {
                    get { return _count < MaxEntries; }
                }

                public override ValidationEntry? Next(DataRow dataRow)
                {
                    if (!HasNext)
                        return null;
                    var validationMessages = dataRow.Validate(_severity);
                    if (validationMessages.Count > 0)
                    {
                        _count++;
                        return new ValidationEntry(dataRow, validationMessages);
                    }
                    else
                        return new ValidationEntry();
                }
            }

            private sealed class AllSeverityCounter : ValidationEntriesCounter
            {
                public AllSeverityCounter(int maxEntries)
                    : base(maxEntries)
                {
                }

                private int _countError, _countWarning, _countHint;

                public override bool HasNext
                {
                    get { return _countError < MaxEntries || _countWarning < MaxEntries || _countHint < MaxEntries; }
                }

                public override ValidationEntry? Next(DataRow dataRow)
                {
                    if (!HasNext)
                        return null;

                    var validationMessages = dataRow.Validate(null);
                    if (validationMessages.Count == 0)
                        return new ValidationEntry();

                    bool hasError = false;
                    bool hasWarning = false;
                    bool hasHint = false;
                    for (int i = 0; i < validationMessages.Count; i++)
                    {
                        var severity = validationMessages[i].Severity;
                        switch (severity)
                        {
                            case ValidationSeverity.Error:
                                hasError = true;
                                break;
                            case ValidationSeverity.Warning:
                                hasWarning = true;
                                break;
                            case ValidationSeverity.Hint:
                                hasHint = true;
                                break;
                        }

                        if (hasError && hasWarning && hasHint)
                            break;
                    }

                    bool shouldYield = false;
                    if (hasError)
                    {
                        if (_countError < MaxEntries)
                            shouldYield = true;
                        _countError++;
                    }
                    if (hasWarning)
                    {
                        if (_countWarning < MaxEntries)
                            shouldYield = true;
                        _countWarning++;
                    }
                    if (hasHint)
                    {
                        if (_countHint < MaxEntries)
                            shouldYield = true;
                        _countHint++;
                    }
                    return shouldYield ? new ValidationEntry(dataRow, validationMessages) : new ValidationEntry();
                }
            }
        }

        public ValidationResult Validate(ValidationSeverity? severity = ValidationSeverity.Error, bool recursive = true, int maxEntries = 100)
        {
            if (maxEntries <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxEntries));
            return ValidationResult.New(Validate(this, ValidationEntriesCounter.Create(severity, maxEntries), recursive));
        }

        private static IEnumerable<ValidationEntry> Validate(DataSet dataSet, ValidationEntriesCounter entriesCounter, bool recursive)
        {
            foreach (var dataRow in dataSet)
            {
                var entry = entriesCounter.Next(dataRow);
                if (!entry.HasValue)
                    yield break;

                var entryValue = entry.GetValueOrDefault();
                if (!entryValue.IsEmpty)
                    yield return entryValue;

                if (recursive)
                {
                    var childModels = dataSet.Model.ChildModels;
                    foreach (var childModel in childModels)
                    {
                        if (!entriesCounter.HasNext)
                            break;
                        var childDataSet = dataRow[childModel];
                        foreach (var childEntry in Validate(childDataSet, entriesCounter, recursive))
                            yield return childEntry;
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
