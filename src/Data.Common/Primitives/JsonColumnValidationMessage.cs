using System;

namespace DevZest.Data.Primitives
{
    public static class JsonColumnValidationMessage
    {
        private const string SEVERITY = nameof(ColumnValidationMessage.Severity);
        private const string DESCRIPTION = nameof(ColumnValidationMessage.Description);
        private const string SOURCE = nameof(ColumnValidationMessage.Source);

        public static JsonWriter Write(this JsonWriter jsonWriter, ColumnValidationMessage validationMessage)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(SEVERITY, validationMessage.Severity.ToString()).WriteComma()
                .WriteNameStringPair(DESCRIPTION, validationMessage.Description).WriteComma()
                .WriteColumns(validationMessage.Source)
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

        public static ColumnValidationMessage ParseValidationMessage(this JsonParser jsonParser, DataSet dataSet)
        {
            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            var severity = (ValidationSeverity)Enum.Parse(typeof(ValidationSeverity), jsonParser.ExpectNameStringPair(SEVERITY, true));
            var description = jsonParser.ExpectNameStringPair(DESCRIPTION, true);
            var source = jsonParser.ParseColumns(dataSet, false);
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);

            return new ColumnValidationMessage(severity, description, source);
        }

        private static IColumns ParseColumns(this JsonParser jsonParser, DataSet dataSet, bool expectComma)
        {
            var text = jsonParser.ExpectNameNullableStringPair(SOURCE, expectComma);
            return text == null ? null : Columns.Deserialize(dataSet.Model, text);
        }

        public static IColumnValidationMessages ParseValidationMessageGroup(this JsonParser jsonParser, DataSet dataSet)
        {
            IColumnValidationMessages result = ColumnValidationMessages.Empty;

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
