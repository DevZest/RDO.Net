using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    internal static class ColumnListExtensions
    {
        internal static List<T> GetCounterpart<T>(this List<T> list, Model model)
            where T : Column
        {
            var result = new List<T>();
            for (int i = 0; i < list.Count; i++)
                result.Add((T)list[i].GetCounterpart(model));

            return result;
        }

    }
}
