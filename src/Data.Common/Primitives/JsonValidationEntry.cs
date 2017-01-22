using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonValidationEntry
    {
        private const string DATA_ROW = nameof(ValidationEntry.DataRow);
        private const string MESSAGES = nameof(ValidationEntry.Messages);

        public static JsonWriter Write(this JsonWriter jsonWriter, ValidationEntry validationEntry)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(DATA_ROW, validationEntry.DataRow.ToString()).WriteComma()
                .WriteNameArrayPair(MESSAGES, validationEntry.Messages, (writer, validationMessage) => writer.Write(validationMessage))
                .WriteEndObject();
        }

        public static ValidationEntry ParseValidationEntry(this JsonParser jsonParser, DataSet dataSet)
        {
            DataRow dataRow;
            IReadOnlyList<ModelValidationMessage> validationMessages;

            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            dataRow = DataRow.FromString(dataSet, jsonParser.ExpectNameStringPair(DATA_ROW, true));
            jsonParser.ExpectObjectName(MESSAGES);
            validationMessages = jsonParser.ParseModelValidationMessages(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);

            return new ValidationEntry(dataRow, validationMessages);
        }

        public static IReadOnlyList<ValidationEntry> ParseValidationEntries(this JsonParser jsonParser, DataSet dataSet)
        {
            List<ValidationEntry> result = null;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                if (result == null)
                    result = new List<ValidationEntry>();
                result.Add(jsonParser.ParseValidationEntry(dataSet));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result.Add(jsonParser.ParseValidationEntry(dataSet));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);

            if (result == null)
                return Array<ValidationEntry>.Empty;
            else
                return result;
        }
    }
}
