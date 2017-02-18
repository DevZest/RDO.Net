using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest
{
    internal sealed class EmptyEnumerator<T> : IEnumerator<T>
    {
        public static EmptyEnumerator<T> Singleton = new EmptyEnumerator<T>();

        private EmptyEnumerator()
        {
        }

        object IEnumerator.Current
        {
            get { throw new InvalidOperationException(); }
        }

        T IEnumerator<T>.Current
        {
            get { throw new InvalidOperationException(); }
        }

        void IDisposable.Dispose()
        {
        }

        bool IEnumerator.MoveNext()
        {
            return false;
        }

        void IEnumerator.Reset()
        {
        }
    }
}
