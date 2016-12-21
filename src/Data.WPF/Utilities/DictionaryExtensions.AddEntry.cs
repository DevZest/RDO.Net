using System.Collections.Generic;

namespace DevZest.Data.Windows.Utilities
{
    static partial class DictionaryExtensions
    {
        internal static Dictionary<TKey, TValue> AddEntry<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
                dictionary = new Dictionary<TKey, TValue>();
            dictionary.Add(key, value);
            return dictionary;
        }
    }
}
