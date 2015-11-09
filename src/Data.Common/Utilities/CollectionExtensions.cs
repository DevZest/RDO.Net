using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Utilities
{
    internal static class CollectionExtensions
    {
        internal static ReadOnlyCollection<T> Concat<T>(this ReadOnlyCollection<T> list1, ReadOnlyCollection<T> list2)
        {
            var result = new T[list1.Count + list2.Count];
            list1.CopyTo(result, 0);
            list2.CopyTo(result, list1.Count);
            return new ReadOnlyCollection<T>(result);
        }

        internal static IList<T> Append<T>(this IList<T> list, T value)
        {
            var result = new T[list.Count + 1];
            list.CopyTo(result, 0);
            result[list.Count] = value;
            return result;
        }

        internal static IList<ColumnMapping> GetParentMappings(this IList<ColumnMapping> columnMappings, IDbTable dbTable)
        {
            var parentMappings = dbTable.Model.ParentRelationship;
            if (parentMappings == null)
                return null;

            var result = new ColumnMapping[parentMappings.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var mapping = parentMappings[i];
                var source = GetSource(mapping.Source, columnMappings);
                if (source == null)
                    throw new InvalidOperationException(Strings.ChildColumnNotExistInColumnMappings(mapping.Source));
                result[i] = new ColumnMapping(source, mapping.TargetColumn);
            }

            return result;
        }

        private static DbExpression GetSource(DbExpression target, IList<ColumnMapping> mappings)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.Target == target)
                    return mapping.Source;
            }
            return null;
        }

        internal static bool ContainsTarget(this IList<ColumnMapping> columnMappings, Column target)
        {
            foreach (var mapping in columnMappings)
            {
                if (mapping.TargetColumn == target)
                    return true;
            }
            return false;
        }
    }
}
