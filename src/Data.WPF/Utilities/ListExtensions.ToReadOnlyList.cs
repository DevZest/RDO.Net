using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Windows.Utilities
{
    static partial class ListExtensions
    {
        internal static IReadOnlyList<T> ToReadOnlyList<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                return Array<T>.Empty;
            else
                return new ReadOnlyCollection<T>(list);
        }
    }
}
