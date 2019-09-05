namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Provides extension methods to serialize/deserialize DataValidationError to/from JSON.
    /// </summary>
    public static class JsonDataValidationError
    {
        private const string MESSAGE = nameof(DataValidationError.Message);
        private const string SOURCE = nameof(DataValidationError.Source);

        /// <summary>
        /// Writes specified DataValidationError into JSON.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/>.</param>
        /// <param name="dataValidationError">The DataValidationError object.</param>
        /// <returns>This <see cref="JsonWriter"/> for fluent coding.</returns>
        public static JsonWriter Write(this JsonWriter jsonWriter, DataValidationError dataValidationError)
        {
            return jsonWriter
                .WriteStartObject()
                .WriteNameStringPair(MESSAGE, dataValidationError.Message).WriteComma()
                .WriteColumns(dataValidationError.Source)
                .WriteEndObject();
        }

        private static JsonWriter WriteColumns(this JsonWriter jsonWriter, IColumns columns)
        {
            if (columns == null)
                jsonWriter.WriteNameValuePair(SOURCE, JsonValue.Null);
            else
                jsonWriter.WriteNameStringPair(SOURCE, columns.Serialize());
            return jsonWriter;
        }

        /// <summary>
        /// Parses JSON into DataValidationError object.
        /// </summary>
        /// <param name="jsonReader">The <see cref="JsonReader"/>.</param>
        /// <param name="dataSet">The DataSet of validation.</param>
        /// <returns>The DataValidationError object.</returns>
        public static DataValidationError ParseDataValidationError(this JsonReader jsonReader, DataSet dataSet)
        {
            jsonReader.ExpectToken(JsonTokenKind.CurlyOpen);
            var message = jsonReader.ExpectStringProperty(MESSAGE, true);
            var source = jsonReader.ParseColumns(dataSet, false);
            jsonReader.ExpectToken(JsonTokenKind.CurlyClose);

            return new DataValidationError(message, source);
        }

        private static IColumns ParseColumns(this JsonReader jsonReader, DataSet dataSet, bool expectComma)
        {
            var text = jsonReader.ExpectNullableStringProperty(SOURCE, expectComma);
            return text == null ? null : Columns.Deserialize(dataSet.Model, text);
        }

        /// <summary>
        /// Parses JSON into <see cref="IDataValidationErrors"/> object.
        /// </summary>
        /// <param name="jsonReader">The <see cref="JsonReader"/>.</param>
        /// <param name="dataSet">The DataSet of validation.</param>
        /// <returns>The <see cref="IDataValidationErrors"/> object.</returns>
        public static IDataValidationErrors ParseDataValidationErrors(this JsonReader jsonReader, DataSet dataSet)
        {
            IDataValidationErrors result = DataValidationErrors.Empty;

            jsonReader.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonReader.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                result = result.Add(jsonReader.ParseDataValidationError(dataSet));

                while (jsonReader.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonReader.ConsumeToken();
                    result.Add(jsonReader.ParseDataValidationError(dataSet));
                }
            }

            jsonReader.ExpectToken(JsonTokenKind.SquaredClose);

            return result;
        }
    }
}
