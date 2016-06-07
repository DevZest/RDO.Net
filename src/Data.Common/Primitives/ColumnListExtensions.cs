using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    internal static class ColumnListExtensions
    {
        internal static List<Column<T>> GetCounterpart<T>(this List<Column<T>> list, Model model)
        {
            var result = new List<Column<T>>();
            for (int i = 0; i < list.Count; i++)
                result.Add(list[i].GetCounterpart(model));

            return result;
        }
    }
}
