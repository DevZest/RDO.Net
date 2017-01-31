using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonColumnValidationMessage
    {
        private const string MESSAGE_ID = nameof(ColumnValidationMessage.Id);
        private const string SEVERITY = nameof(ColumnValidationMessage.Severity);
        private const string DESCRIPTION = nameof(ColumnValidationMessage.Description);
        private const string SOURCE = nameof(ColumnValidationMessage.Source);

        public static JsonWriter Write(this JsonWriter jsonWriter, ColumnValidationMessage validationMessage)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(MESSAGE_ID, validationMessage.Id).WriteComma()
                .WriteNameStringPair(SEVERITY, validationMessage.Severity.ToString()).WriteComma()
                .WriteNameStringPair(DESCRIPTION, validationMessage.Description).WriteComma()
                .WriteSource(validationMessage.Source)
                .WriteEndObject();
        }

        private static JsonWriter WriteSource(this JsonWriter jsonWriter, IColumnSet source)
        {
            if (source == null)
                jsonWriter.WriteNameValuePair(SOURCE, JsonValue.Null);
            else
                jsonWriter.WriteNameStringPair(SOURCE, source.Serialize());
            return jsonWriter;
        }

        public static ColumnValidationMessage ParseColumnValidationMessage(this JsonParser jsonParser, DataSet dataSet)
        {
            string messageId;
            ValidationSeverity severity;
            string description;
            IColumnSet source;

            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            messageId = jsonParser.ExpectNameStringPair(MESSAGE_ID, true);
            severity = (ValidationSeverity)Enum.Parse(typeof(ValidationSeverity), jsonParser.ExpectNameStringPair(SEVERITY, true));
            description = jsonParser.ExpectNameStringPair(DESCRIPTION, true);
            source = jsonParser.ParseSource(dataSet, false);
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);

            return new ColumnValidationMessage(messageId, severity, description, source);
        }

        private static IColumnSet ParseSource(this JsonParser jsonParser, DataSet dataSet, bool expectComma)
        {
            var text = jsonParser.ExpectNameNullableStringPair(SOURCE, expectComma);
            return text == null ? null : ColumnSet.Deserialize(dataSet.Model, text);
        }

        public static IReadOnlyList<ColumnValidationMessage> ParseColumnValidationMessages(this JsonParser jsonParser, DataSet dataSet)
        {
            List<ColumnValidationMessage> result = null;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                if (result == null)
                    result = new List<ColumnValidationMessage>();
                result.Add(jsonParser.ParseColumnValidationMessage(dataSet));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result.Add(jsonParser.ParseColumnValidationMessage(dataSet));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);

            if (result == null)
                return Array<ColumnValidationMessage>.Empty;
            else
                return result;
        }
    }
}
