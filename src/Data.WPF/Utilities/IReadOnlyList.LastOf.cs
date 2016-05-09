using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest
{
    static partial class Extensions
    {
        internal static T LastOf<T>(this IReadOnlyList<T> list, int count = 1)
        {
            Debug.Assert(count > 0);
            var listCount = list.Count;
            return listCount == 0 ? default(T) : list[listCount - count];
        }
    }
}
