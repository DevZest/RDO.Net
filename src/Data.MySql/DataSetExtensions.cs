using DevZest.Data.Primitives;
using System.Collections.Generic;

namespace DevZest.Data.MySql
{
    internal static class DataSetExtensions
    {
        public static string ForJson(this DataSet dataSet, string ordinalColumnName, bool isPretty)
        {
            var jsonWriter = JsonWriter.Create(null);
            jsonWriter.WriteStartArray();
            var columns = dataSet.Model.GetColumns();
            for (int i = 0; i < dataSet.Count; i++)
            {
                if (i > 0)
                    jsonWriter.WriteComma();
                WriteJson(jsonWriter, columns, dataSet[i], ordinalColumnName);
            }
            jsonWriter.WriteEndArray();
            return jsonWriter.ToString(isPretty);
        }

        private static void WriteJson(JsonWriter jsonWriter, IReadOnlyList<Column> columns, DataRow dataRow, string ordinalColumnName)
        {
            jsonWriter.WriteStartObject();

            int count = 0;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (typeof(DataSet).IsAssignableFrom(column.DataType))
                    continue;
                if (column.IsNull(dataRow))
                    continue;
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WritePropertyName(column.Name);
                if (column is _DateTime dateTimeColumn)
                    jsonWriter.WriteValue(JsonValue.DateTime(dateTimeColumn[dataRow], "yyyy-MM-ddTHH:mm:ss.FFFFFF"));
                else
                    jsonWriter.WriteValue(column.Serialize(dataRow.Ordinal));
                count++;
            }

            if (!string.IsNullOrEmpty(ordinalColumnName))
            {
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.WritePropertyName(ordinalColumnName);
                jsonWriter.WriteValue(JsonValue.Number(dataRow.Ordinal));
            }

            jsonWriter.WriteEndObject();
        }
    }
}
