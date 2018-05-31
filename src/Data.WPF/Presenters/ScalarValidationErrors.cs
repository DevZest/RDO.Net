using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public static class ScalarValidationErrors
    {
        private class EmptyGroup : IScalarValidationErrors
        {
            public static EmptyGroup Singleton = new EmptyGroup();
            private EmptyGroup()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public ScalarValidationError this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IScalarValidationErrors Add(ScalarValidationError value)
            {
                return value.VerifyNotNull(nameof(value));
            }

            public IScalarValidationErrors Seal()
            {
                return this;
            }

            public IEnumerator<ScalarValidationError> GetEnumerator()
            {
                return EmptyEnumerator<ScalarValidationError>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<ScalarValidationError>.Singleton;
            }
        }

        private class ListGroup : IScalarValidationErrors
        {
            private bool _isSealed;
            private List<ScalarValidationError> _list = new List<ScalarValidationError>();

            public ListGroup(ScalarValidationError value1, ScalarValidationError value2)
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

            public ScalarValidationError this[int index]
            {
                get { return _list[index]; }
            }

            public IScalarValidationErrors Seal()
            {
                _isSealed = true;
                return this;
            }

            public IScalarValidationErrors Add(ScalarValidationError value)
            {
                value.VerifyNotNull(nameof(value));

                if (!IsSealed)
                {
                    _list.Add(value);
                    return this;
                }

                if (Count == 0)
                    return value;
                else
                {
                    var result = new ListGroup();
                    for (int i = 0; i < Count; i++)
                        result.Add(this[i]);
                    result.Add(value);
                    return result;
                }
            }

            public IEnumerator<ScalarValidationError> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IScalarValidationErrors Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IScalarValidationErrors New(ScalarValidationError value1, ScalarValidationError value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IScalarValidationErrors New(params ScalarValidationError[] values)
        {
            values.VerifyNotNull(nameof(values));

            if (values.Length == 0)
                return Empty;

            IScalarValidationErrors result = values.VerifyNotNull(0, nameof(values));
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values.VerifyNotNull(i, nameof(values)));
            return result;
        }

        public static IScalarValidationErrors Add(this IScalarValidationErrors result, IScalarValidationErrors messages)
        {
            messages.VerifyNotNull(nameof(messages));

            for (int i = 0; i < messages.Count; i++)
                result = result.Add(messages.VerifyNotNull(i, nameof(messages)));

            return result;
        }
    }
}
