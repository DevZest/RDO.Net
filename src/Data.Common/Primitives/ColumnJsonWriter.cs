using System.Collections.Generic;
using System.Text;

namespace DevZest.Data.Primitives
{
    internal static class ColumnJsonWriter
    {
        internal static StringBuilder WriteColumn(this StringBuilder stringBuilder, Column column)
        {
            var converter = ColumnConverter.Get(column);
            converter.WriteJson(stringBuilder, column);
            return stringBuilder;
        }

        internal static StringBuilder WriteNameColumnPair<T>(this StringBuilder stringBuilder, string name, Column<T> column)
        {
            stringBuilder.WriteObjectName(name);
            column.WriteJson(stringBuilder);
            return stringBuilder;
        }

        internal static StringBuilder WriteColumnProperties(this StringBuilder stringBuilder, ColumnConverter converter, object obj)
        {
            converter.WritePropertiesJson(stringBuilder, obj);
            return stringBuilder;
        }

        internal static StringBuilder WriteExpression<T>(this StringBuilder stringBuilder, ColumnExpression<T> expression)
        {
            var converter = ColumnConverter.Get(expression);
            converter.WriteJson(stringBuilder, expression);
            return stringBuilder;
        }

        internal static StringBuilder WriteNameColumnsPair<T>(this StringBuilder stringBuilder, string name, IReadOnlyList<T> columns)
            where T : Column
        {
            return stringBuilder.WriteObjectName(name).WriteStartArray().WriteColumns(columns).WriteEndArray();
        }

        private static StringBuilder WriteColumns<T>(this StringBuilder stringBuilder, IReadOnlyList<T> columns)
            where T : Column
        {
            for (int i = 0; i < columns.Count; i++)
            {
                stringBuilder.WriteColumn(columns[i]);
                if (i < columns.Count - 1)
                    stringBuilder.WriteComma();
            }
            return stringBuilder;
        }
    }
}
