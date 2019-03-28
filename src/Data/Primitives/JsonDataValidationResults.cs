namespace DevZest.Data.Primitives
{
    public static class JsonDataValidationResults
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, IDataValidationResults dataValidationResults)
        {
            return jsonWriter.WriteArray(dataValidationResults, (writer, entry) => writer.Write(entry));
        }

        public static IDataValidationResults ParseDataValidationResults(this JsonReader jsonReader, DataSet dataSet)
        {
            IDataValidationResults results = DataValidationResults.Empty;

            jsonReader.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonReader.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                results = results.Add(jsonReader.ParseDataValidationResult(dataSet));

                while (jsonReader.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonReader.ConsumeToken();
                    results = results.Add(jsonReader.ParseDataValidationResult(dataSet));
                }
            }

            jsonReader.ExpectToken(JsonTokenKind.SquaredClose);

            return results;
        }
    }
}
