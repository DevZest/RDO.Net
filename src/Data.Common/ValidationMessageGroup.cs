using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public static class ValidationMessageGroup
    {
        private class EmptyGroup : IValidationMessageGroup
        {
            public static EmptyGroup Singleton = new EmptyGroup();
            private EmptyGroup()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public ValidationMessage this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            AbstractValidationMessage IReadOnlyList<AbstractValidationMessage>.this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public IValidationMessageGroup Add(ValidationMessage value)
            {
                Check.NotNull(value, nameof(value));
                return value;
            }

            public IValidationMessageGroup Seal()
            {
                return this;
            }

            public IEnumerator<ValidationMessage> GetEnumerator()
            {
                return EmptyEnumerator<ValidationMessage>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<ValidationMessage>.Singleton;
            }

            IAbstractValidationMessageGroup IAbstractValidationMessageGroup.Seal()
            {
                return this;
            }

            public IAbstractValidationMessageGroup Add(AbstractValidationMessage value)
            {
                Check.NotNull(value, nameof(value));
                return value;
            }

            IEnumerator<AbstractValidationMessage> IEnumerable<AbstractValidationMessage>.GetEnumerator()
            {
                return EmptyEnumerator<ValidationMessage>.Singleton;
            }
        }

        private class ListGroup : IValidationMessageGroup
        {
            private bool _isSealed;
            private List<ValidationMessage> _list = new List<ValidationMessage>();

            public ListGroup(ValidationMessage value1, ValidationMessage value2)
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

            AbstractValidationMessage IReadOnlyList<AbstractValidationMessage>.this[int index]
            {
                get { return _list[index]; }
            }

            public ValidationMessage this[int index]
            {
                get { return _list[index]; }
            }

            public IValidationMessageGroup Seal()
            {
                _isSealed = true;
                return this;
            }

            public IValidationMessageGroup Add(ValidationMessage value)
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

            public IEnumerator<ValidationMessage> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IAbstractValidationMessageGroup IAbstractValidationMessageGroup.Seal()
            {
                return Seal();
            }

            public IAbstractValidationMessageGroup Add(AbstractValidationMessage value)
            {
                Check.NotNull(value, nameof(value));

                if (!IsSealed && value is ValidationMessage)
                    return Add((ValidationMessage)value);

                var result = AbstractValidationMessageGroup.Empty;
                for (int i = 0; i < Count; i++)
                    result = result.Add(this[i]);
                return result.Add(value);
            }

            IEnumerator<AbstractValidationMessage> IEnumerable<AbstractValidationMessage>.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IValidationMessageGroup Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IValidationMessageGroup New(ValidationMessage value1, ValidationMessage value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IValidationMessageGroup New(params ValidationMessage[] values)
        {
            Check.NotNull(values, nameof(values));

            if (values.Length == 0)
                return Empty;

            IValidationMessageGroup result = values[0].CheckNotNull();
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull());
            return result;
        }
    }
}
