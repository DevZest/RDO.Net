using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public static class ValidationErrors
    {
        private class EmptyGroup : IValidationErrors
        {
            public static EmptyGroup Singleton = new EmptyGroup();
            private EmptyGroup()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public ValidationError this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IValidationErrors Add(ValidationError value)
            {
                Check.NotNull(value, nameof(value));
                return value;
            }

            public IValidationErrors Seal()
            {
                return this;
            }

            public IEnumerator<ValidationError> GetEnumerator()
            {
                return EmptyEnumerator<ValidationError>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<ValidationError>.Singleton;
            }
        }

        private class ListGroup : IValidationErrors
        {
            private bool _isSealed;
            private List<ValidationError> _list = new List<ValidationError>();

            public ListGroup(ValidationError value1, ValidationError value2)
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

            public ValidationError this[int index]
            {
                get { return _list[index]; }
            }

            public IValidationErrors Seal()
            {
                _isSealed = true;
                return this;
            }

            public IValidationErrors Add(ValidationError value)
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

            public IEnumerator<ValidationError> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IValidationErrors Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IValidationErrors New(ValidationError value1, ValidationError value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IValidationErrors New(params ValidationError[] values)
        {
            Check.NotNull(values, nameof(values));

            if (values.Length == 0)
                return Empty;

            IValidationErrors result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), i));
            return result;
        }
    }
}
