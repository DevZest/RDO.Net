using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonValidationMessage
    {
        private const string ID = nameof(ValidationMessage.Id);
        private const string SEVERITY = nameof(ValidationMessage.Severity);
        private const string DESCRIPTION = nameof(ValidationMessage.Description);
        private const string COLUMNS = nameof(ValidationMessage.Columns);

        public static JsonWriter Write(this JsonWriter jsonWriter, ValidationMessage validationMessage)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(ID, validationMessage.Id).WriteComma()
                .WriteNameStringPair(SEVERITY, validationMessage.Severity.ToString()).WriteComma()
                .WriteNameStringPair(DESCRIPTION, validationMessage.Description).WriteComma()
                .WriteColumns(validationMessage.Columns)
                .WriteEndObject();
        }

        private static JsonWriter WriteColumns(this JsonWriter jsonWriter, IColumnSet columns)
        {
            if (columns == null)
                jsonWriter.WriteNameValuePair(COLUMNS, JsonValue.Null);
            else
                jsonWriter.WriteNameStringPair(COLUMNS, columns.Serialize());
            return jsonWriter;
        }

        public static ValidationMessage ParseValidationMessage(this JsonParser jsonParser, DataSet dataSet)
        {
            string messageId;
            Severity severity;
            string description;
            IColumnSet source;

            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            messageId = jsonParser.ExpectNameStringPair(ID, true);
            severity = (Severity)Enum.Parse(typeof(Severity), jsonParser.ExpectNameStringPair(SEVERITY, true));
            description = jsonParser.ExpectNameStringPair(DESCRIPTION, true);
            source = jsonParser.ParseColumns(dataSet, false);
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);

            return new ValidationMessage(messageId, severity, description, source);
        }

        private static IColumnSet ParseColumns(this JsonParser jsonParser, DataSet dataSet, bool expectComma)
        {
            var text = jsonParser.ExpectNameNullableStringPair(COLUMNS, expectComma);
            return text == null ? null : ColumnSet.Deserialize(dataSet.Model, text);
        }

        public static IReadOnlyList<ValidationMessage> ParseValidationMessages(this JsonParser jsonParser, DataSet dataSet)
        {
            List<ValidationMessage> result = null;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                if (result == null)
                    result = new List<ValidationMessage>();
                result.Add(jsonParser.ParseValidationMessage(dataSet));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result.Add(jsonParser.ParseValidationMessage(dataSet));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);

            if (result == null)
                return Array<ValidationMessage>.Empty;
            else
                return result;
        }
    }
}
