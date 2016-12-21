using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Windows.Utilities
{
    static partial class DictionaryExtensions
    {
        internal static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return (dictionary == null || dictionary.Count == 0) ? EmptyReadOnlyDictionary<TKey, TValue>.Singleton : new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }
    }
}
