using System.Text;

namespace DevZest.Data.Primitives
{
    internal static class DataSetJsonWriter
    {
        internal static StringBuilder WriteDataSet(this StringBuilder stringBuilder, DataSet dataSet)
        {
            stringBuilder.Append('[');
            int count = 0;
            foreach (var dataRow in dataSet)
            {
                if (count > 0)
                    stringBuilder.WriteComma();
                stringBuilder.WriteDataRow(dataRow);
                count++;
            }
            stringBuilder.Append(']');

            return stringBuilder;
        }

        private static void WriteDataRow(this StringBuilder stringBuilder, DataRow dataRow)
        {
            stringBuilder.WriteStartObject();

            var columns = dataRow.Model.Columns;
            int count = 0;
            foreach (var column in columns)
            {
                if (!column.ShouldSerialize)
                    continue;

                if (count > 0)
                    stringBuilder.Append(',');
                stringBuilder.WriteObjectName(column.Name);
                var dataSetColumn = column as IDataSetColumn;
                if (dataSetColumn != null)
                    dataSetColumn.Serialize(dataRow.Ordinal, stringBuilder);
                else
                    stringBuilder.WriteValue(column.Serialize(dataRow.Ordinal));
                count++;
            }

            foreach (var dataSet in dataRow.ChildDataSets)
            {
                if (count > 0)
                    stringBuilder.WriteComma();
                stringBuilder.WriteObjectName(dataSet.Model.Name).WriteDataSet(dataSet);
                count++;
            }

            stringBuilder.WriteEndObject();
        }
    }
}
