using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public struct DataValidationResult
    {
        public DataValidationResult(DataRow dataRow, IDataValidationErrors messages)
        {
            dataRow.VerifyNotNull(nameof(dataRow));
            messages.VerifyNotNull(nameof(messages));
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

        public string ToJsonString(bool isPretty, IJsonCustomizer customizer = null)
        {
            return JsonWriter.Create(customizer).Write(this).ToString(isPretty);
        }

        public static DataValidationResult ParseJson(DataSet dataSet, string json, IJsonCustomizer customizer = null)
        {
            var jsonReader = JsonReader.Create(json, customizer);
            var result = jsonReader.ParseDataValidationResult(dataSet);
            jsonReader.ExpectEof();
            return result;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }
    }
}
