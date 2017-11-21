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
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);
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
                throw new NotSupportedException(Strings.NotSupportedByReadOnlyList);

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

        private abstract class ValidationResultsCounter
        {
            public static ValidationResultsCounter Create(ValidationSeverity severity, int limit)
            {
                return new SingleSeverityCounter(severity, limit);
            }

            public static ValidationResultsCounter Create(int errorLimit, int warningLimit)
            {
                return new AllSeverityCounter(errorLimit, warningLimit);
            }

            protected ValidationResultsCounter()
            {
            }

            public abstract bool HasNext { get; }

            public abstract DataRowValidationResult? Next(DataRow dataRow);

            private sealed class SingleSeverityCounter : ValidationResultsCounter
            {
                public SingleSeverityCounter(ValidationSeverity severity, int limit)
                {
                    _severity = severity;
                    _limit = limit;
                }

                private ValidationSeverity _severity;
                private int _limit;
                private int _count;

                public override bool HasNext
                {
                    get { return _count < _limit; }
                }

                public override DataRowValidationResult? Next(DataRow dataRow)
                {
                    if (!HasNext)
                        return null;
                    var validationMessages = dataRow.Validate(_severity);
                    if (validationMessages.Count > 0)
                    {
                        _count++;
                        return new DataRowValidationResult(dataRow, validationMessages);
                    }
                    else
                        return new DataRowValidationResult();
                }
            }

            private sealed class AllSeverityCounter : ValidationResultsCounter
            {
                public AllSeverityCounter(int errorLimit, int warningLimit)
                {
                    _errorLimit = errorLimit;
                    _warningLimit = warningLimit;
                }

                private int _errorCount, _warningCount, _errorLimit, _warningLimit;

                public override bool HasNext
                {
                    get { return _errorCount < _errorLimit || _warningCount < _warningLimit; }
                }

                private bool HasValidator(Model model)
                {
                    bool nextError = _errorCount < _errorLimit;
                    bool nextWarning = _warningCount < _warningLimit;

                    var validators = model.Validators;
                    if (nextError && validators.Count > 0)
                        return true;
                    else if (nextWarning && validators.Count > 0)
                        return true;
                    else
                        return false;
                }

                private static void Check(IReadOnlyList<ColumnValidationMessage> validationMessages, out bool hasError, out bool hasWarning)
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

                public override DataRowValidationResult? Next(DataRow dataRow)
                {
                    if (!HasNext || !HasValidator(dataRow.Model))
                        return null;

                    var validationMessages = dataRow.Validate(null);
                    if (validationMessages.Count == 0)
                        return new DataRowValidationResult();

                    bool hasError, hasWarning;
                    Check(validationMessages, out hasError, out hasWarning);

                    bool emptyEntry = true;
                    if (hasError)
                    {
                        if (_errorCount < _errorLimit)
                            emptyEntry = false;
                        _errorCount++;
                    }
                    if (hasWarning)
                    {
                        if (_warningCount < _warningLimit)
                            emptyEntry = false;
                        _warningCount++;
                    }
                    return emptyEntry ? new DataRowValidationResult() : new DataRowValidationResult(dataRow, validationMessages);
                }
            }
        }

        public IDataRowValidationResults Validate(bool recursive = true, int errorLimit = 100, int warningLimit = 100)
        {
            return Validate(DataRowValidationResults.Empty, this, ValidationResultsCounter.Create(errorLimit, warningLimit), recursive);
        }

        public IDataRowValidationResults Validate(ValidationSeverity severity, bool recursive = true, int limit = 100)
        {
            return Validate(DataRowValidationResults.Empty, this, ValidationResultsCounter.Create(severity, limit), recursive);
        }

        private static IDataRowValidationResults Validate(IDataRowValidationResults result, DataSet dataSet, ValidationResultsCounter resultsCounter, bool recursive)
        {
            foreach (var dataRow in dataSet)
            {
                var entry = resultsCounter.Next(dataRow);
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
                        if (!resultsCounter.HasNext)
                            break;
                        var childDataSet = dataRow[childModel];
                        result = Validate(result, childDataSet, resultsCounter, recursive);
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

        public IColumnValidationMessages ValidateAddingRow(ValidationSeverity? severity = ValidationSeverity.Error)
        {
            var addingRow = AddingRow;
            if (addingRow == null)
                throw new InvalidOperationException(Strings.DataSet_NullAddingRow);
            return Model.Validate(addingRow, severity).Seal();
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

        public DataSetContainer Container
        {
            get { return Model.DataSetContainer; }
        }
    }
}
