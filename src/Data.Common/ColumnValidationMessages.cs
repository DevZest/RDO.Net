using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public static class ColumnValidationMessages
    {
        private class EmptyGroup : IColumnValidationMessages
        {
            public static EmptyGroup Singleton = new EmptyGroup();
            private EmptyGroup()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public ColumnValidationMessage this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IColumnValidationMessages Add(ColumnValidationMessage value)
            {
                Check.NotNull(value, nameof(value));
                return value;
            }

            public IColumnValidationMessages Seal()
            {
                return this;
            }

            public IEnumerator<ColumnValidationMessage> GetEnumerator()
            {
                return EmptyEnumerator<ColumnValidationMessage>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<ColumnValidationMessage>.Singleton;
            }
        }

        private class ListGroup : IColumnValidationMessages
        {
            private bool _isSealed;
            private List<ColumnValidationMessage> _list = new List<ColumnValidationMessage>();

            public ListGroup(ColumnValidationMessage value1, ColumnValidationMessage value2)
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

            public ColumnValidationMessage this[int index]
            {
                get { return _list[index]; }
            }

            public IColumnValidationMessages Seal()
            {
                _isSealed = true;
                return this;
            }

            public IColumnValidationMessages Add(ColumnValidationMessage value)
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

            public IEnumerator<ColumnValidationMessage> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        public static IColumnValidationMessages Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IColumnValidationMessages New(ColumnValidationMessage value1, ColumnValidationMessage value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        public static IColumnValidationMessages New(params ColumnValidationMessage[] values)
        {
            Check.NotNull(values, nameof(values));

            if (values.Length == 0)
                return Empty;

            IColumnValidationMessages result = values[0].CheckNotNull(nameof(values), 0);
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values[i].CheckNotNull(nameof(values), i));
            return result;
        }
    }
}
