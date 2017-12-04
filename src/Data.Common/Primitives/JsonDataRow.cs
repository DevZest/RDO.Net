using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public static class JsonDataRow
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, DataRow dataRow, bool includeExtender = true)
        {
            jsonWriter.WriteStartObject();

            var columns = dataRow.Model.Columns;
            int count = 0;
            foreach (var column in columns)
            {
                if (!column.ShouldSerialize)
                    continue;

                if (column.Kind == ColumnKind.ColumnList || column.Kind == ColumnKind.Extender)
                    continue;

                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WriteObjectName(column.Name);
                jsonWriter.Write(dataRow, column);
                count++;
            }

            foreach (var columnList in dataRow.Model.ColumnLists)
            {
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WriteObjectName(columnList.Name);
                jsonWriter.WriteStartArray();
                for (int i = 0; i < columnList.Count; i++)
                {
                    if (i > 0)
                        jsonWriter.WriteComma();
                    jsonWriter.Write(dataRow, columnList[i]);
                }
                jsonWriter.WriteEndArray();
                count++;
            }

            if (includeExtender)
            {
                var extender = dataRow.Model.Extender;
                if (extender != null)
                {
                    if (count > 0)
                        jsonWriter.WriteComma();
                    jsonWriter.Write(extender, dataRow);
                    count++;
                }
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

        private static void Write(this JsonWriter jsonWriter, DataRow dataRow, Column column)
        {
            var dataSetColumn = column as IDataSetColumn;
            if (dataSetColumn != null)
                dataSetColumn.Serialize(dataRow.Ordinal, jsonWriter);
            else
                jsonWriter.WriteValue(column.Serialize(dataRow.Ordinal));
        }

        private static void Write(this JsonWriter jsonWriter, ModelExtender extender, DataRow dataRow)
        {
            jsonWriter.WriteObjectName(extender.Name);

            var count = 0;
            jsonWriter.WriteStartObject();
            foreach (var column in extender.Columns)
            {
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WriteObjectName(column.RelativeName);
                jsonWriter.Write(dataRow, column);
                count++;
            }
            foreach (var childExtender in extender.ChildExtenders)
            {
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.Write(childExtender, dataRow);
                count++;
            }
            jsonWriter.WriteEndObject();
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

            if (memberName == nameof(Model.Extender))
            {
                var extender = model.Extender;
                if (model.Extender == null)
                    throw new FormatException(Strings.JsonParser_InvalidModelMember(memberName, model.GetType().FullName));
                jsonParser.Parse(extender, dataRow);
            }
            else
            {
                var member = model[memberName];
                if (member == null)
                    throw new FormatException(Strings.JsonParser_InvalidModelMember(memberName, model.GetType().FullName));
                if (member is Column)
                    jsonParser.Parse((Column)member, dataRow.Ordinal);
                else if (member is ColumnList)
                    jsonParser.Parse((ColumnList)member, dataRow.Ordinal);
                else
                    jsonParser.Parse(dataRow[(Model)member], false);
            }
        }

        private static void Parse(this JsonParser jsonParser, ModelExtender extender, DataRow dataRow)
        {
            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            var token = jsonParser.PeekToken();
            if (token.Kind == JsonTokenKind.String)
            {
                jsonParser.ConsumeToken();
                jsonParser.Parse(extender, token.Text, dataRow);

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    token = jsonParser.ExpectToken(JsonTokenKind.String);
                    jsonParser.Parse(extender, token.Text, dataRow);
                }
            }
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);
        }

        private static void Parse(this JsonParser jsonParser, ModelExtender extender, string memberName, DataRow dataRow)
        {
            jsonParser.ExpectToken(JsonTokenKind.Colon);
            if (extender.ColumnsByRelativeName.ContainsKey(memberName))
                jsonParser.Parse(extender.ColumnsByRelativeName[memberName], dataRow.Ordinal);
            else if (extender.ChildExtendersByName.ContainsKey(memberName))
                jsonParser.Parse(extender.ChildExtendersByName[memberName], dataRow);
            else
                throw new FormatException(Strings.JsonParser_InvalidExtenderMember(memberName, extender.FullName));
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

        private static void Parse(this JsonParser jsonParser, ColumnList columnList, int ordinal)
        {
            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);
            for (int i = 0; i < columnList.Count; i++)
            {
                jsonParser.Parse(columnList[i], ordinal);
                if (i < columnList.Count - 1)
                    jsonParser.ExpectComma();
            }
            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);
        }
    }
}
