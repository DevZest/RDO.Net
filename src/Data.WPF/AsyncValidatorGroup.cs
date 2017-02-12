using DevZest.Data.Windows.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public static class AsyncValidatorGroup
    {
        private sealed class EmptyGroup : IAsyncValidatorGroup
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

            public IAsyncValidatorGroup Add(AsyncValidator value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                return value;
            }

            public IEnumerator<AsyncValidator> GetEnumerator()
            {
                return EmptyEnumerator<AsyncValidator>.Singleton;
            }

            public IAsyncValidatorGroup Seal()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class ListGroup : IAsyncValidatorGroup
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

            public IAsyncValidatorGroup Seal()
            {
                _isSealed = true;
                return this;
            }

            public IAsyncValidatorGroup Add(AsyncValidator value)
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

        public static IAsyncValidatorGroup Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IAsyncValidatorGroup New(AsyncValidator value1, AsyncValidator value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IAsyncValidatorGroup New(params AsyncValidator[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length == 0)
                return Empty;

            IAsyncValidatorGroup result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), 0));
            return result;
        }

        private static T CheckNotNull<T>(this T reference, string paramName, int index)
            where T : class
        {
            if (reference == null)
                throw new NullReferenceException(String.Format("{0}[{1}]", paramName, index));
            return reference;
        }

        public static IAsyncValidatorGroup Where(this IAsyncValidatorGroup asyncValidators, Func<AsyncValidator, bool> predict)
        {
            if (asyncValidators == null)
                throw new ArgumentNullException(nameof(asyncValidators));

            var result = AsyncValidatorGroup.Empty;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                if (predict(asyncValidator))
                    result = result.Add(asyncValidator);
            }
            return result.Seal();
        }
    }
}
