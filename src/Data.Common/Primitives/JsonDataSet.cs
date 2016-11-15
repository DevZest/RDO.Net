using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonDataSet
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, IEnumerable<DataRow> dataSet)
        {
            jsonWriter.WriteStartArray();
            int count = 0;
            foreach (var dataRow in dataSet)
            {
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.Write(dataRow);
                count++;
            }
            return jsonWriter.WriteEndArray();
        }

        public static DataSet Parse(this JsonParser jsonParser, Func<DataSet> dataSetCreator)
        {
            return jsonParser.Parse(dataSetCreator, false);
        }

        internal static DataSet Parse(this JsonParser jsonParser, Func<DataSet> dataSetCreator, bool isTopLevel)
        {
            if (jsonParser.PeekToken().Kind == JsonTokenKind.Null)
            {
                jsonParser.ConsumeToken();
                if (isTopLevel)
                    jsonParser.ExpectToken(JsonTokenKind.Eof);
                return null;
            }

            var result = dataSetCreator();
            jsonParser.Parse(result, isTopLevel);
            return result;
        }

        internal static void Parse(this JsonParser jsonParser, DataSet dataSet, bool isTopLevel)
        {
            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            if (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                var model = dataSet.Model;
                model.EnterDataSetInitialization();

                jsonParser.Parse(dataSet.AddRow());

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    jsonParser.Parse(dataSet.AddRow());
                }

                model.ExitDataSetInitialization();
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);
            if (isTopLevel)
                jsonParser.ExpectToken(JsonTokenKind.Eof);
        }
    }
}
