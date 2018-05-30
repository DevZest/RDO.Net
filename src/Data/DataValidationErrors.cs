using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public static class DataValidationErrors
    {
        private class EmptyGroup : IDataValidationErrors
        {
            public static EmptyGroup Singleton = new EmptyGroup();
            private EmptyGroup()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public DataValidationError this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IDataValidationErrors Add(DataValidationError value)
            {
                value.VerifyNotNull(nameof(value));
                return value;
            }

            public IDataValidationErrors Seal()
            {
                return this;
            }

            public IEnumerator<DataValidationError> GetEnumerator()
            {
                return EmptyEnumerator<DataValidationError>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<DataValidationError>.Singleton;
            }
        }

        private class ListGroup : IDataValidationErrors
        {
            private bool _isSealed;
            private List<DataValidationError> _list = new List<DataValidationError>();

            public ListGroup(DataValidationError value1, DataValidationError value2)
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

            public DataValidationError this[int index]
            {
                get { return _list[index]; }
            }

            public IDataValidationErrors Seal()
            {
                _isSealed = true;
                return this;
            }

            public IDataValidationErrors Add(DataValidationError value)
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

            public IEnumerator<DataValidationError> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IDataValidationErrors Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IDataValidationErrors New(DataValidationError value1, DataValidationError value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IDataValidationErrors New(params DataValidationError[] values)
        {
            values.VerifyNotNull(nameof(values));

            if (values.Length == 0)
                return Empty;

            IDataValidationErrors result = values.VerifyNotNull(0, nameof(values));
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values.VerifyNotNull(i, nameof(values)));
            return result;
        }
    }
}
