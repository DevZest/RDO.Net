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

        internal void WriteJson(StringBuilder stringBuilder)
        {
            stringBuilder
                .WriteStartObject()
                .WriteNameStringPair(nameof(DataRow), DataRow.ToString()).WriteComma()
                .WriteNameArrayPair(nameof(Messages), Messages, (sb, validationMessage) => sb.WriteValidationMessageJson(validationMessage))
                .WriteEndObject();
        }
    }

    internal static class ValidationEntryJson
    {
        internal static StringBuilder WriteValidationEntryJson(this StringBuilder stringBuilder, ValidationEntry validationEntry)
        {
            validationEntry.WriteJson(stringBuilder);
            return stringBuilder;
        }
    }
}
