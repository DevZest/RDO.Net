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

        public static DataValidationError ParseValidationMessage(this JsonParser jsonParser, DataSet dataSet)
        {
            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            var message = jsonParser.ExpectNameStringPair(DESCRIPTION, true);
            var source = jsonParser.ParseColumns(dataSet, false);
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);

            return new DataValidationError(message, source);
        }

        private static IColumns ParseColumns(this JsonParser jsonParser, DataSet dataSet, bool expectComma)
        {
            var text = jsonParser.ExpectNameNullableStringPair(SOURCE, expectComma);
            return text == null ? null : Columns.Deserialize(dataSet.Model, text);
        }

        public static IDataValidationErrors ParseDataValidationErrors(this JsonParser jsonParser, DataSet dataSet)
        {
            IDataValidationErrors result = DataValidationErrors.Empty;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                result = result.Add(jsonParser.ParseValidationMessage(dataSet));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result.Add(jsonParser.ParseValidationMessage(dataSet));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);

            return result;
        }
    }
}
