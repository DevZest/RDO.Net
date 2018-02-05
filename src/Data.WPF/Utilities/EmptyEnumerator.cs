using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest
{
    internal class EmptyEnumerator : IEnumerator
    {
        private static IEnumerator s_singleton;

        public static IEnumerator Singleton
        {
            get { return s_singleton ?? (s_singleton = new EmptyEnumerator()); }
        }

        private EmptyEnumerator()
        {
        }

        public object Current
        {
            get { throw new InvalidOperationException(); }
        }

        public void Reset()
        {
        }

        public bool MoveNext()
        {
            return false;
        }
    }

    internal sealed class EmptyEnumerator<T> : IEnumerator<T>
    {
        private static IEnumerator<T> s_singleton;

        public static IEnumerator<T> Singleton
        {
            get { return s_singleton ?? (s_singleton = new EmptyEnumerator<T>()); }
        }

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
