namespace DevZest.Data.Primitives
{
    public static class JsonDataValidationResults
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, IDataValidationResults dataValidationResults)
        {
            return jsonWriter.WriteArray(dataValidationResults, (writer, entry) => writer.Write(entry));
        }

        public static IDataValidationResults ParseDataValidationResults(this JsonParser jsonParser, DataSet dataSet)
        {
            IDataValidationResults results = DataValidationResults.Empty;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                results = results.Add(jsonParser.ParseDataValidationResult(dataSet));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    results = results.Add(jsonParser.ParseDataValidationResult(dataSet));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);

            return results;
        }
    }
}
