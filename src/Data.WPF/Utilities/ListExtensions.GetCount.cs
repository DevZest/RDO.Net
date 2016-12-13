using System.Collections.Generic;

namespace DevZest.Data.Windows.Utilities
{
    static partial class ListExtensions
    {
        internal static int GetCount<T>(this List<T> list)
        {
            return list == null ? 0 : list.Count;
        }
    }
}
