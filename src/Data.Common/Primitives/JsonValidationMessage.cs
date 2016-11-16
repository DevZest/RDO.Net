using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonValidationMessage
    {
        private const string MESSAGE_ID = nameof(ValidationMessage.Id);
        private const string SEVERITY = nameof(ValidationMessage.Severity);
        private const string COLUMNS = nameof(ValidationMessage.Columns);
        private const string DESCRIPTION = nameof(ValidationMessage.Description);

        public static JsonWriter Write(this JsonWriter jsonWriter, ValidationMessage validationMessage)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(MESSAGE_ID, validationMessage.Id).WriteComma()
                .WriteNameStringPair(SEVERITY, validationMessage.Severity.ToString()).WriteComma()
                .WriteNameStringPair(COLUMNS, validationMessage.Columns.Serialize()).WriteComma()
                .WriteNameStringPair(DESCRIPTION, validationMessage.Description)
                .WriteEndObject();
        }

        public static ValidationMessage ParseValidationMessage(this JsonParser jsonParser, DataSet dataSet)
        {
            string messageId;
            ValidationSeverity severity;
            IColumnSet columns;
            string description;

            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            messageId = jsonParser.ExpectNameStringPair(MESSAGE_ID, true);
            severity = (ValidationSeverity)Enum.Parse(typeof(ValidationSeverity), jsonParser.ExpectNameStringPair(SEVERITY, true));
            columns = ColumnSet.Deserialize(dataSet.Model, jsonParser.ExpectNameStringPair(COLUMNS, true));
            description = jsonParser.ExpectNameStringPair(DESCRIPTION, false);
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);

            return new ValidationMessage(messageId, severity, columns, description);
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
