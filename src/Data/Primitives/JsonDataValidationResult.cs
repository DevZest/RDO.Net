namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Provides extension methods to serialize/deserialize DataValidationResult to/from JSON.
    /// </summary>
    public static class JsonDataValidationResult
    {
        private const string DATA_ROW = nameof(DataValidationResult.DataRow);
        private const string ERRORS = nameof(DataValidationResult.Errors);

        /// <summary>
        /// Writes DataValidationResult into JSON.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/>.</param>
        /// <param name="dataValidationResult">The DataValidationResult.</param>
        /// <returns>This <see cref="JsonWriter"/> for fluent coding.</returns>
        public static JsonWriter Write(this JsonWriter jsonWriter, DataValidationResult dataValidationResult)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(DATA_ROW, dataValidationResult.DataRow.ToString()).WriteComma()
                .WriteNameArrayPair<DataValidationError>(ERRORS, dataValidationResult.Errors, (writer, validationMessage) => writer.Write(validationMessage))
                .WriteEndObject();
        }

        /// <summary>
        /// Parses JSON into DataValidationResult object.
        /// </summary>
        /// <param name="jsonReader">The <see cref="JsonReader"/>.</param>
        /// <param name="dataSet">The DataSet of validation.</param>
        /// <returns>The DataValidationResult object.</returns>
        public static DataValidationResult ParseDataValidationResult(this JsonReader jsonReader, DataSet dataSet)
        {
            DataRow dataRow;
            IDataValidationErrors validationErrors;

            jsonReader.ExpectToken(JsonTokenKind.CurlyOpen);
            dataRow = DataRow.FromString(dataSet, jsonReader.ExpectStringProperty(DATA_ROW, true));
            jsonReader.ExpectPropertyName(ERRORS);
            validationErrors = jsonReader.ParseDataValidationErrors(dataSet);
            jsonReader.ExpectToken(JsonTokenKind.CurlyClose);

            return new DataValidationResult(dataRow, validationErrors);
        }
    }
}
