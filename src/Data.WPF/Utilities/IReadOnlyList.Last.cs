using System.Collections.Generic;

namespace DevZest
{
    static partial class Extensions
    {
        internal static T Last<T>(this IReadOnlyList<T> list)
        {
            var count = list.Count;
            return count == 0 ? default(T) : list[count - 1];
        }
    }
}
