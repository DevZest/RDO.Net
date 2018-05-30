using System.Collections.Generic;

namespace DevZest
{
    internal static class CollectionExtensions
    {
        internal static IReadOnlyList<T> Concat<T>(this IReadOnlyList<T> list1, IReadOnlyList<T> list2)
        {
            var result = new T[list1.Count + list2.Count];
            list1.CopyTo(result, 0);
            list2.CopyTo(result, list1.Count);
            return result;
        }

        private static void CopyTo<T>(this IReadOnlyList<T> list, T[] array, int startIndex)
        {
            for (int i = 0; i < list.Count; i++)
                array[startIndex + i] = list[i];
        }

        internal static IReadOnlyList<T> Append<T>(this IReadOnlyList<T> list, T value)
        {
            var result = new T[list.Count + 1];
            list.CopyTo(result, 0);
            result[list.Count] = value;
            return result;
        }

        internal static List<T> Append<T>(this List<T> result, IReadOnlyList<T> items)
        {
            if (items == null || items.Count == 0)
                return result;

            if (result == null)
                result = new List<T>();
            result.AddRange(items);
            return result;
        }
    }
}
