using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public static class ScalarValidationMessages
    {
        private class EmptyGroup : IScalarValidationMessages
        {
            public static EmptyGroup Singleton = new EmptyGroup();
            private EmptyGroup()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public ScalarValidationMessage this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IScalarValidationMessages Add(ScalarValidationMessage value)
            {
                Check.NotNull(value, nameof(value));
                return value;
            }

            public IScalarValidationMessages Seal()
            {
                return this;
            }

            public IEnumerator<ScalarValidationMessage> GetEnumerator()
            {
                return EmptyEnumerator<ScalarValidationMessage>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<ScalarValidationMessage>.Singleton;
            }
        }

        private class ListGroup : IScalarValidationMessages
        {
            private bool _isSealed;
            private List<ScalarValidationMessage> _list = new List<ScalarValidationMessage>();

            public ListGroup(ScalarValidationMessage value1, ScalarValidationMessage value2)
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

            public ScalarValidationMessage this[int index]
            {
                get { return _list[index]; }
            }

            public IScalarValidationMessages Seal()
            {
                _isSealed = true;
                return this;
            }

            public IScalarValidationMessages Add(ScalarValidationMessage value)
            {
                Check.NotNull(value, nameof(value));

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

            public IEnumerator<ScalarValidationMessage> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IScalarValidationMessages Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IScalarValidationMessages New(ScalarValidationMessage value1, ScalarValidationMessage value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IScalarValidationMessages New(params ScalarValidationMessage[] values)
        {
            Check.NotNull(values, nameof(values));

            if (values.Length == 0)
                return Empty;

            IScalarValidationMessages result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), i));
            return result;
        }
    }
}
