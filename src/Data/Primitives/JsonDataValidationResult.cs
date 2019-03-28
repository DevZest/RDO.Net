using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonDataValidationResult
    {
        private const string DATA_ROW = nameof(DataValidationResult.DataRow);
        private const string ERRORS = nameof(DataValidationResult.Errors);

        public static JsonWriter Write(this JsonWriter jsonWriter, DataValidationResult dataValidationResult)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(DATA_ROW, dataValidationResult.DataRow.ToString()).WriteComma()
                .WriteNameArrayPair<DataValidationError>(ERRORS, dataValidationResult.Errors, (writer, validationMessage) => writer.Write(validationMessage))
                .WriteEndObject();
        }

        public static DataValidationResult ParseDataValidationResult(this JsonReader jsonReader, DataSet dataSet)
        {
            DataRow dataRow;
            IDataValidationErrors validationErrors;

            jsonReader.ExpectToken(JsonTokenKind.CurlyOpen);
            dataRow = DataRow.FromString(dataSet, jsonReader.ExpectNameStringPair(DATA_ROW, true));
            jsonReader.ExpectObjectName(ERRORS);
            validationErrors = jsonReader.ParseDataValidationErrors(dataSet);
            jsonReader.ExpectToken(JsonTokenKind.CurlyClose);

            return new DataValidationResult(dataRow, validationErrors);
        }
    }
}
