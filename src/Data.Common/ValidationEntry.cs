using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Text;

namespace DevZest.Data
{
    public struct ValidationEntry
    {
        public ValidationEntry(DataRow dataRow, IReadOnlyList<ValidationMessage> messages)
        {
            DataRow = dataRow;
            Messages = messages;
        }

        public readonly DataRow DataRow;

        public readonly IReadOnlyList<ValidationMessage> Messages;

        internal void WriteJson(JsonWriter jsonWriter)
        {
            jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(nameof(DataRow), DataRow.ToString()).WriteComma()
                .WriteNameArrayPair(nameof(Messages), Messages, (writer, validationMessage) => writer.Write(validationMessage))
                .WriteEndObject();
        }
    }
}
