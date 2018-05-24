using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Utilities
{
    internal sealed class EmptyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        public static readonly EmptyDictionary<TKey, TValue> Singleton = new EmptyDictionary<TKey, TValue>();

        private EmptyDictionary()
        {
        }

        public TValue this[TKey key]
        {
            get { throw new ArgumentOutOfRangeException(nameof(key)); }
        }

        public int Count
        {
            get { return 0; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return EmptyEnumerable<TKey>.Singleton;  }
        }

        public IEnumerable<TValue> Values
        {
            get { return EmptyEnumerable<TValue>.Singleton; }
        }

        public bool ContainsKey(TKey key)
        {
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return EmptyEnumerator<KeyValuePair<TKey, TValue>>.Singleton;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
