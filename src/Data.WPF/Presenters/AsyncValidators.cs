using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public static class AsyncValidators
    {
        private sealed class EmptyGroup : IAsyncValidators
        {
            public readonly static EmptyGroup Singleton = new EmptyGroup();

            private EmptyGroup()
            {
            }

            public AsyncValidator this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IAsyncValidators Add(AsyncValidator value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                return value;
            }

            public IEnumerator<AsyncValidator> GetEnumerator()
            {
                return EmptyEnumerator<AsyncValidator>.Singleton;
            }

            public IAsyncValidators Seal()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class ListGroup : IAsyncValidators
        {
            private bool _isSealed;
            private List<AsyncValidator> _list = new List<AsyncValidator>();

            public ListGroup(AsyncValidator value1, AsyncValidator value2)
            {
                Debug.Assert(value1 != null && value2 != null);
                Add(value1);
                Add(value2);
            }

            private ListGroup()
            {
            }

            public bool IsSealed
            {
                get { return _isSealed; }
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public AsyncValidator this[int index]
            {
                get { return _list[index]; }
            }

            public IAsyncValidators Seal()
            {
                _isSealed = true;
                return this;
            }

            public IAsyncValidators Add(AsyncValidator value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (!IsSealed)
                {
                    _list.Add(value);
                    return this;
                }

                Debug.Assert(Count > 0);
                var result = new ListGroup();
                for (int i = 0; i < Count; i++)
                    result.Add(this[i]);
                result.Add(value);
                return result;
            }

            public IEnumerator<AsyncValidator> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IAsyncValidators Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IAsyncValidators New(AsyncValidator value1, AsyncValidator value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IAsyncValidators New(params AsyncValidator[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length == 0)
                return Empty;

            IAsyncValidators result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), 0));
            return result;
        }

        public static IAsyncValidators Where(this IAsyncValidators asyncValidators, Func<AsyncValidator, bool> predict)
        {
            if (asyncValidators == null)
                throw new ArgumentNullException(nameof(asyncValidators));

            var result = AsyncValidators.Empty;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                if (predict(asyncValidator))
                    result = result.Add(asyncValidator);
            }
            return result.Seal();
        }

        public static void Each(this IAsyncValidators asyncValidators, Action<AsyncValidator> action)
        {
            if (asyncValidators == null)
                throw new ArgumentNullException(nameof(asyncValidators));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            for (int i = 0; i < asyncValidators.Count; i++)
                action(asyncValidators[i]);
        }
    }
}
