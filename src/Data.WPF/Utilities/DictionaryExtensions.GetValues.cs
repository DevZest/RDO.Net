using System.Collections.Generic;

namespace DevZest.Data.Windows.Utilities
{
    static partial class DictionaryExtensions
    {
        internal static IReadOnlyList<TValue> GetValues<TKey, TValue>(this Dictionary<TKey, IReadOnlyList<TValue>> dictionary, TKey key)
        {
            if (dictionary == null)
                return Array<TValue>.Empty;

            IReadOnlyList<TValue> result;
            if (dictionary.TryGetValue(key, out result))
                return result;
            return Array<TValue>.Empty;
        }
    }
}
