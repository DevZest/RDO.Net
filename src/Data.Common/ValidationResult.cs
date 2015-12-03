using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DevZest.Data
{
    public struct ValidationResult
    {
        private sealed class _ValidationMessage : Model
        {
            public static readonly Accessor<_ValidationMessage, _String> DataRowAccessor = RegisterColumn((_ValidationMessage x) => x.DataRow);
            public static readonly Accessor<_ValidationMessage, _String> ValidatorIdAccessor = RegisterColumn((_ValidationMessage x) => x.ValidatorId);
            public static readonly Accessor<_ValidationMessage, _Int32> ValidationLevelAccessor = RegisterColumn((_ValidationMessage x) => x.ValidationLevel);
            public static readonly Accessor<_ValidationMessage, _String> ColumnsAccessor = RegisterColumn((_ValidationMessage x) => x.Columns);
            public static readonly Accessor<_ValidationMessage, _String> DescriptionAccessor = RegisterColumn((_ValidationMessage x) => x.Description);

            public _String DataRow { get; private set; }

            public _String ValidatorId { get; private set; }

            public _Int32 ValidationLevel { get; private set; }

            public new _String Columns { get; private set; }

            public _String Description { get; private set; }
        }

        internal static ValidationResult New()
        {
            return new ValidationResult(DataSet<_ValidationMessage>.New());
        }

        public static ValidationResult ParseJson(string json)
        {
            return new ValidationResult(DataSet<_ValidationMessage>.ParseJson(json));
        }

        private readonly DataSet<_ValidationMessage> _validationMessages;

        private ValidationResult(DataSet<_ValidationMessage> validationMessages)
        {
            _validationMessages = validationMessages;
        }

        internal void Add(DataRow dataRow, ValidationMessage validationMessage)
        {
            Debug.Assert(_validationMessages != null);

            var index = _validationMessages.AddRow().Ordinal;
            _validationMessages._.DataRow[index] = dataRow.ToString();
            _validationMessages._.ValidatorId[index] = validationMessage.ValidatorId.ToString();
            _validationMessages._.ValidationLevel[index] = (int)validationMessage.Level;
            _validationMessages._.Columns[index] = SerializeColumns(validationMessage.Columns);
            _validationMessages._.Description[index] = validationMessage.Description;
        }

        internal void UpdateValidationMessages(DataSet dataSet)
        {
            if (_validationMessages == null)
                return;

            for (int i = 0; i < _validationMessages.Count; i++)
            {
                var dataRow = DataRow.FromString(dataSet, _validationMessages._.DataRow[i]);
                var validatorId = ValidatorId.Deserialize(_validationMessages._.ValidatorId[i]);
                var validationLevel = (ValidationLevel)_validationMessages._.ValidationLevel[i];
                var columns = DeserializeColumns(dataRow, _validationMessages._.Columns[i]);
                var description = _validationMessages._.Description[i];
                dataRow.AddValidationMessage(new ValidationMessage(validatorId, validationLevel, description, columns));
            }
        }

        private static string SerializeColumns(IReadOnlyList<Column> columns)
        {
            return columns == null || columns.Count == 0 ? string.Empty : string.Join(",", columns.Select(x => x.Name));
        }

        private static IReadOnlyList<Column> s_emptyColumns = new Column[0];
        private static IReadOnlyList<Column> DeserializeColumns(DataRow dataRow, string input)
        {
            if (string.IsNullOrEmpty(input))
                return s_emptyColumns;

            var columnNames = input.Split(',');
            if (columnNames == null || columnNames.Length == 0)
                return s_emptyColumns;

            if (columnNames.Length == 1)
            {
                var column = dataRow.DeserializeColumn(columnNames[0]);
                if (column != null)
                    return column;
                return new Column[] { null };
            }

            var result = new Column[columnNames.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = dataRow.DeserializeColumn(columnNames[i]);

            return result;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }

        public string ToJsonString(bool isPretty)
        {
            return _validationMessages.ToJsonString(isPretty);
        }
    }
}
