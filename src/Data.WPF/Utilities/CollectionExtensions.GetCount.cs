using System.Collections.Generic;

namespace DevZest.Data.Windows.Utilities
{
    static partial class ListExtensions
    {
        internal static int GetCount<T>(this ICollection<T> collection)
        {
            return collection == null ? 0 : collection.Count;
        }
    }
}
