using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public static class AbstractValidationMessageGroup
    {
        private class EmptyGroup : IAbstractValidationMessageGroup
        {
            public static EmptyGroup Singleton = new EmptyGroup();
            private EmptyGroup()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public AbstractValidationMessage this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IAbstractValidationMessageGroup Add(AbstractValidationMessage value)
            {
                Check.NotNull(value, nameof(value));
                return value;
            }

            public IAbstractValidationMessageGroup Seal()
            {
                return this;
            }

            public IEnumerator<AbstractValidationMessage> GetEnumerator()
            {
                return EmptyEnumerator<ValidationMessage>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<ValidationMessage>.Singleton;
            }
        }

        private class ListGroup : IAbstractValidationMessageGroup
        {
            private bool _isSealed;
            private List<AbstractValidationMessage> _list = new List<AbstractValidationMessage>();

            public ListGroup(AbstractValidationMessage value1, AbstractValidationMessage value2)
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

            public AbstractValidationMessage this[int index]
            {
                get { return _list[index]; }
            }

            public IAbstractValidationMessageGroup Seal()
            {
                _isSealed = true;
                return this;
            }

            public IAbstractValidationMessageGroup Add(AbstractValidationMessage value)
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
                    foreach (var column in this)
                        result.Add(column);
                    result.Add(value);
                    return result;
                }
            }
            
            public IEnumerator<AbstractValidationMessage> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IAbstractValidationMessageGroup Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IAbstractValidationMessageGroup New(AbstractValidationMessage value1, AbstractValidationMessage value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IAbstractValidationMessageGroup New(params AbstractValidationMessage[] values)
        {
            Check.NotNull(values, nameof(values));

            if (values.Length == 0)
                return Empty;

            IAbstractValidationMessageGroup result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), i));
            return result;
        }
    }
}
