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
