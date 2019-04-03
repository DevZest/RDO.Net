using System;
using System.Collections.Generic;

namespace DevZest
{
    static partial class Extension
    {
        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }
    }
}
