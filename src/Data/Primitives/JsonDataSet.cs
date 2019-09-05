using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Provides extension methods to serialize/deserialize DataSet to/from JSON.
    /// </summary>
    public static class JsonDataSet
    {
        /// <summary>
        /// Writes JSON for specified DataSet.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/>.</param>
        /// <param name="dataSet">The DataSet.</param>
        /// <returns>This <see cref="JsonWriter"/> for fluent coding.</returns>
        public static JsonWriter Write(this JsonWriter jsonWriter, DataSet dataSet)
        {
            return jsonWriter.InternalWrite(dataSet, dataSet);
        }

        /// <summary>
        /// Writes JSON for selected DataRow objects for specified DataSet.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/>.</param>
        /// <param name="dataSet">The DataSet.</param>
        /// <param name="dataRows">The selected DataRow objects.</param>
        /// <returns>This <see cref="JsonWriter"/> for fluent coding.</returns>
        public static JsonWriter Write(this JsonWriter jsonWriter, DataSet dataSet, IEnumerable<DataRow> dataRows)
        {
            return jsonWriter.InternalWrite(dataSet, dataRows);
        }

        /// <summary>
        /// Writes JSON for selected DataRow objects for specified JsonView.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/>.</param>
        /// <param name="jsonView">The JsonView.</param>
        /// <param name="dataRows">The selected DataRow objects.</param>
        /// <returns>This <see cref="JsonWriter"/> for fluent coding.</returns>
        public static JsonWriter Write(this JsonWriter jsonWriter, JsonView jsonView, IEnumerable<DataRow> dataRows)
        {
            return jsonWriter.InternalWrite(jsonView, dataRows);
        }

        internal static JsonWriter InternalWrite(this JsonWriter jsonWriter, IJsonView jsonView, IEnumerable<DataRow> dataRows)
        {
            jsonWriter.WriteStartArray();
            int count = 0;
            foreach (var dataRow in dataRows)
            {
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.Write(jsonView, dataRow);
                count++;
            }
            return jsonWriter.WriteEndArray();
        }

        /// <summary>
        /// Parses JSON into DataSet.
        /// </summary>
        /// <param name="jsonReader">The <see cref="JsonReader"/>.</param>
        /// <param name="dataSetCreator">Delegate to create DataSet.</param>
        /// <returns>The deserialized DataSet.</returns>
        public static DataSet Parse(this JsonReader jsonReader, Func<DataSet> dataSetCreator)
        {
            return jsonReader.Parse(dataSetCreator, isTopLevel: true);
        }

        internal static DataSet Parse(this JsonReader jsonReader, Func<DataSet> dataSetCreator, bool isTopLevel)
        {
            if (jsonReader.PeekToken().Kind == JsonTokenKind.Null)
            {
                jsonReader.ConsumeToken();
                if (isTopLevel)
                    jsonReader.ExpectEof();
                return null;
            }

            var result = dataSetCreator();
            jsonReader.Deserialize(result, isTopLevel);
            return result;
        }

        /// <summary>
        /// Deserializes JSON into data values of DataSet.
        /// </summary>
        /// <param name="jsonReader">The <see cref="JsonReader"/>.</param>
        /// <param name="dataSet">The data set.</param>
        /// <param name="isTopLevel">A value inidicates whether DataSet is top level which should consume all JSON data.</param>
        public static void Deserialize(this JsonReader jsonReader, DataSet dataSet, bool isTopLevel)
        {
            dataSet.VerifyNotNull(nameof(dataSet));

            var startWithSquaredOpen = jsonReader.PeekToken().Kind == JsonTokenKind.SquaredOpen;
            if (startWithSquaredOpen)
                jsonReader.ConsumeToken();

            if (jsonReader.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                var model = dataSet.Model;
                model.SuspendIdentity();

                dataSet.AddRow(x =>
                {
                    jsonReader.Deserialize(x);
                    x.IsPrimaryKeySealed = true;
                });

                while (startWithSquaredOpen && jsonReader.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonReader.ConsumeToken();
                    dataSet.AddRow(x =>
                    {
                        jsonReader.Deserialize(x);
                        x.IsPrimaryKeySealed = true;
                    });
                }

                model.ResumeIdentity();
            }
            else if (!startWithSquaredOpen)
                jsonReader.ExpectToken(JsonTokenKind.CurlyOpen);    // This will throw a FormatException.

            if (startWithSquaredOpen)
                jsonReader.ExpectToken(JsonTokenKind.SquaredClose);

            if (isTopLevel)
                jsonReader.ExpectEof();
        }
    }
}
