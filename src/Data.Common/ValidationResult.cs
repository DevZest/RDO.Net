using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DevZest.Data
{
    public struct ValidationResult
    {
        internal static ValidationResult New(IEnumerable<ValidationEntry> entries)
        {
            var array = entries == null ? null : entries.ToArray();
            return array == null || array.Length == 0 ? new ValidationResult() : new ValidationResult(array);
        }

        public static ValidationResult ParseJson(DataSet dataSet, string json)
        {
            var validationMessages = DataSet<_ValidationMessage>.ParseJson(json);
            return Deserialize(dataSet, validationMessages);
        }

        public static ValidationResult Deserialize(DataSet dataSet, DataSet<_ValidationMessage> validationMessages)
        {
            Check.NotNull(dataSet, nameof(dataSet));

            if (validationMessages == null || validationMessages.Count == 0)
                return new ValidationResult();

            var entries = new ValidationEntry[validationMessages.Count];
            for (int i = 0; i < validationMessages.Count; i++)
            {
                var dataRow = DataRow.FromString(dataSet, validationMessages._.DataRow[i]);
                var validatorId = ValidatorId.Deserialize(validationMessages._.ValidatorId[i]);
                var validationLevel = (ValidationLevel)validationMessages._.ValidationLevel[i];
                var columns = DeserializeColumns(dataRow, validationMessages._.Columns[i]);
                var description = validationMessages._.Description[i];
                entries[i] = new ValidationEntry(dataRow, new ValidationMessage(validatorId, validationLevel, description, columns));
            }

            return new ValidationResult(entries);
        }

        private readonly IReadOnlyList<ValidationEntry> _entries;

        private ValidationResult(IReadOnlyList<ValidationEntry> entries)
        {
            Debug.Assert(entries != null && entries.Count > 0);
            _entries = entries;
        }

        private static string SerializeColumns(IReadOnlyList<Column> columns)
        {
            return columns == null || columns.Count == 0 ? string.Empty : string.Join(",", columns.Select(x => x.Name));
        }

        private static IColumnSet DeserializeColumns(DataRow dataRow, string input)
        {
            if (string.IsNullOrEmpty(input))
                return ColumnSet.Empty;

            var columnNames = input.Split(',');
            if (columnNames == null || columnNames.Length == 0)
                return ColumnSet.Empty;

            var result = new Column[columnNames.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = dataRow.DeserializeColumn(columnNames[i]);

            return ColumnSet.Create(result);
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }

        public string ToJsonString(bool isPretty)
        {
            return DataSet.ToJsonString(ToDataSet(), isPretty);
        }

        public DataSet<_ValidationMessage> ToDataSet()
        {
            if (_entries == null)
                return null;

            var result = DataSet<_ValidationMessage>.New();
            foreach (var entry in _entries)
            {
                var dataRow = entry.DataRow;
                var validationMessage = entry.Message;
                var index = result.AddRow().Ordinal;
                result._.DataRow[index] = dataRow.ToString();
                result._.ValidatorId[index] = validationMessage.ValidatorId.ToString();
                result._.ValidationLevel[index] = (int)validationMessage.Level;
                result._.Columns[index] = SerializeColumns(validationMessage.Columns);
                result._.Description[index] = validationMessage.Description;
            }
            return result;
        }

        internal DataSet NewDataSet()
        {
            return DataSet<_ValidationMessage>.New();
        }

        public int Count
        {
            get { return _entries == null ? 0 : _entries.Count; }
        }

        public ValidationEntry this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _entries[index];
            }
        }

        public bool IsValid
        {
            get { return Count == 0; }
        }
    }
}
