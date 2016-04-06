using System;
using System.Collections.Generic;

namespace DevZest
{
    static partial class Extension
    {
        public static void Sort<T>(this IList<T> list, Func<T, T, int> comparer)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (comparer(list[i], list[j]) < 0)
                    {
                        // swap list[i] and list[j]
                        var temp = list[i];
                        list[i] = list[j];
                        list[j] = temp;
                    }
                }
            }
        }
    }
}
