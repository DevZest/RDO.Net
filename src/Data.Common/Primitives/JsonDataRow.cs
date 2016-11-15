using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public static class JsonDataRow
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, DataRow dataRow)
        {
            jsonWriter.WriteStartObject();

            var columns = dataRow.Model.Columns;
            int count = 0;
            foreach (var column in columns)
            {
                if (!column.ShouldSerialize)
                    continue;

                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WriteObjectName(column.Name);
                var dataSetColumn = column as IDataSetColumn;
                if (dataSetColumn != null)
                    dataSetColumn.Serialize(dataRow.Ordinal, jsonWriter);
                else
                    jsonWriter.WriteValue(column.Serialize(dataRow.Ordinal));
                count++;
            }

            foreach (var dataSet in dataRow.ChildDataSets)
            {
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WriteObjectName(dataSet.Model.Name).Write(dataSet);
                count++;
            }

            return jsonWriter.WriteEndObject();
        }

        public static void Parse(this JsonParser jsonParser, DataRow dataRow)
        {
            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);

            var token = jsonParser.PeekToken();
            if (token.Kind == JsonTokenKind.String)
            {
                jsonParser.ConsumeToken();
                jsonParser.Parse(dataRow, token.Text);

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    token = jsonParser.ExpectToken(JsonTokenKind.String);
                    jsonParser.Parse(dataRow, token.Text);
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);
        }

        private static void Parse(this JsonParser jsonParser, DataRow dataRow, string memberName)
        {
            jsonParser.ExpectToken(JsonTokenKind.Colon);

            var model = dataRow.Model;
            var member = model[memberName];
            if (member == null)
                throw new FormatException(Strings.JsonParser_InvalidModelMember(memberName, model.GetType().FullName));
            var column = member as Column;
            if (column != null)
                jsonParser.Parse(column, dataRow.Ordinal);
            else
                jsonParser.Parse(dataRow[(Model)member], false);
        }

        private static void Parse(this JsonParser jsonParser, Column column, int ordinal)
        {
            Debug.Assert(column != null);

            var dataSetColumn = column as IDataSetColumn;
            if (dataSetColumn != null)
            {
                var dataSet = jsonParser.Parse(() => dataSetColumn.NewValue(ordinal), false);
                if (column.ShouldSerialize)
                    dataSetColumn.Deserialize(ordinal, dataSet);
                return;
            }

            var token = jsonParser.ExpectToken(JsonTokenKind.ColumnValues);
            if (column.ShouldSerialize)
                column.Deserialize(ordinal, token.JsonValue);
        }
    }
}
