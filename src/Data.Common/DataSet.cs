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
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (dataRow.ParentDataRow != null)
                throw new ArgumentException(Strings.DataSet_InvalidNewDataRow, nameof(dataRow));

            InternalInsert(index, dataRow);

            Model.HandlesDataRowInserting(dataRow);
            if (updateAction != null)
            {
                dataRow.SuspendUpdated();
                try
                {
                    updateAction(dataRow);
                }
                finally
                {
                    dataRow.ResetUpdated();
                }
            }
            Model.HandlesDataRowInserted(dataRow);
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

            InternalRemoveAt(index, true);
        }

        internal void InternalRemoveAt(int index, bool notifyChange)
        {
            Debug.Assert(index >= 0 && index < Count);

            UpdateRevision();
            var dataRow = this[index];
            if (notifyChange)
            {
                var baseDataSet = dataRow.BaseDataSet;
                var ordinal = dataRow.Ordinal;
                var dataSet = dataRow.DataSet;
                Model.HandlesDataRowRemoving(dataRow, baseDataSet, ordinal, dataSet, index);
                dataRow.SuspendUpdated();
                InternalRemoveAtCore(index, dataRow);
                dataRow.ResetUpdated();
                Model.HandlesDataRowRemoved(dataRow, baseDataSet, ordinal, dataSet, index);
            }
            else
                InternalRemoveAtCore(index, dataRow);
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
                InternalRemoveAt(i, true);
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

        private abstract class ValidationEntriesCounter
        {
            public static ValidationEntriesCounter Create(ValidationSeverity severity, int maxEntries)
            {
                return new SingleSeverityCounter(severity, maxEntries);
            }

            public static ValidationEntriesCounter Create(int maxErrors, int maxWarnings)
            {
                return new AllSeverityCounter(maxErrors, maxWarnings);
            }

            protected ValidationEntriesCounter()
            {
            }

            public abstract bool HasNext { get; }

            public abstract ValidationEntry? Next(DataRow dataRow);

            private sealed class SingleSeverityCounter : ValidationEntriesCounter
            {
                public SingleSeverityCounter(ValidationSeverity severity, int maxEntries)
                {
                    _severity = severity;
                    _maxEntries = maxEntries;
                }

                private ValidationSeverity _severity;
                private int _maxEntries;
                private int _count;

                public override bool HasNext
                {
                    get { return _count < _maxEntries; }
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
                public AllSeverityCounter(int maxErrors, int maxWarnings)
                {
                    _maxErrors = maxErrors;
                    _maxWarnings = maxWarnings;
                }

                private int _countError, _countWarning, _maxErrors, _maxWarnings;

                public override bool HasNext
                {
                    get { return _countError < _maxErrors || _countWarning < _maxWarnings; }
                }

                private bool HasValidator(Model model)
                {
                    bool nextError = _countError < _maxErrors;
                    bool nextWarning = _countWarning < _maxWarnings;

                    foreach (var validator in model.Validators)
                    {
                        if (nextError && validator.Severity == ValidationSeverity.Error)
                            return true;

                        if (nextWarning && validator.Severity == ValidationSeverity.Warning)
                            return true;
                    }
                    return false;
                }

                private static void Check(IReadOnlyList<ValidationMessage> validationMessages, out bool hasError, out bool hasWarning)
                {
                    hasError = hasWarning = false;
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
                        }

                        if (hasError && hasWarning)
                            return;
                    }
                }

                public override ValidationEntry? Next(DataRow dataRow)
                {
                    if (!HasNext || !HasValidator(dataRow.Model))
                        return null;

                    var validationMessages = dataRow.Validate(null);
                    if (validationMessages.Count == 0)
                        return new ValidationEntry();

                    bool hasError, hasWarning;
                    Check(validationMessages, out hasError, out hasWarning);

                    bool emptyEntry = true;
                    if (hasError)
                    {
                        if (_countError < _maxErrors)
                            emptyEntry = false;
                        _countError++;
                    }
                    if (hasWarning)
                    {
                        if (_countWarning < _maxWarnings)
                            emptyEntry = false;
                        _countWarning++;
                    }
                    return emptyEntry ? new ValidationEntry() : new ValidationEntry(dataRow, validationMessages);
                }
            }
        }

        public IValidationResult Validate(bool recursive = true, int maxErrorEntries = 100, int maxWarningEntries = 100)
        {
            return Validate(ValidationResult.Empty, this, ValidationEntriesCounter.Create(maxErrorEntries, maxWarningEntries), recursive);
        }

        public IValidationResult Validate(ValidationSeverity severity = ValidationSeverity.Error, bool recursive = true, int maxEntries = 100)
        {
            return Validate(ValidationResult.Empty, this, ValidationEntriesCounter.Create(severity, maxEntries), recursive);
        }

        private static IValidationResult Validate(IValidationResult result, DataSet dataSet, ValidationEntriesCounter entriesCounter, bool recursive)
        {
            foreach (var dataRow in dataSet)
            {
                var entry = entriesCounter.Next(dataRow);
                if (!entry.HasValue)
                    return result;

                var entryValue = entry.GetValueOrDefault();
                if (!entryValue.IsEmpty)
                    result = result.Add(entryValue);

                if (recursive)
                {
                    var childModels = dataSet.Model.ChildModels;
                    foreach (var childModel in childModels)
                    {
                        if (!entriesCounter.HasNext)
                            break;
                        var childDataSet = dataRow[childModel];
                        result = Validate(result, childDataSet, entriesCounter, recursive);
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
