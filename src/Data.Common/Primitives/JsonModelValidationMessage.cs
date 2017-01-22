using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonModelValidationMessage
    {
        private const string MESSAGE_ID = nameof(ModelValidationMessage.Id);
        private const string SEVERITY = nameof(ModelValidationMessage.Severity);
        private const string DESCRIPTION = nameof(ModelValidationMessage.Description);
        private const string SOURCE = nameof(ModelValidationMessage.Source);

        public static JsonWriter Write(this JsonWriter jsonWriter, ModelValidationMessage validationMessage)
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

        public static ModelValidationMessage ParseModelValidationMessage(this JsonParser jsonParser, DataSet dataSet)
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

            return new ModelValidationMessage(messageId, severity, description, source);
        }

        private static IValidationSource<Column> ParseSource(this JsonParser jsonParser, DataSet dataSet, bool expectComma)
        {
            var text = jsonParser.ExpectNameNullableStringPair(SOURCE, expectComma);
            return text == null ? null : ValidationSource.Deserialize(dataSet.Model, text);
        }

        public static IReadOnlyList<ModelValidationMessage> ParseModelValidationMessages(this JsonParser jsonParser, DataSet dataSet)
        {
            List<ModelValidationMessage> result = null;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                if (result == null)
                    result = new List<ModelValidationMessage>();
                result.Add(jsonParser.ParseModelValidationMessage(dataSet));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result.Add(jsonParser.ParseModelValidationMessage(dataSet));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);

            if (result == null)
                return Array<ModelValidationMessage>.Empty;
            else
                return result;
        }
    }
}
