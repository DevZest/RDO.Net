using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public struct ValidationEntry
    {
        public ValidationEntry(DataRow dataRow, IValidationMessageGroup messages)
        {
            Check.NotNull(dataRow, nameof(dataRow));
            Check.NotNull(messages, nameof(messages));
            if (messages.Count == 0)
                throw new ArgumentException(Strings.ValidationEntry_EmptyMessages, nameof(messages));
            DataRow = dataRow;
            Messages = messages.Seal();
        }

        public readonly DataRow DataRow;

        public readonly IValidationMessageGroup Messages;

        public bool IsEmpty
        {
            get { return DataRow == null; }
        }

        public string ToJsonString(bool isPretty)
        {
            return JsonWriter.New().Write(this).ToString(isPretty);
        }

        public static ValidationEntry ParseJson(DataSet dataSet, string json)
        {
            var jsonParser = new JsonParser(json);
            var result = jsonParser.ParseValidationEntry(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.Eof);
            return result;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }
    }
}
