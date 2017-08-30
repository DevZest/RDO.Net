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
                .WriteNameArrayPair<ColumnValidationMessage>(MESSAGES, validationEntry.Messages, (writer, validationMessage) => writer.Write(validationMessage))
                .WriteEndObject();
        }

        public static ValidationEntry ParseValidationEntry(this JsonParser jsonParser, DataSet dataSet)
        {
            DataRow dataRow;
            IColumnValidationMessages validationMessages;

            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            dataRow = DataRow.FromString(dataSet, jsonParser.ExpectNameStringPair(DATA_ROW, true));
            jsonParser.ExpectObjectName(MESSAGES);
            validationMessages = jsonParser.ParseValidationMessageGroup(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);

            return new ValidationEntry(dataRow, validationMessages);
        }
    }
}
