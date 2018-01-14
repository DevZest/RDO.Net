using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public struct DataValidationResult
    {
        public DataValidationResult(DataRow dataRow, IDataValidationErrors messages)
        {
            Check.NotNull(dataRow, nameof(dataRow));
            Check.NotNull(messages, nameof(messages));
            if (messages.Count == 0)
                throw new ArgumentException(DiagnosticMessages.ValidationEntry_EmptyMessages, nameof(messages));
            DataRow = dataRow;
            Errors = messages.Seal();
        }

        public readonly DataRow DataRow;

        public readonly IDataValidationErrors Errors;

        public bool IsEmpty
        {
            get { return DataRow == null; }
        }

        public string ToJsonString(bool isPretty)
        {
            return JsonWriter.New().Write(this).ToString(isPretty);
        }

        public static DataValidationResult ParseJson(DataSet dataSet, string json)
        {
            var jsonParser = new JsonParser(json);
            var result = jsonParser.ParseDataValidationResult(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.Eof);
            return result;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }
    }
}
