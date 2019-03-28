using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonDataSet
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, DataSet dataSet)
        {
            return jsonWriter.InternalWrite(dataSet, dataSet);
        }

        public static JsonWriter Write(this JsonWriter jsonWriter, DataSet dataSet, IEnumerable<DataRow> dataRows)
        {
            return jsonWriter.InternalWrite(dataSet, dataRows);
        }

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
                    jsonReader.ExpectToken(JsonTokenKind.Eof);
                return null;
            }

            var result = dataSetCreator();
            jsonReader.Deserialize(result, isTopLevel);
            return result;
        }

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
                jsonReader.ExpectToken(JsonTokenKind.Eof);
        }
    }
}
