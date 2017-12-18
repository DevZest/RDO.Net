using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public struct DataRowValidationResult
    {
        public DataRowValidationResult(DataRow dataRow, IColumnValidationMessages messages)
        {
            Check.NotNull(dataRow, nameof(dataRow));
            Check.NotNull(messages, nameof(messages));
            if (messages.Count == 0)
                throw new ArgumentException(DiagnosticMessages.ValidationEntry_EmptyMessages, nameof(messages));
            DataRow = dataRow;
            Messages = messages.Seal();
        }

        public readonly DataRow DataRow;

        public readonly IColumnValidationMessages Messages;

        public bool IsEmpty
        {
            get { return DataRow == null; }
        }

        public string ToJsonString(bool isPretty)
        {
            return JsonWriter.New().Write(this).ToString(isPretty);
        }

        public static DataRowValidationResult ParseJson(DataSet dataSet, string json)
        {
            var jsonParser = new JsonParser(json);
            var result = jsonParser.ParseValidationEntry(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.Eof);
            return result;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }
    }
}
