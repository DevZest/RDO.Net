using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public class ValidationMessage : ValidationMessage<IColumnSet>
    {
        public ValidationMessage(string id, ValidationSeverity severity, string description, IColumnSet source)
            : base(id, severity, description, source)
        {
            Check.NotNull(source, nameof(source));
            if (source.Count == 0)
                throw new ArgumentException(Strings.ValidationMessage_EmptySourceColumns, nameof(source));
        }

        public string ToJsonString(bool isPretty)
        {
            return JsonWriter.New().Write(this).ToString(isPretty);
        }

        public static ValidationMessage ParseJson(DataSet dataSet, string json)
        {
            var jsonParser = new JsonParser(json);
            var result = jsonParser.ParseValidationMessage(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.Eof);
            return result;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }
    }
}
