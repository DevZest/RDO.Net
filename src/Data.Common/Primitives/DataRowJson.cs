using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class DataRowJson
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
    }
}
