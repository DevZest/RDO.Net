using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows.Utilities
{
    internal static class CachedList
    {
        public static T GetOrCreate<T>(ref List<T> cachedList, Func<T> constructor)
            where T : class
        {
            Debug.Assert(constructor != null);

            if (cachedList == null || cachedList.Count == 0)
                return constructor();

            var last = cachedList.Count - 1;
            var result = cachedList[last];
            cachedList.RemoveAt(last);
            return result;
        }

        public static void Recycle<T>(ref List<T> cachedList, T value)
            where T : class
        {
            Debug.Assert(value != null);

            if (cachedList == null)
                cachedList = new List<T>();
            cachedList.Add(value);
        }
    }
}
