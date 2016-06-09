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
    }
}
