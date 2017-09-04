using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public static class RowAsyncValidators
    {
        private sealed class EmptyGroup : IRowAsyncValidators
        {
            public readonly static EmptyGroup Singleton = new EmptyGroup();

            private EmptyGroup()
            {
            }

            public RowAsyncValidator this[int index]
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

            public IRowAsyncValidators Add(RowAsyncValidator value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                return value;
            }

            public IEnumerator<RowAsyncValidator> GetEnumerator()
            {
                return EmptyEnumerator<RowAsyncValidator>.Singleton;
            }

            public IRowAsyncValidators Seal()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class ListGroup : IRowAsyncValidators
        {
            private bool _isSealed;
            private List<RowAsyncValidator> _list = new List<RowAsyncValidator>();

            public ListGroup(RowAsyncValidator value1, RowAsyncValidator value2)
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

            public RowAsyncValidator this[int index]
            {
                get { return _list[index]; }
            }

            public IRowAsyncValidators Seal()
            {
                _isSealed = true;
                return this;
            }

            public IRowAsyncValidators Add(RowAsyncValidator value)
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

            public IEnumerator<RowAsyncValidator> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IRowAsyncValidators Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IRowAsyncValidators New(RowAsyncValidator value1, RowAsyncValidator value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IRowAsyncValidators New(params RowAsyncValidator[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length == 0)
                return Empty;

            IRowAsyncValidators result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), 0));
            return result;
        }

        public static IRowAsyncValidators Where(this IRowAsyncValidators asyncValidators, Func<RowAsyncValidator, bool> predict)
        {
            if (asyncValidators == null)
                throw new ArgumentNullException(nameof(asyncValidators));

            var result = RowAsyncValidators.Empty;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                if (predict(asyncValidator))
                    result = result.Add(asyncValidator);
            }
            return result.Seal();
        }

        public static void Each(this IRowAsyncValidators asyncValidators, Action<RowAsyncValidator> action)
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
