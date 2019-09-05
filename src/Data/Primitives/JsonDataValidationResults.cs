namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Provides extension methods to serialize/deserialize <see cref="IDataValidationResults"/> object to/from JSON.
    /// </summary>
    public static class JsonDataValidationResults
    {
        /// <summary>
        /// Writes <see cref="IDataValidationResults"/> object into JSON.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/>.</param>
        /// <param name="dataValidationResults">The <see cref="IDataValidationResults"/> object.</param>
        /// <returns>This <see cref="JsonWriter"/> for fluent coding.</returns>
        public static JsonWriter Write(this JsonWriter jsonWriter, IDataValidationResults dataValidationResults)
        {
            return jsonWriter.WriteArray(dataValidationResults, (writer, entry) => writer.Write(entry));
        }

        /// <summary>
        /// Parses JSON into <see cref="IDataValidationResults"/> object.
        /// </summary>
        /// <param name="jsonReader">The <see cref="JsonReader"/>.</param>
        /// <param name="dataSet">The DataSet of validation.</param>
        /// <returns>The <see cref="IDataValidationResults"/> object.</returns>
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
