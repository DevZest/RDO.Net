using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonValidationResult
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, IValidationResult validationResult)
        {
            return jsonWriter.WriteArray(validationResult, (writer, entry) => writer.Write(entry));
        }

        public static IValidationResult ParseValidationResult(this JsonParser jsonParser, DataSet dataSet)
        {
            IValidationResult result = ValidationResult.Empty;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                result = result.Add(jsonParser.ParseValidationEntry(dataSet));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result = result.Add(jsonParser.ParseValidationEntry(dataSet));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);

            return result;
        }
    }
}
