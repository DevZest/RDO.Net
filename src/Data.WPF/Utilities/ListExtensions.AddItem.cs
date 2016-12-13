using System.Collections.Generic;

namespace DevZest.Data.Windows.Utilities
{
    static partial class ListExtensions
    {
        internal static List<T> AddItem<T>(this List<T> list, T item)
        {
            if (list == null)
                list = new List<T>();
            list.Add(item);
            return list;
        }
    }
}
