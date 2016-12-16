using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonValidationMessage
    {
        private const string MESSAGE_ID = nameof(ValidationMessage<Column>.Id);
        private const string SEVERITY = nameof(ValidationMessage<Column>.Severity);
        private const string DESCRIPTION = nameof(ValidationMessage<Column>.Description);
        private const string SOURCE = nameof(ValidationMessage<Column>.Source);

        public static JsonWriter Write(this JsonWriter jsonWriter, ValidationMessage<Column> validationMessage)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(MESSAGE_ID, validationMessage.Id).WriteComma()
                .WriteNameStringPair(SEVERITY, validationMessage.Severity.ToString()).WriteComma()
                .WriteNameStringPair(DESCRIPTION, validationMessage.Description).WriteComma()
                .WriteSource(validationMessage.Source)
                .WriteEndObject();
        }

        private static JsonWriter WriteSource(this JsonWriter jsonWriter, IValidationSource<Column> source)
        {
            if (source == null)
                jsonWriter.WriteNameValuePair(SOURCE, JsonValue.Null);
            else
                jsonWriter.WriteNameStringPair(SOURCE, source.Serialize());
            return jsonWriter;
        }

        public static ValidationMessage<Column> ParseValidationMessage(this JsonParser jsonParser, DataSet dataSet)
        {
            string messageId;
            ValidationSeverity severity;
            string description;
            IValidationSource<Column> source;

            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            messageId = jsonParser.ExpectNameStringPair(MESSAGE_ID, true);
            severity = (ValidationSeverity)Enum.Parse(typeof(ValidationSeverity), jsonParser.ExpectNameStringPair(SEVERITY, true));
            description = jsonParser.ExpectNameStringPair(DESCRIPTION, true);
            source = jsonParser.ParseSource(dataSet, false);
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);

            return new ValidationMessage<Column>(messageId, severity, description, source);
        }

        private static IValidationSource<Column> ParseSource(this JsonParser jsonParser, DataSet dataSet, bool expectComma)
        {
            var text = jsonParser.ExpectNameNullableStringPair(SOURCE, expectComma);
            return text == null ? null : ValidationSource.Deserialize(dataSet.Model, text);
        }

        public static IReadOnlyList<ValidationMessage<Column>> ParseValidationMessages(this JsonParser jsonParser, DataSet dataSet)
        {
            List<ValidationMessage<Column>> result = null;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                if (result == null)
                    result = new List<ValidationMessage<Column>>();
                result.Add(jsonParser.ParseValidationMessage(dataSet));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result.Add(jsonParser.ParseValidationMessage(dataSet));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);

            if (result == null)
                return Array<ValidationMessage<Column>>.Empty;
            else
                return result;
        }
    }
}
