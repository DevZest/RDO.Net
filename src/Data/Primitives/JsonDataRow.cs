using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Provides extension methods to serialize/deserialize DataRow to/from JSON.
    /// </summary>
    public static class JsonDataRow
    {
        /// <summary>
        /// Writes JSON for specified DataRow.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/>.</param>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <returns>This <see cref="JsonWriter"/> for fluent coding.</returns>
        public static JsonWriter Write(this JsonWriter jsonWriter, DataRow dataRow)
        {
            return Write(jsonWriter, dataRow.DataSet, dataRow);
        }

        /// <summary>
        /// Writes JSON for specified DataRow and JsonView.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/>.</param>
        /// <param name="dataRow">The DataRow.</param>
        /// <param name="jsonView">The JsonView.</param>
        /// <returns>This <see cref="JsonWriter"/> for fluent coding.</returns>
        public static JsonWriter Write(this JsonWriter jsonWriter, DataRow dataRow, JsonView jsonView)
        {
            return Write(jsonWriter, jsonView, dataRow);
        }

        internal static JsonWriter Write(this JsonWriter jsonWriter, IJsonView jsonView, DataRow dataRow)
        {
            return jsonWriter.Write(jsonView, dataRow, dataRow.Model);
        }

        private static JsonWriter Write(this JsonWriter jsonWriter, IJsonView jsonView, DataRow dataRow, Model model)
        {
            jsonWriter.WriteStartObject();

            var jsonFilter = jsonView.Filter;
            var columns = model.Columns;
            int count = 0;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (!jsonWriter.IsSerializable(column))
                    continue;

                if (column.Kind == ColumnKind.ColumnListItem)
                    continue;

                if (jsonFilter != null && !jsonFilter.ShouldSerialize(column))
                    continue;

                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WritePropertyName(column.Name);
                jsonWriter.Write(dataRow, column);
                count++;
            }

            var columnLists = model.ColumnLists;
            for (int i = 0; i < columnLists.Count; i++)
            {
                var columnList = columnLists[i];
                if (jsonFilter != null && !jsonFilter.ShouldSerialize(columnList))
                    continue;

                if (count > 0)
                    jsonWriter.WriteComma();

                jsonWriter.Write(dataRow, columnList, jsonFilter);
                count++;
            }

            var projections = model.Projections;
            for (int i = 0; i < projections.Count; i++)
            {
                var projection = projections[i];
                if (jsonFilter == null || jsonFilter.ShouldSerialize(projection))
                {
                    if (count > 0)
                        jsonWriter.WriteComma();
                    Debug.Assert(!string.IsNullOrEmpty(projection.Name));
                    jsonWriter.WritePropertyName(projection.Name);
                    jsonWriter.Write(jsonView, dataRow, projection);
                    count++;
                }
            }

            if (string.IsNullOrEmpty(model.Namespace))  // for child models, make sure it's not projection.
            {
                var childDataSets = dataRow.ChildDataSets;
                for (int i = 0; i < childDataSets.Count; i++)
                {
                    var dataSet = childDataSets[i];
                    if (jsonFilter != null && !jsonFilter.ShouldSerialize(dataSet.Model))
                        continue;

                    if (count > 0)
                        jsonWriter.WriteComma();
                    var childJsonView = jsonView.GetChildView(dataSet);
                    jsonWriter.WritePropertyName(dataSet.Model.Name).InternalWrite(childJsonView, dataSet);
                    count++;
                }
            }

            return jsonWriter.WriteEndObject();
        }


        private static void Write(this JsonWriter jsonWriter, DataRow dataRow, ColumnList columnList, JsonFilter jsonFilter)
        {
            jsonWriter.WritePropertyName(columnList.Name);
            jsonWriter.WriteStartArray();
            var count = 0;
            for (int i = 0; i < columnList.Count; i++)
            {
                var column = columnList[i];
                if (jsonFilter != null && !jsonFilter.ShouldSerialize(column))
                    continue;
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.Write(dataRow, column);
                count++;
            }
            jsonWriter.WriteEndArray();
        }

        private static void Write(this JsonWriter jsonWriter, DataRow dataRow, Column column)
        {
            if (column is IDataSetColumn dataSetColumn)
                dataSetColumn.Serialize(dataRow.Ordinal, jsonWriter);
            else
                jsonWriter.WriteValue(jsonWriter.Serialize(column, dataRow.Ordinal));
        }

        /// <summary>
        /// Deserializes JSON into data values of DataRow.
        /// </summary>
        /// <param name="jsonReader">The JsonReader.</param>
        /// <param name="dataRow">The DataRow.</param>
        public static void Deserialize(this JsonReader jsonReader, DataRow dataRow)
        {
            jsonReader.Deserialize(dataRow, dataRow.Model);
        }

        private static void Deserialize(this JsonReader jsonReader, DataRow dataRow, Model model)
        {
            jsonReader.ExpectToken(JsonTokenKind.CurlyOpen);

            var token = jsonReader.PeekToken();
            if (token.Kind == JsonTokenKind.PropertyName)
            {
                jsonReader.ConsumeToken();
                jsonReader.Deserialize(dataRow, model, token.Text);

                while (jsonReader.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonReader.ConsumeToken();
                    token = jsonReader.ExpectToken(JsonTokenKind.PropertyName);
                    jsonReader.Deserialize(dataRow, model, token.Text);
                }
            }

            jsonReader.ExpectToken(JsonTokenKind.CurlyClose);
        }

        private static void Deserialize(this JsonReader jsonReader, DataRow dataRow, Model model, string memberName)
        {
            var member = model[memberName];
            if (member == null)
                throw new FormatException(DiagnosticMessages.JsonReader_InvalidModelMember(memberName, model.GetType().FullName));
            if (member is Column column)
                jsonReader.Deserialize(column, dataRow.Ordinal);
            else if (member is ColumnList columnList)
                jsonReader.Deserialize(columnList, dataRow.Ordinal);
            else if (member is Model projectionOrChildModel)
            {
                if (string.IsNullOrEmpty(projectionOrChildModel.Namespace)) // child model
                    jsonReader.Deserialize(dataRow[projectionOrChildModel], isTopLevel: false);
                else
                    jsonReader.Deserialize(dataRow, projectionOrChildModel);
            }
            else
                throw new FormatException(DiagnosticMessages.JsonReader_InvalidModelMember(memberName, model.GetType().FullName));
        }

        private static void Deserialize(this JsonReader jsonReader, Column column, int ordinal)
        {
            Debug.Assert(column != null);

            if (column is IDataSetColumn dataSetColumn)
            {
                var dataSet = jsonReader.Parse(() => dataSetColumn.NewValue(ordinal), false);
                if (jsonReader.IsDeserializable(column))
                    dataSetColumn.Deserialize(ordinal, dataSet);
                return;
            }

            var token = jsonReader.ExpectToken(JsonTokenKind.ColumnValues);
            if (jsonReader.IsDeserializable(column))
                jsonReader.Deserialize(column, ordinal, token.JsonValue);
        }

        private static void Deserialize(this JsonReader jsonReader, ColumnList columnList, int ordinal)
        {
            jsonReader.ExpectToken(JsonTokenKind.SquaredOpen);
            for (int i = 0; i < columnList.Count; i++)
            {
                jsonReader.Deserialize(columnList[i], ordinal);
                if (i < columnList.Count - 1)
                    jsonReader.ExpectComma();
            }
            jsonReader.ExpectToken(JsonTokenKind.SquaredClose);
        }
    }
}
