using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Utilities
{
    internal class EmptyEnumerable<T> : IEnumerable<T>
    {
        internal static readonly EmptyEnumerable<T> Singleton = new EmptyEnumerable<T>();

        private EmptyEnumerable()
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return EmptyEnumerator<T>.Singleton;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(); ;
        }
    }
}
