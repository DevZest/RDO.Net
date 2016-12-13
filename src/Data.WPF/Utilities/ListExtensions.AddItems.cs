using System.Collections.Generic;

namespace DevZest.Data.Windows.Utilities
{
    static partial class ListExtensions
    {
        internal static List<T> AddItems<T>(this List<T> list, IReadOnlyList<T> items)
        {
            if (items == null || items.Count == 0)
                return list;

            if (list == null)
                list = new List<T>();
            list.AddRange(items);

            return list;
        }
    }
}
