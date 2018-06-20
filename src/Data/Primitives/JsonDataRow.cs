﻿using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public static class JsonDataRow
    {
        public static JsonWriter Write(this JsonWriter jsonWriter, DataRow dataRow)
        {
            return Write(jsonWriter, dataRow.DataSet, dataRow);
        }

        public static JsonWriter Write(this JsonWriter jsonWriter, DataRow dataRow, JsonView jsonView)
        {
            return Write(jsonWriter, jsonView, dataRow);
        }

        internal static JsonWriter Write(this JsonWriter jsonWriter, IJsonView jsonView, DataRow dataRow)
        {
            jsonWriter.WriteStartObject();

            var jsonFilter = jsonView.Filter;
            var columns = dataRow.Model.Columns;
            int count = 0;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (!column.ShouldSerialize)
                    continue;

                if (column.Kind == ColumnKind.ColumnListItem || column.Kind == ColumnKind.ColumnGroupMember)
                    continue;

                if (jsonFilter != null && !jsonFilter.ShouldSerialize(column))
                    continue;

                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WriteObjectName(column.Name);
                jsonWriter.Write(dataRow, column);
                count++;
            }
            var columnLists = dataRow.Model.ColumnLists;
            for (int i =0; i < columnLists.Count; i++)
            {
                var columnList = columnLists[i];
                if (jsonFilter != null && !jsonFilter.ShouldSerialize(columnList))
                    continue;

                if (count > 0)
                    jsonWriter.WriteComma();

                jsonWriter.Write(dataRow, columnList);
                count++;
            }

            var ext = dataRow.Model.ExtraColumns;
            if (ext != null)
            {
                if (jsonFilter == null || jsonFilter.ShouldSerialize(ext))
                {
                    if (count > 0)
                        jsonWriter.WriteComma();
                    jsonWriter.Write(dataRow, ext, jsonFilter);
                    count++;
                }
            }

            var childDataSets = dataRow.ChildDataSets;
            for (int i = 0; i < childDataSets.Count; i++)
            {
                var dataSet = childDataSets[i];
                if (jsonFilter != null && !jsonFilter.ShouldSerialize(dataSet.Model))
                    continue;

                if (count > 0)
                    jsonWriter.WriteComma();
                var childJsonView = jsonView.GetChildView(dataSet);
                jsonWriter.WriteObjectName(dataSet.Model.Name).InternalWrite(childJsonView, dataSet);
                count++;
            }

            return jsonWriter.WriteEndObject();
        }

        private static void Write(this JsonWriter jsonWriter, DataRow dataRow, ColumnList columnList)
        {
            jsonWriter.WriteObjectName(columnList.Name);
            jsonWriter.WriteStartArray();
            for (int i = 0; i < columnList.Count; i++)
            {
                if (i > 0)
                    jsonWriter.WriteComma();
                jsonWriter.Write(dataRow, columnList[i]);
            }
            jsonWriter.WriteEndArray();
        }

        private static void Write(this JsonWriter jsonWriter, DataRow dataRow, Column column)
        {
            var dataSetColumn = column as IDataSetColumn;
            if (dataSetColumn != null)
                dataSetColumn.Serialize(dataRow.Ordinal, jsonWriter);
            else
                jsonWriter.WriteValue(column.Serialize(dataRow.Ordinal));
        }

        private static void Write(this JsonWriter jsonWriter, DataRow dataRow, Projection columnCombination, JsonFilter jsonFilter)
        {
            if (string.IsNullOrEmpty(columnCombination.Name))
                jsonWriter.WriteMembers(dataRow, columnCombination, jsonFilter);
            else
                jsonWriter.WriteExt(dataRow, columnCombination, jsonFilter);
        }

        private static void WriteExt(this JsonWriter jsonWriter, DataRow dataRow, Projection columnCombination, JsonFilter jsonFilter)
        {
            Debug.Assert(!string.IsNullOrEmpty(columnCombination.Name));
            jsonWriter.WriteObjectName(columnCombination.Name);
            jsonWriter.WriteStartObject();
            jsonWriter.WriteMembers(dataRow, columnCombination, jsonFilter);
            jsonWriter.WriteEndObject();
        }

        private static void WriteMembers(this JsonWriter jsonWriter, DataRow dataRow, Projection columnCombination, JsonFilter jsonFilter)
        {
            var count = 0;
            var columns = columnCombination.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (jsonFilter != null && !jsonFilter.ShouldSerialize(column))
                    continue;

                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WriteObjectName(column.RelativeName);
                jsonWriter.Write(dataRow, column);
                count++;
            }

            var children = columnCombination.Children;
            for (int i = 0; i < children.Count; i++)
            {
                var childContainer = children[i];
                if (jsonFilter != null && !jsonFilter.ShouldSerialize(childContainer))
                    continue;
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.Write(dataRow, childContainer, jsonFilter);
                count++;
            }
        }


        public static void Parse(this JsonParser jsonParser, DataRow dataRow)
        {
            var model = dataRow.Model;
            if (model.IsExtRoot)
            {
                var ext = model.ExtraColumns;
                Debug.Assert(ext != null);
                jsonParser.Parse(ext, dataRow);
                return;
            }

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

            if (memberName == Projection.EXT_ROOT_NAME)
            {
                var ext = model.ExtraColumns;
                if (ext == null)
                    throw new FormatException(DiagnosticMessages.JsonParser_InvalidModelMember(memberName, model.GetType().FullName));
                jsonParser.Parse(ext, dataRow);
            }
            else
            {
                var member = model[memberName];
                if (member == null)
                    throw new FormatException(DiagnosticMessages.JsonParser_InvalidModelMember(memberName, model.GetType().FullName));
                if (member is Column)
                    jsonParser.Parse((Column)member, dataRow.Ordinal);
                else if (member is ColumnList)
                    jsonParser.Parse((ColumnList)member, dataRow.Ordinal);
                else
                    jsonParser.Parse(dataRow[(Model)member], false);
            }
        }

        private static void Parse(this JsonParser jsonParser, Projection columnCombination, DataRow dataRow)
        {
            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            var token = jsonParser.PeekToken();
            if (token.Kind == JsonTokenKind.String)
            {
                jsonParser.ConsumeToken();
                jsonParser.Parse(columnCombination, token.Text, dataRow);

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    token = jsonParser.ExpectToken(JsonTokenKind.String);
                    jsonParser.Parse(columnCombination, token.Text, dataRow);
                }
            }
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);
        }

        private static void Parse(this JsonParser jsonParser, Projection columnCombination, string memberName, DataRow dataRow)
        {
            jsonParser.ExpectToken(JsonTokenKind.Colon);
            if (columnCombination.ColumnsByRelativeName.ContainsKey(memberName))
                jsonParser.Parse(columnCombination.ColumnsByRelativeName[memberName], dataRow.Ordinal);
            else if (columnCombination.ChildrenByName.ContainsKey(memberName))
                jsonParser.Parse(columnCombination.ChildrenByName[memberName], dataRow);
            else
                throw new FormatException(DiagnosticMessages.JsonParser_InvalidColumnContainerMember(memberName, columnCombination.FullName));
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
