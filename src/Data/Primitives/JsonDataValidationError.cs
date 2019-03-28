using System;

namespace DevZest.Data.Primitives
{
    public static class JsonDataValidationError
    {
        private const string DESCRIPTION = nameof(DataValidationError.Message);
        private const string SOURCE = nameof(DataValidationError.Source);

        public static JsonWriter Write(this JsonWriter jsonWriter, DataValidationError dataValidationError)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(DESCRIPTION, dataValidationError.Message).WriteComma()
                .WriteColumns(dataValidationError.Source)
                .WriteEndObject();
        }

        private static JsonWriter WriteColumns(this JsonWriter jsonWriter, IColumns columns)
        {
            if (columns == null)
                jsonWriter.WriteNameValuePair(SOURCE, JsonValue.Null);
            else
                jsonWriter.WriteNameStringPair(SOURCE, columns.Serialize());
            return jsonWriter;
        }

        public static DataValidationError ParseValidationMessage(this JsonReader jsonReader, DataSet dataSet)
        {
            jsonReader.ExpectToken(JsonTokenKind.CurlyOpen);
            var message = jsonReader.ExpectNameStringPair(DESCRIPTION, true);
            var source = jsonReader.ParseColumns(dataSet, false);
            jsonReader.ExpectToken(JsonTokenKind.CurlyClose);

            return new DataValidationError(message, source);
        }

        private static IColumns ParseColumns(this JsonReader jsonReader, DataSet dataSet, bool expectComma)
        {
            var text = jsonReader.ExpectNameNullableStringPair(SOURCE, expectComma);
            return text == null ? null : Columns.Deserialize(dataSet.Model, text);
        }

        public static IDataValidationErrors ParseDataValidationErrors(this JsonReader jsonReader, DataSet dataSet)
        {
            IDataValidationErrors result = DataValidationErrors.Empty;

            jsonReader.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonReader.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                result = result.Add(jsonReader.ParseValidationMessage(dataSet));

                while (jsonReader.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonReader.ConsumeToken();
                    result.Add(jsonReader.ParseValidationMessage(dataSet));
                }
            }

            jsonReader.ExpectToken(JsonTokenKind.SquaredClose);

            return result;
        }
    }
}
