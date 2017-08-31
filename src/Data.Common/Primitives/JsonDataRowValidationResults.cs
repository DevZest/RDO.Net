using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonDataRowValidationResults
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, IDataRowValidationResults validationResults)
        {
            return jsonWriter.WriteArray(validationResults, (writer, entry) => writer.Write(entry));
        }

        public static IDataRowValidationResults ParseValidationResult(this JsonParser jsonParser, DataSet dataSet)
        {
            IDataRowValidationResults results = DataRowValidationResults.Empty;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                results = results.Add(jsonParser.ParseValidationEntry(dataSet));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    results = results.Add(jsonParser.ParseValidationEntry(dataSet));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);

            return results;
        }
    }
}
