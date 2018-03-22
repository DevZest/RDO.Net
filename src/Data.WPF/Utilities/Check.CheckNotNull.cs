using System;
using System.Collections.Generic;

namespace DevZest
{
    internal static partial class Check
    {
        internal static void CheckNotNull<T>(this IReadOnlyList<T> list, string listParamName)
            where T : class
        {
            Check.NotNull(list, listParamName);
            for (int i = 0; i < list.Count; i++)
                list[i].CheckNotNull(listParamName, i);
        }

        internal static T CheckNotNull<T>(this T reference, string paramName, int index)
            where T : class
        {
            if (reference == null)
                throw new ArgumentNullException(String.Format("{0}[{1}]", paramName, index));
            return reference;
        }
    }
}
