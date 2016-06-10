using System.Text;

namespace DevZest.Data.Primitives
{
    internal static class ColumnJsonWriter
    {
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
    }
}
