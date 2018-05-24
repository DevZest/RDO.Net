using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Utilities
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

        internal static bool ContainsSource(this IReadOnlyList<ColumnMapping> columnMappings, Column source)
        {
            foreach (var mapping in columnMappings)
            {
                if (mapping.Source == source)
                    return true;
            }
            return false;
        }

        internal static List<T> TranslateToColumns<T>(this List<T> columns, Model model)
            where T : Column
        {
            if (columns == null)
                return null;

            List<T> result = null;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var translated = column.TranslateTo(model);
                if (result != null)
                    result.Add(translated);
                else if (translated != column)
                {
                    if (result == null)
                    {
                        result = new List<T>();
                        for (int j = 0; j < i; j++)
                            result.Add(columns[j]);
                    }
                    result.Add(translated);
                }
            }
            return result ?? columns;
        }

        internal static T[] TranslateToParams<T>(this ReadOnlyCollection<T> parameters, Model model)
            where T : Column
        {
            if (parameters == null)
                return null;

            T[] result = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                var column = parameters[i];
                var translated = column.TranslateTo(model);
                if (result != null)
                    result[i] = translated;
                else if (translated != column)
                {
                    if (result == null)
                    {
                        result = new T[parameters.Count];
                        for (int j = 0; j < i; j++)
                            result[j] = parameters[j];
                    }
                    result[i] = translated;
                }
            }
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
