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
            _validationMessages._.DataRow[index] = SerializeDataRow(dataRow);
            _validationMessages._.ValidatorId[index] = validationMessage.ValidatorId.ToString();
            _validationMessages._.ValidationLevel[index] = (int)validationMessage.Level;
            _validationMessages._.Columns[index] = SerializeColumns(validationMessage.Columns);
            _validationMessages._.Description[index] = validationMessage.Description;
        }

        private static string SerializeDataRow(DataRow dataRow)
        {
            var parentDataRow = dataRow.Parent;
            if (dataRow.Parent == null)
                return string.Format(CultureInfo.InvariantCulture, "[{0}]", dataRow.Ordinal);

            var result = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", dataRow.Model.Name, dataRow.ChildOrdinal);
            return SerializeDataRow(parentDataRow) + "/" + result;
        }

        private static string SerializeColumns(IReadOnlyList<Column> columns)
        {
            return columns == null || columns.Count == 0 ? string.Empty : string.Join(",", columns.Select(x => x.Name));
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
